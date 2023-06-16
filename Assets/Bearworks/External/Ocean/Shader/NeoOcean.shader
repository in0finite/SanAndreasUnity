Shader "URPOcean/NeoSurface" { 
Properties {
	_BaseColor ("Base color", COLOR)  = ( .54, .95, .99, 1)	
	_ShallowColor ("Shallow color", COLOR)  = ( .10, .4, .43, 1)	

	_DistortParams ("Refract, Reflect, Normal Power, Normal Sharp Bias", Vector) = (0.05 , 0.05, 4.0, 3.0)
	_Depth("Depth", Range(0.01, 5)) = 1
	_ShallowDepth("Shallow", Range(0.1, 10)) = 5
	_Transparency("Transparency", Range(0.1, 1)) = 0.5

	_Fresnel("Fresnel", Range(0.02, 0.5)) = 0.04
	_Shadow("Shadow", Range(0.1, 0.9)) = 0.35
	_Fade ("Fade", Range (0.001, 0.03)) = 0.002

	_FoamMask("Foam Mask (Peak(R) FX(B) Shore(G))", 2D) = "black" {}

	_FoamPeak("Foam Peak (Blend & Scale & Attetion & Edge)", Vector) = (1, 1, 1, 1)
	_Foam ("Foam Intensity, Depth, Appear, Distort", Vector) = (1, 1, 0.5, 0.1)

	_SunIntensity ("SunIntensity", Range (0.1, 10)) = 0.05
	_Shininess ("Shininess", Range (2.0, 500)) = 32	

	_TessellationUniform("Tessellation Uniform", Range(1, 6)) = 1
	_FadeTess("Tessellation Fade", Range(0.01, 0.06)) = 0.002

	[HideInInspector][KeywordEnum(OFF, ON)] _PROJECTED("Projected Mode", float) = 0
	[HideInInspector] _Map0("Map0", 2D) = "black" {}
	[HideInInspector] _Map1("Map1", 2D) = "black" {}
} 


Subshader
{
	Tags{ "RenderType" = "Transparent" "Queue" = "Transparent-11" }

	Lod 300

	Pass{
	Tags{
	"LightMode" = "UniversalForward"
}
	Blend One Zero
	ZTest LEqual
	ZWrite On
	Cull Off
	Fog{ Mode off }

	HLSLPROGRAM

	#pragma target 4.6
	#pragma exclude_renderers gles

	#pragma multi_compile _ _PROJECTED_ON
	#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
	#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
	#pragma multi_compile _ _WATERWAVE_ON
	#pragma multi_compile_fragment _ _SSREFLECTION_ON
	#pragma multi_compile_fragment _ _SHADOWS_SOFT
	#pragma multi_compile_fog
	#include "NeoInclude.hlsl"
	#pragma vertex TessellationVertexProgram_MQ
	#pragma hull HullProgram_MQ
	#pragma domain DomainProgram_MQ
	#pragma fragment frag_MQ
	#pragma fragmentoption ARB_precision_hint_fastest

	ENDHLSL
}
}


Fallback off
}
