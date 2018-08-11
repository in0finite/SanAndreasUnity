// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

Shader "Hidden/Amplify Motion/GPUSkinDeform" {
	Properties { }
	CGINCLUDE
		#pragma target 3.0
		#pragma glsl
		#include "UnityCG.cginc"

		sampler2D _AM_BONE_TEX;
		sampler2D _AM_BONE_INDEX_TEX;
		sampler2D _AM_BASE_VERTEX0_TEX;
		sampler2D _AM_BASE_VERTEX1_TEX;
		sampler2D _AM_BASE_VERTEX2_TEX;
		sampler2D _AM_BASE_VERTEX3_TEX;

		float4 _AM_BONE_TEXEL_SIZE;
		float4 _AM_BONE_TEXEL_HALFSIZE;

		float4x4 _AM_WORLD_TO_LOCAL_MATRIX;

		inline float4x4 fetch_bone( float index )
		{
			float4 col0 = tex2Dlod( _AM_BONE_TEX, float4( float2( index, 0 ) * _AM_BONE_TEXEL_SIZE.xy + _AM_BONE_TEXEL_HALFSIZE.xy, 0, 0 ) );
			float4 col1 = tex2Dlod( _AM_BONE_TEX, float4( float2( index, 1 ) * _AM_BONE_TEXEL_SIZE.xy + _AM_BONE_TEXEL_HALFSIZE.xy, 0, 0 ) );
			float4 col2 = tex2Dlod( _AM_BONE_TEX, float4( float2( index, 2 ) * _AM_BONE_TEXEL_SIZE.xy + _AM_BONE_TEXEL_HALFSIZE.xy, 0, 0 ) );
			return float4x4( col0, col1, col2, float4( 0, 0, 0, 1 ) );
		}
	ENDCG
	SubShader {
		ZTest Always Cull Off ZWrite Off Fog { Mode off }

		// 1 weight per-vertex
		Pass {
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				float4 frag( v2f_img i ) : SV_Target
				{
					float4 boneIndices = tex2Dlod( _AM_BONE_INDEX_TEX, float4( i.uv, 0, 0 ) );

					float4 baseVertex0 = tex2Dlod( _AM_BASE_VERTEX0_TEX, float4( i.uv, 0, 0 ) );

					float4x4 bone0 = mul( _AM_WORLD_TO_LOCAL_MATRIX, fetch_bone( boneIndices.x ) );

					float4 deformedVertex;
					deformedVertex = mul( bone0, baseVertex0 );

					return float4( deformedVertex.xyz, 1 );
				}
			ENDCG
		}

		// 2 weights per-vertex
		Pass {
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				float4 frag( v2f_img i ) : SV_Target
				{
					float4 boneIndices = tex2Dlod( _AM_BONE_INDEX_TEX, float4( i.uv, 0, 0 ) );

					float4 baseVertex0 = tex2Dlod( _AM_BASE_VERTEX0_TEX, float4( i.uv, 0, 0 ) );
					float4 baseVertex1 = tex2Dlod( _AM_BASE_VERTEX1_TEX, float4( i.uv, 0, 0 ) );

					float4x4 bone0 = mul( _AM_WORLD_TO_LOCAL_MATRIX, fetch_bone( boneIndices.x ) );
					float4x4 bone1 = mul( _AM_WORLD_TO_LOCAL_MATRIX, fetch_bone( boneIndices.y ) );

					float4 deformedVertex;
					deformedVertex  = mul( bone0, baseVertex0 );
					deformedVertex += mul( bone1, baseVertex1 );

					return float4( deformedVertex.xyz, 1 );
				}
			ENDCG
		}

		// 4 weights per-vertex
		Pass {
			CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag

				float4 frag( v2f_img i ) : SV_Target
				{
					float4 boneIndices = tex2Dlod( _AM_BONE_INDEX_TEX, float4( i.uv, 0, 0 ) );

					float4 baseVertex0 = tex2Dlod( _AM_BASE_VERTEX0_TEX, float4( i.uv, 0, 0 ) );
					float4 baseVertex1 = tex2Dlod( _AM_BASE_VERTEX1_TEX, float4( i.uv, 0, 0 ) );
					float4 baseVertex2 = tex2Dlod( _AM_BASE_VERTEX2_TEX, float4( i.uv, 0, 0 ) );
					float4 baseVertex3 = tex2Dlod( _AM_BASE_VERTEX3_TEX, float4( i.uv, 0, 0 ) );

					float4x4 bone0 = mul( _AM_WORLD_TO_LOCAL_MATRIX, fetch_bone( boneIndices.x ) );
					float4x4 bone1 = mul( _AM_WORLD_TO_LOCAL_MATRIX, fetch_bone( boneIndices.y ) );
					float4x4 bone2 = mul( _AM_WORLD_TO_LOCAL_MATRIX, fetch_bone( boneIndices.z ) );
					float4x4 bone3 = mul( _AM_WORLD_TO_LOCAL_MATRIX, fetch_bone( boneIndices.w ) );

					float4 deformedVertex;
					deformedVertex  = mul( bone0, baseVertex0 );
					deformedVertex += mul( bone1, baseVertex1 );
					deformedVertex += mul( bone2, baseVertex2 );
					deformedVertex += mul( bone3, baseVertex3 );

					return float4( deformedVertex.xyz, 1 );
				}
			ENDCG
		}
	}
}
