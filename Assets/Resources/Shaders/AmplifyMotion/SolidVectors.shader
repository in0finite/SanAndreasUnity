// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Motion/SolidVectors"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.25
	}

	CGINCLUDE
		#pragma vertex MotionVertex
		#pragma fragment MotionFragment
		#pragma fragmentoption ARB_precision_hint_fastest
		#include "Shared.cginc"
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Blend Off Cull Off Fog { Mode off }
		ZTest LEqual ZWrite On

		// PACKED
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_MOBILE
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_MOBILE
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}
	}

	FallBack Off
}
