// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Motion/MotionBlurSM2" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MotionTex ("Motion (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma exclude_renderers flash

		#include "MotionBlurShared.cginc"
	ENDCG
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode off }
		Pass {
			Name "MOB"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_mobile
				#pragma target 2.0
			ENDCG
		}
		Pass {
			Name "STD"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_sm2
				#pragma target 2.0
			ENDCG
		}
		Pass {
			Name "STD"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_sm2
				#pragma target 2.0
			ENDCG
		}
		Pass {
			Name "STD"
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_sm2
				#pragma target 2.0
			ENDCG
		}
		Pass {
			Name "MOB_NOISE" // NOISE NOT SUPPORTED IN SM2; FALLBACK TO MOB
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_mobile
				#pragma target 2.0
			ENDCG
		}
		Pass {
			Name "STD_NOISE" // NOISE NOT SUPPORTED IN SM2; FALLBACK TO STD
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_sm2
				#pragma target 2.0
			ENDCG
		}
		Pass {
			Name "STD_NOISE" // NOISE NOT SUPPORTED IN SM2; FALLBACK TO STD
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_sm2
				#pragma target 2.0
			ENDCG
		}
		Pass {
			Name "STD_NOISE" // NOISE NOT SUPPORTED IN SM2; FALLBACK TO STD
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_sm2
				#pragma target 2.0
			ENDCG
		}
	}
	Fallback Off
}
