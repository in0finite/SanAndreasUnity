Shader "Hidden/Universal Render Pipeline/SunShaftsComposite"
{
    Properties
    {
        _MainTex ("Base", 2D) = "white" {}
    }

    HLSLINCLUDE
    #if SHADER_API_GLES
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
			
	struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
	};
		
	struct v2f_radial {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 blurVector : TEXCOORD1;
	};
		
	TEXTURE2D_X(_MainTex);
	TEXTURE2D_X_FLOAT(_CameraDepthTexture);
	TEXTURE2D_X(_ColorBuffer);

	half4 _BlurRadius4;
	half4 _SunPosition;
	half4 _MainTex_TexelSize;	

	#define SAMPLES_FLOAT 6.0f
	#define SAMPLES_INT 6
			
	v2f vert(Attributes v ) 
	{
		v2f o;
		o.pos = TransformObjectToHClip(v.positionOS.xyz);
		o.uv = v.uv.xy;
		
		return o;
	}
		
	half4 fragScreen(v2f i) : SV_Target 
	{ 
		half4 colorA = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, i.uv.xy);

		half4 colorB = SAMPLE_TEXTURE2D_X(_ColorBuffer, sampler_PointClamp, i.uv.xy);

		half4 depthMask = saturate (colorB * min(_MainLightColor, 1));
		return 1.0f - (1.0f-colorA) * (1.0f-depthMask);	
	}

	half4 fragAdd(v2f i) : SV_Target 
	{ 
		half4 colorA = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, i.uv.xy);
		half4 colorB = SAMPLE_TEXTURE2D_X(_ColorBuffer, sampler_PointClamp, i.uv.xy);
		half4 depthMask = saturate (colorB * min(_MainLightColor,1));
		return colorA + depthMask;	
	}
	
	v2f_radial vert_radial(Attributes v ) 
	{
		v2f_radial o;
		o.pos = TransformObjectToHClip(v.positionOS.xyz);
		
		o.uv.xy =  v.uv.xy;
		o.blurVector = (_SunPosition.xy - v.uv.xy) * _BlurRadius4.xy;
		
		return o; 
	}
	
	half4 frag_radial(v2f_radial i) : SV_Target 
	{	
		half4 color = half4(0,0,0,0);
		for(int j = 0; j < SAMPLES_INT; j++)   
		{	
			half4 tmpColor = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, i.uv.xy);
			color += tmpColor;
			i.uv.xy += i.blurVector; 	
		}
		return color / SAMPLES_FLOAT;
	}	
	
	half4 TransformColor (half4 skyboxValue) {
		return max (skyboxValue, 0); 		
	}
	
	half4 frag_depth (v2f i) : SV_Target {

		float depthSample = (SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_PointClamp, i.uv.xy)).r;

		half4 tex = SAMPLE_TEXTURE2D_X(_MainTex, sampler_PointClamp, i.uv.xy);
		
		depthSample = Linear01Depth (depthSample, _ZBufferParams);
		 
		// consider maximum radius
		half2 vec = _SunPosition.xy - i.uv.xy;		

		half dist = saturate(_SunPosition.w - length(vec.xy));

		half4 outColor = 0;
		
		// consider shafts blockers
		if (depthSample > 0.99)
			outColor = TransformColor (tex) * dist;
			
		return outColor;
	}

	half4 FragBlurH(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float texelSize = _MainTex_TexelSize.x;
		float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

		// 9-tap gaussian blur on the downsampled source
		half3 c0 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 4.0, 0.0))).rgb;
		half3 c1 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 3.0, 0.0))).rgb;
		half3 c2 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 2.0, 0.0))).rgb;
		half3 c3 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(texelSize * 1.0, 0.0))).rgb;
		half3 c4 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv)).rgb;
		half3 c5 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 1.0, 0.0))).rgb;
		half3 c6 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 2.0, 0.0))).rgb;
		half3 c7 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 3.0, 0.0))).rgb;
		half3 c8 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(texelSize * 4.0, 0.0))).rgb;

		half3 color = c0 * 0.01621622 + c1 * 0.05405405 + c2 * 0.12162162 + c3 * 0.19459459
					+ c4 * 0.22702703
					+ c5 * 0.19459459 + c6 * 0.12162162 + c7 * 0.05405405 + c8 * 0.01621622;

		return half4(color,1);
	}

	half4 FragBlurV(Varyings input) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
		float texelSize = _MainTex_TexelSize.y;
		float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);

		// Optimized bilinear 5-tap gaussian on the same-sized source (9-tap equivalent)
		half3 c0 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 3.23076923))).rgb;
		half3 c1 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv - float2(0.0, texelSize * 1.38461538))).rgb;
		half3 c2 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv)).rgb;
		half3 c3 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 1.38461538))).rgb;
		half3 c4 = (SAMPLE_TEXTURE2D_X(_MainTex, sampler_LinearClamp, uv + float2(0.0, texelSize * 3.23076923))).rgb;

		half3 color = c0 * 0.07027027 + c1 * 0.31621622
					+ c2 * 0.22702703
					+ c3 * 0.31621622 + c4 * 0.07027027;

		return half4(color, 1);
	}
	
	half4 frag_depth_masks (v2f i) : SV_Target {

		return 1;
	}	
    #endif
    ENDHLSL

    Subshader
    {

        //0
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }

            HLSLPROGRAM
            #if SHADER_API_GLES
		  #pragma fragmentoption ARB_precision_hint_fastest 
		  #pragma vertex vert
		  #pragma fragment fragScreen
		  #pragma exclude_renderers gles
            #endif
            ENDHLSL
        }

        //1
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }

            HLSLPROGRAM
            #if SHADER_API_GLES
		  #pragma fragmentoption ARB_precision_hint_fastest
		  #pragma vertex vert_radial
		  #pragma fragment frag_radial
		  #pragma exclude_renderers gles
            #endif
            ENDHLSL
        }

        //2
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }

            HLSLPROGRAM
            #if SHADER_API_GLES
		  #pragma fragmentoption ARB_precision_hint_fastest      
		  #pragma vertex vert
		  #pragma fragment frag_depth
		  #pragma exclude_renderers gles
            #endif
            ENDHLSL
        }

        //3
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }

            HLSLPROGRAM
            #if SHADER_API_GLES
		  #pragma fragmentoption ARB_precision_hint_fastest      
		  #pragma vertex vert
		  #pragma fragment frag_depth_masks
		  #pragma exclude_renderers gles
            #endif
            ENDHLSL
        }

        //4
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }

            HLSLPROGRAM
            #if SHADER_API_GLES
		  #pragma fragmentoption ARB_precision_hint_fastest 
		  #pragma vertex vert
		  #pragma fragment fragAdd
		  #pragma exclude_renderers gles
            #endif
            ENDHLSL
        }

        //5
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }

            HLSLPROGRAM
            #if SHADER_API_GLES
		#pragma fragmentoption ARB_precision_hint_fastest 
		#pragma vertex vert
		#pragma fragment FragBlurH
		#pragma exclude_renderers gles
            #endif
            ENDHLSL
        }

        //6
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog
            {
                Mode off
            }

            HLSLPROGRAM
            #if SHADER_API_GLES
		#pragma fragmentoption ARB_precision_hint_fastest 
		#pragma vertex vert
		#pragma fragment FragBlurV
		#pragma exclude_renderers gles
            #endif
            ENDHLSL
        }
    }

    Fallback off

} // shader