// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Motion/Dilation" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MotionTex ("Motion (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma exclude_renderers flash
		#include "Shared.cginc"

		struct v2f
		{
			float4 pos : SV_POSITION;
			float4 uv : TEXCOORD0;
		};

		v2f vert( appdata_img v )
		{
			v2f o;
			o.pos = CustomObjectToClipPos( v.vertex );
			o.uv.xy = v.texcoord.xy;
			o.uv.zw = v.texcoord.xy;
		#if UNITY_UV_STARTS_AT_TOP
			if ( _MainTex_TexelSize.y < 0 )
				o.uv.w = 1 - o.uv.w;
		#endif
			return o;
		}
	ENDCG
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode off }

		// Separable Dilation - 3-Tap Horizontal
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_horizontal

				half4 frag_horizontal( v2f i ) : SV_Target
				{
					float tx = _MainTex_TexelSize.x;
					float2 offsets[ 3 ] = { float2( -tx, 0 ), float2( 0, 0 ), float2( tx, 0 ) };

					half4 motion_ref = tex2D( _MotionTex, i.uv.zw );
					float depth_ref = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.zw );
					half4 result = motion_ref;

					for ( int tap = 0; tap < 3; tap++ )
					{
						float2 tap_uv = i.uv.zw + offsets[ tap ];

						half4 motion = tex2D( _MotionTex, tap_uv );
						float depth =  SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, tap_uv );
						result = ( depth < depth_ref ) ? motion : result;
					}

					return result;
				}
			ENDCG
		}

		// Separable Dilation - 3-Tap Vertical
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_vertical

				half4 frag_vertical( v2f i ) : SV_Target
				{
					float ty = _MainTex_TexelSize.y;
					float2 offsets[ 3 ] = { float2( 0, -ty ), float2( 0, 0 ), float2( 0, ty ) };

					half4 motion_ref = tex2D( _MotionTex, i.uv.zw );
					float depth_ref = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.zw );
					half4 result = motion_ref;

					for ( int tap = 0; tap < 3; tap++ )
					{
						float2 tap_uv = i.uv.zw + offsets[ tap ];

						half4 motion = tex2D( _MotionTex, tap_uv );
						float depth =  SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, tap_uv );
						result = ( depth < depth_ref ) ? motion : result;
					}

					return result;
				}
			ENDCG
		}

		// Separable Dilation - 5-Tap Horizontal
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_horizontal

				half4 frag_horizontal( v2f i ) : SV_Target
				{
					float tx1 = _MainTex_TexelSize.x;
					float tx2 = tx1 + tx1;
					float2 offsets[ 5 ] = { float2( -tx2, 0 ), float2( -tx1, 0 ), float2( 0, 0 ), float2( tx1, 0 ), float2( tx2, 0 ) };

					half4 motion_ref = tex2D( _MotionTex, i.uv.zw );
					float depth_ref = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.zw );
					half4 result = motion_ref;

					for ( int tap = 0; tap < 5; tap++ )
					{
						float2 tap_uv = i.uv.zw + offsets[ tap ];

						half4 motion = tex2D( _MotionTex, tap_uv );
						float depth =  SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, tap_uv );
						result = ( depth < depth_ref ) ? motion : result;
					}

					return result;
				}
			ENDCG
		}

		// Separable Dilation - 5-Tap Vertical
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag_vertical

				half4 frag_vertical( v2f i ) : SV_Target
				{
					float ty1 = _MainTex_TexelSize.y;
					float ty2 = ty1 + ty1;
					float2 offsets[ 5 ] = { float2( 0, -ty2 ), float2( 0, -ty1 ), float2( 0, 0 ), float2( 0, ty1 ), float2( 0, ty2 ) };

					half4 motion_ref = tex2D( _MotionTex, i.uv.zw );
					float depth_ref = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.zw );
					half4 result = motion_ref;

					for ( int tap = 0; tap < 5; tap++ )
					{
						float2 tap_uv = i.uv.zw + offsets[ tap ];

						half4 motion = tex2D( _MotionTex, tap_uv );
						float depth =  SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, tap_uv );
						result = ( depth < depth_ref ) ? motion : result;
					}

					return result;
				}
			ENDCG
		}
	}

	Fallback Off
}
