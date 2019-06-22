Shader "Custom/Texture Only (With Grid)"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset] _Grid ("Texture", 2D) = "white" {}
        _OffsetX("Tile Scale X", Range(1, 10000)) = 782
        _OffsetY("Tile Scale Y", Range(1, 10000)) = 782
    }
    SubShader
    {
        Pass
        {
            Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            // compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

            float _OffsetX;
            float _OffsetY;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(1) // put shadows data into TEXCOORD1
                float2 uvGrid : TEXCOORD2;
                fixed3 diff : COLOR0;
                float4 pos : SV_POSITION;
            };
            v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0.rgb;
                // compute shadows data
                TRANSFER_SHADOW(o)
                o.uvGrid = v.vertex.xz + float2(_OffsetX, _OffsetY);
                return o;
            }

            sampler2D _MainTex;
            sampler2D _Grid;
            

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col100 = tex2D(_Grid, i.uvGrid / 100.0);
                fixed4 col10 = tex2D(_Grid, i.uvGrid / 10.0);
                fixed4 col1 = tex2D(_Grid, i.uvGrid / 1.0);
                if (abs(i.uvGrid.x) <= 80 && abs(i.uvGrid.y) <= 80) {
                    col10 = (1.0 - col10.a) * col10 + col10.a * fixed4(1,1,0,1);
                }
                if (abs(i.uvGrid.x) <= 80 && abs(i.uvGrid.y) <= 1) {
                    col100 = (1.0 - col100.a) * col100 + col100.a * fixed4(1,0,0,1);
                }
                if (abs(i.uvGrid.y) <= 80 && abs(i.uvGrid.x) <= 1) {
                    col100 = (1.0 - col100.a) * col100 + col100.a * fixed4(0,0,1,1);
                }
                col = (1.0 - col1.a) * col + col1.a * col1;
                col = (1.0 - col10.a) * col + col10.a * col10;
                col = (1.0 - col100.a) * col + col100.a * col100;
                // compute shadow attenuation (1.0 = fully lit, 0.0 = fully shadowed)
                fixed shadow = SHADOW_ATTENUATION(i);
                // darken light's illumination with shadow, keep ambient intact
                fixed3 lighting = i.diff * shadow;
                col.rgb *= lighting;
                return col;
            }
            ENDCG
        }

        // shadow casting support
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
}