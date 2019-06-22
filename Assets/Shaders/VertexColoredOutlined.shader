﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Custom/VertexColoredOutlined" {
	Properties{
		_Color("Main Color", Color) = (.5,.5,.5,1)
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.15)) = .005
	}

		CGINCLUDE
#include "UnityCG.cginc"

		struct appdata
	{
		half4 vertex : POSITION;
		half4 color : COLOR;
		half3 normal : NORMAL;
		half2 texcoord : TEXCOORD0;
	};

	struct v2f
	{
		half4 pos : POSITION;
		half4 color : COLOR;
		half2 uv : TEXCOORD0;
		half3 normalDir : NORMAL;
	};

	uniform half4 _Color;
	uniform half _Outline;
	uniform half4 _OutlineColor;

	ENDCG

		SubShader{
		Tags{"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}

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
    o.color = _Color;
    o.uv = half2(0,0);
    o.normalDir = half3(0,0,0);
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

    half3 vertex
    = v.vertex.xyz;
    //vertex -= _OutlineOffset;
      half3 offset = v.color.rgb;
		vertex.x += offset.x * _Outline;
		vertex.y += offset.y * _Outline;
		vertex.z += offset.z * _Outline;
		//vertex += _OutlineOffset;
		o.pos = UnityObjectToClipPos(half4(vertex, v.vertex.w));
    o.color = v.color;
    o.normalDir = offset;
    o.uv = v.texcoord;
		return o;
	}

	half4 frag(v2f i) :COLOR {
		return half4(_OutlineColor.rgb, i.color.a);
	}
		ENDCG
	}

		Pass{
		Name "BASE"
		ZWrite On
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
    Offset -1,-1

		CGPROGRAM

#pragma vertex vert2
#pragma fragment frag

		v2f vert2(appdata v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
		o.normalDir = normalize(mul(half4(v.normal, 0), unity_WorldToObject).xyz);
    o.color = v.color;
		return o;
	}

	uniform half4 _LightColor0;

	half4 frag(v2f i) : COLOR
	{
		half4 c = half4(_Color.rgb,i.color.a);
		//return c;

		half3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
		half diffuse = max(0.4, dot(i.normalDir, lightDirection));

		half3 color = diffuse * _LightColor0.rgb * c.rgb;
		return half4(color, c.a);
	}

		ENDCG
	}

	}

		FallBack "VertexLit"    // Use VertexLit's shadow caster/receiver passes.
}