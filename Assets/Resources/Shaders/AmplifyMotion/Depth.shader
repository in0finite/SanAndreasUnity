// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Motion/Depth" {
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
			float4 uv  : TEXCOORD0;
			float4 uv0 : TEXCOORD1;
			float4 uv1 : TEXCOORD2;
			float4 uv2 : TEXCOORD3;
			float4 uv3 : TEXCOORD4;
		};

		v2f vert( appdata_img v )
		{
			v2f o;
			o.pos = CustomObjectToClipPos( v.vertex );

			const float4 texels = float4( _MainTex_TexelSize.xy, _CameraDepthTexture_TexelSize.xy );
			const float4 offsets[ 4 ] = {
				float4( 0.5, 0.5, 0.5, 0.5 ),
				float4( 1.5, 0.5, 1.5, 0.5 ),
				float4( 1.5, 1.5, 1.5, 1.5 ),
				float4( 0.5, 1.5, 0.5, 1.5 )
			};

			o.uv = v.texcoord.xyxy;
			o.uv0 = v.texcoord.xyxy + texels * offsets[ 0 ];
			o.uv1 = v.texcoord.xyxy + texels * offsets[ 1 ];
			o.uv2 = v.texcoord.xyxy + texels * offsets[ 2 ];
			o.uv3 = v.texcoord.xyxy + texels * offsets[ 3 ];

		#if UNITY_UV_STARTS_AT_TOP
			if ( _MainTex_TexelSize.y < 0 )
			{
				o.uv.w  = 1 - o.uv.w;
				o.uv0.w = 1 - o.uv0.w;
				o.uv1.w = 1 - o.uv1.w;
				o.uv2.w = 1 - o.uv2.w;
				o.uv3.w = 1 - o.uv3.w;
			}
		#endif
			return o;
		}
	ENDCG
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode off }

		// Straight Copy
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				fixed4 frag( v2f i ) : SV_Target
				{
					float depth = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.zw );
					return ( depth == 1.0 ) ? ( 1.0 ).xxxx : EncodeFloatRGBA( depth );
				}
			ENDCG
		}

		// 4-Tap MinZ Depth Downsampling
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag( v2f i ) : SV_Target
				{
					float depth0 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv0.zw );
					float depth1 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv1.zw );
					float depth2 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv2.zw );
					float depth3 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv3.zw );

					float depth = depth0;
					depth = ( depth1 < depth ) ? depth1 : depth;
					depth = ( depth2 < depth ) ? depth2 : depth;
					depth = ( depth3 < depth ) ? depth3 : depth;

					return depth.xxxx;
				}
			ENDCG
		}

		// 4-Tap Depth-Aware (MinZ) Combined Downsampling
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag( v2f i ) : SV_Target
				{
					float depth0 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv0.zw );
					float depth1 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv1.zw );
					float depth2 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv2.zw );
					float depth3 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv3.zw );

					half4 combo0 = half4( tex2D( _MainTex, i.uv0.xy ).xyz, tex2D( _MotionTex, i.uv0.zw ).a + 0.0000001 );
					half4 combo1 = half4( tex2D( _MainTex, i.uv1.xy ).xyz, tex2D( _MotionTex, i.uv1.zw ).a + 0.0000001 );
					half4 combo2 = half4( tex2D( _MainTex, i.uv2.xy ).xyz, tex2D( _MotionTex, i.uv2.zw ).a + 0.0000001 );
					half4 combo3 = half4( tex2D( _MainTex, i.uv3.xy ).xyz, tex2D( _MotionTex, i.uv3.zw ).a + 0.0000001 );

					half4 combo = combo0;
					float depth = depth0;

					combo = ( depth1 < depth ) ? combo1 : combo;
					depth = ( depth1 < depth ) ? depth1 : depth;
					combo = ( depth2 < depth ) ? combo2 : combo;
					depth = ( depth2 < depth ) ? depth2 : depth;
					combo = ( depth3 < depth ) ? combo3 : combo;
					depth = ( depth3 < depth ) ? depth3 : depth;

					return combo;
				}
			ENDCG
		}
	}

	Fallback Off
}
