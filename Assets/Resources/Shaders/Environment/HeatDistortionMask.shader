// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ITGGame/HeatDistortionMask" 
{
		Properties
		{
			_DistortionTex("Normal Map", 2D) = "white" {}           
			_MaskTex("Mask Map", 2D) = "white" {} 
			_Strength("Distortion Strength",Range(-1,1)) = 1
		}
		Category
		{
			Tags{ "Queue" = "Transparent+1" "RenderType" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha   
										  
			AlphaTest Greater .01     
			Cull Off Lighting Off ZWrite Off

			SubShader
			{
				GrabPass
				{
					Name "BASE"
					Tags{ "LightMode" = "Always" }
				}
				Pass
				{
					Name "BASE"
					Tags{ "LightMode" = "Always" }

					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma fragmentoption ARB_precision_hint_fastest
					#include "UnityCG.cginc"
					struct v2f
					{
						float4 vertex : POSITION; 
						float4 uvgrab : TEXCOORD0; 
						float2 uvmain : TEXCOORD1; 
					};
					float4 _DistortionTex_ST;
					float4 _MaskTex_ST;
					sampler2D _DistortionTex;
					sampler2D _MaskTex;
					sampler2D _GrabTexture; 
					float _Strength;
					v2f vert(appdata_base v)
					{
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);
						#if UNITY_UV_STARTS_AT_TOP  
								float scale = -1.0;
						#else
								float scale = 1.0;
						#endif
						o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
						o.uvgrab.zw = o.vertex.zw;
						o.uvmain = TRANSFORM_TEX(v.texcoord, _MaskTex);
						return o;
					}
					half4 frag(v2f i) : COLOR
					{
						float3 normal = normalize(tex2D(_DistortionTex,i.uvmain)).rgb - float3(0.5,0.5,0.5);
						float2 uvOffset = refract(float3(0, 0, 1), normal, 1)  * _Strength;

						i.uvgrab.xy = i.uvgrab.xy + uvOffset;

						half4 noiseCol = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
						noiseCol.a = 1;

						half4 areaCol = tex2D(_MaskTex, i.uvmain);

						return  noiseCol * areaCol;
					}
					ENDCG
				}
			}

	}
}
