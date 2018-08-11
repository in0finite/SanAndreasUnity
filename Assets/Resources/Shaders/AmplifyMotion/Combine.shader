// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Motion/Combine" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MotionTex ("Motion (RGB)", 2D) = "white" {}
		_CameraMotionTex ("Motion (RGB)", 2D) = "white" {}
	}
	CGINCLUDE
		#include "Shared.cginc"

		sampler2D _BlurredTex;

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
		#if defined( UNITY_UV_STARTS_AT_TOP )
			if ( _MainTex_TexelSize.y < 0 )
				o.uv.w = 1 - o.uv.w;
		#endif
			return o;
		}
	ENDCG
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode off }

		// Combine source RGB and motion object ID
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma exclude_renderers flash

				half4 frag( v2f i ) : SV_Target
				{
					return half4( tex2D( _MainTex, i.uv.xy ).xyz, tex2D( _MotionTex, i.uv.zw ).a + 0.0000001f ); // hack to trick Unity into behaving
				}
			ENDCG
		}

		// Combine motion blurred lowres and non-blurred full res (mobile mode)
		Pass {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma exclude_renderers flash

				half4 frag( v2f i ) : SV_Target
				{
					half4 source = tex2D( _MainTex, i.uv.xy );
					half mag = 2 * tex2D( _MotionTex, i.uv.zw ).z;
					return half4( source.rgb, 1 - saturate( mag * 1.5 ) );
				}
			ENDCG
		}
	}

	Fallback Off
}
