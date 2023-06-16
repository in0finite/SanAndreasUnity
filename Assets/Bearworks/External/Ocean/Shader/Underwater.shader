Shader "URPOcean/Underwater"
{
	Properties
	{
		_BaseColor("Under Water color", COLOR) = (.54, .95, .99, 1)
		_DistortMap("Distort Map", 2D) = "black" {}
		_DepthFade("Depth Fade", Range(0.0, 1.0)) = 0.95
	}

	SubShader
	{
		Tags { "LightMode"="Underwater" "RenderType"="Transparent" "Queue"="Transparent" }

		Pass
		{
			// The ocean surface will render after the skirt, and overwrite the pixels
			ZWrite Off
			ZTest Always

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "NeoInclude.hlsl"
			
			#define MAX_OFFSET 5.0
			float _OceanHeight;

			struct VaryingsUnder
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				half4 screenPos : TEXCOORD1;
				float3 positionWS : TEXCOORD2;
			};

			inline void SampleWavesPos(float2 position, out float3 waveOut)
			{
				float2 tileableUvScale = position * _InvNeoScale;

				half4 c = tex2Dlod(_Map0, float4(tileableUvScale, 0, 0));
				half4 c2 = tex2Dlod(_Map0, float4(tileableUvScale * 2, 0, 0)) * 0.5;
				half4 c3 = tex2Dlod(_Map0, float4(tileableUvScale * 4, 0, 0)) * 0.25;

				float3 delta = c.zxw + c2.zxw + c3.zxw;
				delta.y += c.y + c2.y + c3.y;

				waveOut = delta;
			}

			float IntersectRayWithWaterSurface(const float3 pos, const float3 dir)
			{
				// Find intersection of the near plane and the water surface at this vert using FPI. See here for info about
				// FPI http://www.huwbowles.com/fpi-gdc-2016/

				// get point at sea level
				float2 sampleXZ = pos.xz - dir.xz * (pos.y - _OceanHeight) / dir.y;
				float3 disp;
				//for (int i = 0; i < 5; i++)
				{
					// Sample displacement textures, add results to current world pos / normal / foam
					SampleWavesPos(sampleXZ, disp); 
					disp += float3(sampleXZ.x, _OceanHeight, sampleXZ.y);
					float3 nearestPointOnRay = pos + dir * dot(disp - pos, dir);
				    float2 error = disp.xz - nearestPointOnRay.xz;
					sampleXZ -= error;
				}

				// Sample displacement textures, add results to current world pos / normal / foam
				SampleWavesPos(sampleXZ, disp);
				disp += float3(sampleXZ.x, _OceanHeight, sampleXZ.y);

				return dot(disp - pos, dir);
			}

			float _HeightOffset;


			struct Attributes
			{
				float3 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			#define CREST_MAX_UPDOWN_AMOUNT 0.8

			VaryingsUnder Vert(Attributes input)
			{
				VaryingsUnder o;

				// Goal of this vert shader is to place a sheet of triangles in front of the camera. The geometry has
				// two rows of verts, the top row and the bottom row (top and bottom are view relative). The bottom row
				// is pushed down below the bottom of the screen. Every vert in the top row can take any vertical position
				// on the near plane in order to find the meniscus of the water. Due to render states, the ocean surface
				// will stomp over the results of this shader. The ocean surface has necessary code to render from underneath
				// and correctly fog etc.

				// Potential optimisations (note that this shader runs over a few dozen vertices, not over screen pixels!):
				// - when looking down through the water surface, the code currently pushes the top verts of the skirt
				//   up to cover the whole screen, but it only needs to get pushed up to the horizon level to meet the water surface

				// view coordinate frame for camera
				const float3 right   = unity_CameraToWorld._11_21_31;
				const float3 up      = unity_CameraToWorld._12_22_32;
				const float3 forward = unity_CameraToWorld._13_23_33;

				const float3 nearPlaneCenter = _WorldSpaceCameraPos + forward * _ProjectionParams.y * 1.001;
				// Spread verts across the near plane.
				const float aspect = _ScreenParams.x / _ScreenParams.y;
				o.positionWS = nearPlaneCenter
					+ 2.6 * unity_CameraInvProjection._m11 * aspect * right * input.positionOS.x * _ProjectionParams.y
					+ up * input.positionOS.z * _ProjectionParams.y;

				// Isolate topmost edge
				if (input.positionOS.z > 0.45)
				{
					const float3 posOnNearPlane = o.positionWS;

					// Only compute intersection of water if viewer is looking "horizontal-ish". When the viewer starts to look
					// too much up or down, the intersection between the near plane and the water surface can be complex.
					if (abs(forward.y) < CREST_MAX_UPDOWN_AMOUNT)
					{
						// move vert in the up direction, but only to an extent, otherwise numerical issues can cause weirdness
						o.positionWS += min(IntersectRayWithWaterSurface(o.positionWS, up), MAX_OFFSET) * up;

						// Move the geometry towards the horizon. As noted above, the skirt will be stomped by the ocean
						// surface render. If we project a bit towards the horizon to make a bit of overlap then we can reduce
						// the chance render issues from cracks/gaps with down angles, or of the skirt being too high for up angles.
						float3 horizonPoint = _WorldSpaceCameraPos + (posOnNearPlane - _WorldSpaceCameraPos) * 10000.0;
						horizonPoint.y = _OceanHeight;
						const float3 horizonDir = normalize(horizonPoint - _WorldSpaceCameraPos);
						const float3 projectionOfHorizonOnNearPlane = _WorldSpaceCameraPos + horizonDir / dot(horizonDir, forward);
						o.positionWS = lerp(o.positionWS, projectionOfHorizonOnNearPlane, 0.1);
					}
					else if (_HeightOffset < -1.0)
					{
						// Deep under water - always push top edge up to cover screen
						o.positionWS += MAX_OFFSET * up;
					}
					else
					{
						// Near water surface - this is where the water can intersect the lens in nontrivial ways and causes problems
						// for finding the meniscus / water line.

						// Push top edge up if we are looking down so that the screen defaults to looking underwater.
						// Push top edge down if we are looking up so that the screen defaults to looking out of water.
						o.positionWS -= sign(forward.y) * MAX_OFFSET * up;
					}

					// Test - always put top row of verts at water horizon, because then it will always meet the water
					// surface. Good idea but didnt work because it then does underwater shading on opaque surfaces which
					// can be ABOVE the water surface. Not sure if theres any way around this.
					o.positionCS = mul(UNITY_MATRIX_VP, float4(o.positionWS, 1.0));
					o.positionCS.z = o.positionCS.w;
				}
				else
				{
					// Bottom row of verts - push them down below bottom of screen
					o.positionWS -= MAX_OFFSET * up;

					o.positionCS = mul(UNITY_MATRIX_VP, float4(o.positionWS, 1.0));
					o.positionCS.z = o.positionCS.w;
				}

				o.screenPos = ComputeScreenPos(o.positionCS);

				o.uv = input.uv;

				return o;
			}

			sampler2D _DistortMap;
			float4 _DistortMap_ST;
			float _DepthFade;

		    half3 PerPixelBump(sampler2D bumpMap, float4 coords, half bumpStrength)
			{
				half4 bump = tex2D(bumpMap, coords.xy) + tex2D(bumpMap, coords.zw);
				bump *= 0.5;
				half3 normal = UnpackNormal(bump);
				normal.xy *= bumpStrength;
				return normalize(normal);
			}

			TEXTURE2D(_ColorTexture); SAMPLER(sampler_ColorTexture);

			half4 Frag(VaryingsUnder input) : SV_Target
			{
				float2 uv = input.screenPos.xy / input.screenPos.w;

				half3 distort = PerPixelBump(_DistortMap, uv.xyxy * _DistortMap_ST.xyxy + _WaveTime * _DistortMap_ST.w, _DistortMap_ST.z);

				half4 color = SAMPLE_TEXTURE2D(_ColorTexture, sampler_ColorTexture, uv + distort.xy);

				float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv + distort.xy), _ZBufferParams);

				return lerp(_BaseColor, color, saturate(exp(-1000 * depth * _DepthFade)));
			}
			ENDHLSL
		}
	}
}
