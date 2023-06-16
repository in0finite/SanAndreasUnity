Shader "Hidden/XVolumeClouds" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
			
		Pass
		{
			ZTest off Cull Off ZWrite Off
			
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "VolumeCloudsCommon.hlsl"

			sampler2D _MainTex;
			
			sampler3D _Perlin3D;
			sampler3D _Detail3D;
			sampler2D _Coverage;
			sampler2D _Curl2D;

			float _StartHeight;
			float _AtmosphereThickness;
			float3 _CameraPosition;
			float _MaxDistance;

			float _CloudBottomFade;
			
			float3 _BaseOffset;
			float3 _DetailOffset;
			float2 _CoverageOffset;
			float _BaseScale;
			float _CoverageScale;
			float _HorizonFadeStartAlpha;
			float _HorizonFadeScalar;				
			float _LightScalar;
			float _AmbientScalar;
			float3 _CloudBaseColor;
			float3 _CloudTopColor;
			float3 _SunLightColor;
			float _SunRayLength;
			float _ConeRadius;
		 	float _MaxIterations;
			float _RayStepLength;
			float _SampleScalar;
			float _SampleThreshold;
			float _DetailScale;
			float _DetailStrength;
			float _ErosionEdgeSize;
			float _CloudDistortion;
			float _CloudDistortionScale;
			float _Density;

			float _LODDistance;

			float3 _Random0;
			float3 _Random1;
			float3 _Random2;
			float3 _Random3;
			float3 _Random4;
			float3 _Random5;

			clouds_v2f vert(appdata_img v)
			{
			   	clouds_v2f o;
				o.position = UnityObjectToClipPos( v.vertex);
				o.uv = v.texcoord;
				
				o.cameraRay = UVToCameraRay( o.uv);
				
			   	return o;
			}
			
			inline float NormalizedAtmosphereY( float3 ray)
			{
				float y = length(ray) - _StartHeight;
				return y / _AtmosphereThickness;
			}
			
			inline float GradientStep( float a)
			{
				return 1 - smoothstep(0, 1, a);
			}

			inline float SampleCoverage(float3 rayPos, inout float lod)
			{
				float depth = distance(rayPos, _CameraPosition) / _MaxDistance;
				lod = step(_LODDistance, depth);

				float2 uv = rayPos.xz * _CoverageScale + _CoverageOffset;

				float3 coverage = tex2Dlod(_Coverage, float4(uv, 0.0, lod)).rgb;

				return lerp(coverage.b, lerp(coverage.g, coverage.r, depth), depth);
			}
			
			inline float SmoothThreshold( float value, float threshold, float edgeSize)
			{
				return smoothstep( threshold, threshold + edgeSize, value);
			}

			inline float SampleCloud(float3 rayPos, float coverage, float csRayHeight, float lod)
			{
				float value = 0.0;
				float4 coord = float4(rayPos * _BaseScale + _BaseOffset, lod);
				float4 noiseSample = tex3Dlod(_Perlin3D, coord);

				float gradHeight = GradientStep(csRayHeight);

				float4 gradientScalar = float4( 1.0, gradHeight, gradHeight, gradHeight);

				noiseSample *= gradientScalar;

				float noise = saturate((noiseSample.r + noiseSample.g + noiseSample.b + noiseSample.a) / 4.0);

				noise *= gradHeight;

				noise = SmoothThreshold(noise, _SampleThreshold, _ErosionEdgeSize);
				noise = saturate(noise - (1.0 - coverage)) * coverage;
				
				if (noise > 0.0 && noise < 1.0 && lod == 0.0)
				{
					float4 distUV = float4(rayPos.xy * _CloudDistortionScale, 0.0, lod);
					float3 curl = tex2Dlod(_Curl2D, distUV) * 2.0 - 1.0;

					coord = float4(rayPos * _DetailScale, lod);
					coord.xyz += _DetailOffset;

					curl *= _CloudDistortion * csRayHeight;
					coord.xyz += curl;

					float3 detail = 1.0 - tex3Dlod(_Detail3D, coord);
					float detailValue = detail.r + detail.g + detail.b;
					detailValue /= 3.0;
					detailValue *= smoothstep( 1.0, 0.0, noise) * _DetailStrength;
					noise -= smoothstep(0, 1, detailValue);

					noise = saturate(noise);
				}

				return noise * _SampleScalar * smoothstep(0.0, _CloudBottomFade, csRayHeight);
			}

			float HenyeyGreensteinPhase( float cosAngle, float g)
			{
				const float g2 = g * g;
				return (1.0 - g2) / pow(max(1.0 + g2 - 2.0 * g * cosAngle, 1e-7), 1.5);
			}

			inline float BeerTerm( float densityAtSample)
			{
				return exp( -_Density * densityAtSample);
			}
			
			inline float PowderTerm( float densityAtSample, float cosTheta)
			{
				return saturate(1.0 - exp( -_Density * densityAtSample * 2.0));
			}

			inline float3 SampleLight( float3 origin, float originDensity, float3 cosAngle, float3 RandomUnitSphere[6])
			{
				const float iterations = 5.0;
#if 0
				float sunRayLength = InternalRaySphereIntersectLength(_StartHeight + _AtmosphereThickness, origin, _MainLightPosition.xyz);
				sunRayLength /= _AtmosphereThickness;
				sunRayLength /= (iterations + 1);
#else
				float sunRayLength = 1;
				sunRayLength /= iterations;
#endif

				float3 rayStep = _MainLightPosition.xyz * (sunRayLength * _SunRayLength);
				float3 rayPos = origin + rayStep;
				
				float atmosphereY = 0.0;
				float value = 0.0;
				float coverage;
				float coneRadius = 0.0;
				const float coneStep = sunRayLength * _ConeRadius;

				float thickness = 0.0;

				for( float i=0.0; i<iterations; i++)
				{
					rayPos += rayStep;
					atmosphereY = NormalizedAtmosphereY(rayPos);

					if (atmosphereY > 0)
					{
						float3 randomOffsetPos = rayPos + RandomUnitSphere[i] * coneRadius;
						float lod = 0;
						coverage = SampleCoverage(randomOffsetPos, lod);
						value = SampleCloud(randomOffsetPos, coverage, atmosphereY, lod);
						thickness += value;
					}

					coneRadius += coneStep;
				}

				float forwardP = HenyeyGreensteinPhase( cosAngle, 0.997);
				float backwardsP = HenyeyGreensteinPhase( cosAngle, -0.003);
				float P = (forwardP + backwardsP) / 2.0;

				return _SunLightColor.rgb * BeerTerm(thickness) * PowderTerm(originDensity, cosAngle) * P;
			}

			inline float3 SampleAmbientLight(float atmosphereY)
			{
				return lerp(_CloudBaseColor, _CloudTopColor, atmosphereY);
			}
			
			half4 frag(clouds_v2f i) : SV_Target
			{
				half4 color = half4( 0.0, 0.0, 0.0, 0.0);
				float3 rayDirection = normalize(i.cameraRay);

				float fade = smoothstep( 0,  _HorizonFadeScalar,  rayDirection.y);
			    fade = lerp(fade,1,_HorizonFadeStartAlpha);

				if( rayDirection.y > 0 && fade>0)
				{
					float2 uv = i.uv;
					float3 rayPos = InternalRaySphereIntersect(_StartHeight, _CameraPosition, rayDirection);
					float3 rayStep = rayDirection * _RayStepLength;

					float atmosphereY = 0.0;
					float cosAngle = dot( rayDirection, _MainLightPosition.xyz);

					const float3 RandomUnitSphere[6] = { _Random0, _Random1, _Random2, _Random3, _Random4, _Random5 };
					
					float lerpIterations = lerp(_MaxIterations * fade, _MaxIterations, rayDirection.y);
					rayStep *= _MaxIterations / max(lerpIterations, 0.01);
					for( float iI = 0 ; iI < lerpIterations; iI +=1)
					{
						if(color.a > 1.0 || atmosphereY > 1.0)
							break;
						
						float lod = 0;
						float coverage = SampleCoverage(rayPos, lod);
						float density = SampleCloud(rayPos, coverage, atmosphereY, lod);

						if(density > 0.0)
						{
							float3 ambientLight = SampleAmbientLight(atmosphereY);
							float3 sunLight = SampleLight(rayPos, density, cosAngle, RandomUnitSphere);

							sunLight *= _LightScalar;
							ambientLight *= _AmbientScalar;

							half3 light = sunLight + ambientLight;
							light *= (1 - exp(-density * _RayStepLength));
							color.rgb += light;
							color.a += (1.0 - color.a) * density;
						}

						rayPos += rayStep;
						atmosphereY = NormalizedAtmosphereY(rayPos);
					}
					
					color *= fade;
				}

				return color;
			}
			
			ENDHLSL
		}
	} 
}
