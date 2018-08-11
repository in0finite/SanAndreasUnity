
Shader "Hidden/Amplify Motion/ReprojectionVectors" {
	Properties {
		_MainTex ("-", 2D) = "" {}
	}
	SubShader {
		Cull Off ZTest Always ZWrite Off Blend Off Fog { Mode Off }
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile AM_PACKED
				#include "Shared.cginc"

				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float2 uv_rt : TEXCOORD1;
				};

				float4x4 _AM_MATRIX_CURR_REPROJ;

				v2f vert( appdata_img v )
				{
					v2f o;
					o.pos = CustomObjectToClipPos( v.vertex );
					o.uv = v.texcoord.xy;
					o.uv_rt = v.texcoord.xy;
				#if UNITY_UV_STARTS_AT_TOP
					if ( _MainTex_TexelSize.y < 0 )
						o.uv_rt.y = 1 - o.uv_rt.y;
				#endif
					return o;
				}

				half4 frag( v2f i ) : SV_Target
				{
					float d = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv_rt );
				#if defined( SHADER_API_OPENGL ) || defined( SHADER_API_GLES ) || defined( SHADER_API_GLES3 ) || defined( SHADER_API_GLCORE )
					float4 pos_curr = float4( float3( i.uv.xy, d ) * 2 - 1, 1 );
				#else
					float4 pos_curr = float4( i.uv.xy * 2 - 1, d, 1 );
				#endif

					// 1) unproject to world; 2) reproject into previous ViewProj
					float4 pos_prev = mul( _AM_MATRIX_CURR_REPROJ, pos_curr );

					half2 motion = ComputeMotionVector( pos_prev, pos_curr, _AM_MOTION_PARAMS.x );
				#if defined( AM_PACKED )
					return PackMotionVector( motion, _AM_MOTION_PARAMS.z );
				#else
					return half4( motion.xy, _AM_MOTION_PARAMS.z, 0 );
				#endif
				}
			ENDCG
		}
	}
	FallBack Off
}
