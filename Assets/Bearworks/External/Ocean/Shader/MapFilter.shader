// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "URPOcean/MapFilter"
{
	Properties
	{
		_Map0("Map Tex", 2D) = "black" {}
	}

	SubShader
	{

	Pass
	{
		ZTest Always Cull Off ZWrite Off
		Fog{ Mode off }

		HLSLPROGRAM
		#include "NeoInclude.hlsl"
		#pragma target 3.0
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma exclude_renderers gles

		struct v2f
		{
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
		};

		v2f vert(appdata_base v)
		{
			v2f OUT;
			OUT.pos = UnityObjectToClipPos(v.vertex);
			OUT.uv = v.texcoord.xy;
			return OUT;
		}

		//sampler2D _FFTMap0;
		sampler2D _FFTMap1;
		sampler2D _FFTMap2;

		float4 frag(v2f IN) : SV_Target
		{
			//float4 z1 = tex2D(_FFTMap0, IN.uv);
			float4 z2 = tex2D(_FFTMap1, IN.uv);
			float4 z3 = tex2D(_FFTMap2, IN.uv);

			float2x2 jacobian = float2x2(1 - z3.x, z3.y, z3.z, 1 - z3.w);
			float det = determinant(jacobian);

			return float4(z2.x + z2.z, z2.y + z2.w, (1 - det), 0);
		}

			ENDHLSL
		}
		}
		Fallback off
}
