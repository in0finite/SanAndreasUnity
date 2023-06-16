// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "URPOcean/Fourier_L" 
{
	HLSLINCLUDE
	
	#include "NeoInclude.hlsl"

	sampler2D _ReadBuffer0, _ReadBuffer1, _ReadBuffer2;
	sampler2D _ButterFlyLookUp;
	
	struct f2a
	{
		float4 col0 : SV_Target0;
		float4 col1 : SV_Target1;
		float4 col2 : SV_Target2;
	};
	
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
	
	//Performs two FFTs on two complex numbers packed in a vector4
	float4 FFT4(float2 w, float4 input1, float4 input2) 
	{
		float rx = w.x * input2.x - w.y * input2.y;
		float ry = w.y * input2.x + w.x * input2.y;
		float rz = w.x * input2.z - w.y * input2.w;
		float rw = w.y * input2.z + w.x * input2.w;

		return input1 + float4(rx,ry,rz,rw);
	}

	f2a fragX_L(v2f IN)
	{
		float4 lookUp = tex2D(_ButterFlyLookUp, float2(IN.uv.x, 0));

		float a = UNITY_TWO_PI * lookUp.z;
		float2 w = float2(cos(a), sin(a));
		
		 w *= (lookUp.w * 2 - 1.0);
		
		f2a OUT;
		
		float2 uv1 = float2(lookUp.x, IN.uv.y);
		float2 uv2 = float2(lookUp.y, IN.uv.y);
		
		OUT.col0 = FFT4(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT4(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));
		OUT.col2 = FFT4(w, tex2D(_ReadBuffer2, uv1), tex2D(_ReadBuffer2, uv2));

		return OUT;
	}
	
	f2a fragY_L(v2f IN)
	{
		float4 lookUp = tex2D(_ButterFlyLookUp, float2(IN.uv.y, 0));
		
		//todo: Wlut
		float a = UNITY_TWO_PI*lookUp.z;
		float2 w = float2(cos(a), sin(a));
		
		w *= (lookUp.w * 2 - 1.0);
		
		f2a OUT;
		
		float2 uv1 = float2(IN.uv.x, lookUp.x);
		float2 uv2 = float2(IN.uv.x, lookUp.y);
		
		OUT.col0 = FFT4(w, tex2D(_ReadBuffer0, uv1), tex2D(_ReadBuffer0, uv2));
		OUT.col1 = FFT4(w, tex2D(_ReadBuffer1, uv1), tex2D(_ReadBuffer1, uv2));
		OUT.col2 = FFT4(w, tex2D(_ReadBuffer2, uv1), tex2D(_ReadBuffer2, uv2));

		return OUT;
	}
	
	ENDHLSL
			
	SubShader 
	{
		Pass 
    	{
			ZTest Always Cull Off ZWrite Off
      		Fog { Mode off }
    		
			HLSLPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragX_L
			#pragma exclude_renderers gles

			#pragma fragmentoption ARB_precision_hint_fastest

			ENDHLSL
		}
		
		Pass 
    	{
			ZTest Always Cull Off ZWrite Off
      		Fog { Mode off }
    		
			HLSLPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment fragY_L
			#pragma exclude_renderers gles

			#pragma fragmentoption ARB_precision_hint_fastest

			ENDHLSL
		}
	}

}