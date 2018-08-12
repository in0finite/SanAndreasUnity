//////////////////////////////////////////////////////
// MK Glow Free Blur Shader    						//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de | www.michaelkremmel.store //
// Copyright © 2017 All rights reserved.            //
//////////////////////////////////////////////////////
Shader "Hidden/MK/Glow/Blur"
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "" {}
	}

	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2fBlur 
	{
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;

		float4 uv01 : TEXCOORD1;
		float4 uv23 : TEXCOORD2;
		float4 uv45 : TEXCOORD3;
	};
	
	uniform half _Offset;
	uniform sampler2D _MainTex;
	uniform float4 _MainTex_ST;
	uniform float2 _MainTex_TexelSize;
	uniform half _VRMult;

	ENDCG
	
	SubShader 
	{
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			Blend One Zero

			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vertBlur
			#pragma fragment fragBlur
			#pragma target 2.0

			#define V 1

			#define _MK_HQ_BLUR 1

			#include "MKGlowBlurInc.cginc"
			ENDCG
		}
		Pass 
		{
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			Blend One One

			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vertBlur
			#pragma fragment fragBlur
			#pragma target 2.0

			#define _MK_HQ_BLUR 1

			#include "MKGlowBlurInc.cginc"
			ENDCG
		}
	}
FallBack Off
}
