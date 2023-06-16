// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/URPOcean/WaveEquation" 
{

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

		sampler2D _WaveTex;
	half4 _WaveOffset;

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

	float4 frag(v2f IN) : SV_Target
	{
		return tex2D(_WaveTex, IN.uv + _WaveOffset.xy);
	}

		ENDHLSL
	}

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

		sampler2D _WaveTex;
	sampler2D _WaveTex_Make;
	float4 _WaveParam;
	float4 _WaveMake;
	float _WaveDecay;

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

	float4 frag(v2f IN) : SV_Target
	{
		//todo split _WaveTex
		float2 z = tex2D(_WaveTex, IN.uv).zw;

		float z1 = tex2D(_WaveTex, IN.uv + float2(_WaveParam.x, 0)).z;
		float z2 = tex2D(_WaveTex, IN.uv + float2(0, _WaveParam.x)).z;
		float z3 = tex2D(_WaveTex, IN.uv - float2(_WaveParam.x, 0)).z;
		float z4 = tex2D(_WaveTex, IN.uv - float2(0, _WaveParam.x)).z;

		float2 offset = IN.uv - 0.5;
		float a = exp2(-dot(offset, offset) * _WaveMake.w) * _WaveMake.z;

#if 1
		float check = z.r - (tex2D(_WaveTex_Make, 1 - IN.uv).r) * _WaveMake.x; // nesscceary ? a

		check = clamp(check, -_WaveMake.y, _WaveMake.y);

		float new1 = (_WaveParam.w * (z1 + z2 + z3 + z4) + _WaveParam.y * check + _WaveParam.z * (z.g)) * _WaveDecay;

		new1 = clamp(new1, -_WaveMake.y, _WaveMake.y);

		return float4((z2 - z4) * a, (z1 - z3) * a, new1, z.r * _WaveDecay);
#else
		float pluse = lerp(0, (tex2D(_WaveTex_Make, 1 - IN.uv).r) * _WaveMake.x, step(abs(z.r), _WaveMake.y));
		z.r += pluse; // nesscceary ? a

		return float4((z2 - z4) * a, (z1 - z3) * a, (_WaveParam.w * (z1 + z2 + z3 + z4) + _WaveParam.y * z.r + _WaveParam.z * z.g) * _WaveDecay, z.r * _WaveDecay);
#endif
	}

		ENDHLSL
	}
	}
		Fallback off
}
