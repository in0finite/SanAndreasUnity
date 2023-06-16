// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/URPOcean/WaveWriter" 
{
	
	SubShader {
		Tags {"LightMode" = "UniversalForward" }
		
		pass {
			HLSLPROGRAM
			#include "NeoInclude.hlsl"

				struct v2f {
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata_base v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			float4 frag(v2f i) : COLOR{
				return 1;

			}
				#pragma vertex vert
				#pragma fragment frag
			ENDHLSL
		}
	}
}
