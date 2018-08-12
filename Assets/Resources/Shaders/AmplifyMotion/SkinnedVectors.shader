// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Motion/SkinnedVectors"
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

		// PACKED - CPU path
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_DEFORM
				#pragma multi_compile AM_MOBILE
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_DEFORM
				#pragma multi_compile AM_MOBILE
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_DEFORM
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_DEFORM
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}

		// PACKED - GPU Path
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_DEFORM_GPU
				#pragma multi_compile AM_MOBILE
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_DEFORM_GPU
				#pragma multi_compile AM_MOBILE
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_DEFORM_GPU
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_PACKED
				#pragma multi_compile AM_DEFORM_GPU
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}

		// UNPACKED - CPU path
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_DEFORM
				#pragma multi_compile AM_MOBILE
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_DEFORM
				#pragma multi_compile AM_MOBILE
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_DEFORM
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_DEFORM
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}

		// UNPACKED - GPU Path
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_DEFORM_GPU
				#pragma multi_compile AM_MOBILE
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_DEFORM_GPU
				#pragma multi_compile AM_MOBILE
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_DEFORM_GPU
			ENDCG
		}
		Pass {
			CGPROGRAM
				#pragma multi_compile AM_DEFORM_GPU
				#pragma multi_compile AM_CUTOUT
			ENDCG
		}
	}

	FallBack Off
}
