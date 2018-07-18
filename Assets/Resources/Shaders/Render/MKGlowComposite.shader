//////////////////////////////////////////////////////
// MK Glow Free Composite Shader    				//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de | www.michaelkremmel.store //
// Copyright © 2017 All rights reserved.            //
//////////////////////////////////////////////////////
Shader "Hidden/MK/Glow/Composite" 
{
	Properties 
	{ 
		_MainTex("", 2D) = "Black" {}
		_MKGlowTexInner("", 2D) = "Black" {} 
		_MKGlowTexOuter("", 2D) = "Black" {} 
		_LensTex("", 2D) = "White" {} 
		_GlowTint("", Color) = (1,1,1,1)
	}
	SubShader 
	{
		ZTest off 
		Fog { Mode Off }
		Cull back
		Lighting Off
		ZWrite Off

		Pass 
		{
			Blend One Zero
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 2.0
			#pragma multi_compile __ _MK_LENS

			#include "UnityCG.cginc"
			#include "MKGlowCompositeInc.cginc"
			ENDCG
		}

		Pass 
		{
			Blend One Zero
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 2.0

			#define FULLSCREEN 1

			#include "UnityCG.cginc"
			#include "MKGlowCompositeInc.cginc"
			ENDCG
		}
	}
	FallBack Off
}