// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Outline/Opaque" {
	Properties{
    _MainTex("Base Texture (RGB)", 2D) = "white" {}
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.15)) = .005
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

	uniform half _Outline;
	uniform half4 _OutlineColor;

	ENDCG

		SubShader{
		Tags{"Queue" = "Geometry"}

		Pass{
		Name "STENCIL"
		ZWrite On
		ZTest LEqual
		ColorMask 0

		Stencil{
		Ref 2
		Comp always
		Pass keep
		ZFail decrWrap
	}

		CGPROGRAM

#pragma vertex vert2
#pragma fragment frag

		v2f vert2(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
    o.normalDir = half3(0,0,0);
		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		return half4(0.5,0.5,0.5,1.0);
	}

		ENDCG


	}

		Pass{
		Name "OUTLINE"
		Tags{"LightMode" = "Always"}
		Cull Front
		ZWrite On
    ZTest LEqual
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
		vertex.x += v.normal.x * _Outline;
		vertex.y += v.normal.y * _Outline;
		vertex.z += v.normal.z * _Outline;
		//vertex += _OutlineOffset;
		o.pos = UnityObjectToClipPos(half4(vertex, v.vertex.w));
    o.normalDir = v.normal;
		return o;
	}

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
		Material{
		Diffuse(1,1,1,1)
		Ambient(1,1,1,1)
	}
		Lighting On
		SetTexture[_MainTex]{
		Combine previous * texture
	}
	}

	}

		FallBack "VertexLit"    // Use VertexLit's shadow caster/receiver passes.
}