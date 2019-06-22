// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Outline/Transparent" {
	Properties{
		_Color("Main Color", Color) = (.5,.5,.5,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.15)) = .005
		_OutlineOffset("Outline Offset", Vector) = (0, 0, 0)
	    _Alpha("Alpha", Float) = 1
	}

		CGINCLUDE
#include "UnityCG.cginc"

		struct appdata
	{
		half4 vertex : POSITION;
		half3 normal : NORMAL;
	};

	struct v2f
	{
		half4 pos : POSITION;
		half3 normalDir : NORMAL;
	};

	uniform half4 _Color;
	uniform half _Outline;
	uniform half4 _OutlineColor;

	ENDCG

		SubShader{
		Tags{"Queue" = "Transparent+10" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

		Pass{
		Name "STENCIL"
		ZWrite Off
		ZTest Always
		ColorMask 0

		Stencil{
		Ref 2
		Comp always
		Pass replace
		ZFail decrWrap
	}

		CGPROGRAM

#pragma vertex vert2
#pragma fragment frag

		v2f vert2(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
    o.normalDir = o.pos.xyz;
		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		return _Color;
	}

		ENDCG


	}

		Pass{
		Name "OUTLINE"
		Tags{"LightMode" = "Always"}
		Cull Off
		ZWrite Off
		ColorMask RGB

		Blend SrcAlpha OneMinusSrcAlpha

		Stencil{
		Ref 2
		Comp NotEqual
		Pass replace
		ZFail decrWrap
	}

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

		half3 _OutlineOffset;

	v2f vert(appdata v)
	{
		v2f o;

		half3 vertex = v.vertex.xyz;
		//vertex -= _OutlineOffset;
    // Little cheat, since unity objects are centered
    half3 off = normalize(vertex);
		vertex.x += off.x * _Outline;
		vertex.y += off.y * _Outline;
		vertex.z += off.z * _Outline;
		//vertex += _OutlineOffset;
		o.pos = UnityObjectToClipPos(half4(vertex, v.vertex.w));
    o.normalDir = v.normal;
		return o;
	}

	half _Alpha;
	half4 frag(v2f i) :COLOR{
		return half4(_OutlineColor.rgb, 1);
	}
		ENDCG
	}


		Pass{
		Name "BASE"
		ZWrite On
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
    Offset -1,-1

		CGPROGRAM

#pragma vertex vert2
#pragma fragment frag

		v2f vert2(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.normalDir = normalize(mul(half4(v.normal, 0), unity_WorldToObject).xyz);

		return o;
	}

	uniform half4 _LightColor0;
	half _Alpha;

	half4 frag(v2f i) : COLOR
	{
		half3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
		half diffuse = max(0.4, dot(i.normalDir, lightDirection));

		half3 color = diffuse * _LightColor0.rgb * _Color.rgb;
        return half4(color, _Color.a);
	}

		ENDCG
	}

	}

		FallBack "VertexLit"    // Use VertexLit's shadow caster/receiver passes.
}