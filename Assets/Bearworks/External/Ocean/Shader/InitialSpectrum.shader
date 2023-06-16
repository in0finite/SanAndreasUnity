Shader "URPOcean/InitialSpectrum" {


	SubShader {
	
		Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

		HLSLPROGRAM
		#include "NeoInclude.hlsl"
		#pragma vertex vert_img
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_nicest
		#pragma target 3.0 
		#pragma exclude_renderers gles

		#define WAVE_KM 370.0
		#define WAVE_CM 0.23
	    #define twoPI UNITY_PI * 2
		
		float Omega;
		float windSpeed;
		float waveDirFlow;
		float waveAngle;
		float waveAmp;
		float4 sampleFFTSize;
		float fftresolution;
		//float _Offset;

		//to avoid unity gles build error
	    float tanh_es(float x)
		{
			float ex = exp(x);
			float e_x = exp(-x);
			 
			return (ex - e_x) / (ex + e_x);
		}

        float sqr(float x) { return x * x; }

        float omegak(float k) { return sqrt(9.81 * k * (1.0 + sqr(k / WAVE_KM))); } // Eq 24

#if 1
        float Spectrum(float kx, float ky)
        {
            //I know this is a big chunk of ugly math but dont worry to much about what it all means
            //It recreates a statistcally representative model of a wave spectrum in the frequency domain.

            float U10 = windSpeed;

            // phase speed
            float k = sqrt(kx * kx + ky * ky);
            float c = omegak(k) / k;

            // spectral peak
            float kp = 9.81 * sqr(Omega / U10); // after Eq 3
            float cp = omegak(kp) / kp;

            // friction velocity
            float z0 = 3.7e-5 * sqr(U10) / 9.81 * pow(saturate(U10 / cp), 0.9); // Eq 66
            float u_star = 0.41 * U10 / log(10.0 / z0); // Eq 60

            float Lpm = exp(-5.0 / 4.0 * sqr(kp / k)); // after Eq 3
            float gamma = (Omega < 1.0) ? 1.7 : 1.7 + 6.0 * log(Omega); // after Eq 3 // log10 or log?
            float sigma = 0.08 * (1.0 + 4.0 / pow(Omega, 3.0)); // after Eq 3
            float Gamma = exp(-1.0 / (2.0 * sqr(sigma)) * sqr(sqrt(k / kp) - 1.0));
            float Jp = pow(saturate(gamma), Gamma); // Eq 3
            float Fp = Lpm * Jp * exp(-Omega / sqrt(10.0) * (sqrt(k / kp) - 1.0)); // Eq 32
            float alphap = 0.006 * sqrt(Omega); // Eq 34
            float Bl = 0.5 * alphap * cp / c * Fp; // Eq 31

            float alpham = 0.01 * (u_star < WAVE_CM ? 1.0 + log(u_star / WAVE_CM) : 1.0 + 3.0 * log(u_star / WAVE_CM)); // Eq 44
            float Fm = exp(-0.25 * sqr(k / WAVE_KM - 1.0)); // Eq 41
            float Bh = 0.5 * alpham * WAVE_CM / c * Fm * Lpm; // Eq 40 (fixed)

            float a0 = log(2.0) / 4.0;
            float ap = 4.0;
            float am = 0.13 * u_star / WAVE_CM; // Eq 59
            float Delta = saturate(tanh_es(a0 + ap * pow(c / cp, 2.5) + am * pow(WAVE_CM / c, 2.5))); // Eq 57

            float cosphi = dot(float2(sin(waveAngle * UNITY_PI /180 + UNITY_PI), cos(waveAngle * UNITY_PI /180 + UNITY_PI)), normalize(float2(kx, ky)));

            //wind angle
            if (cosphi < waveDirFlow - 1) return 0.0;

            Bl *= 2.0;
            Bh *= 2.0;

            return max(waveAmp * (Bl + Bh) * (1.0 + Delta * cosphi) / (twoPI * sqr(sqr(k))), 0); // Eq 67
        }
#else
		float Spectrum(float kx, float ky)
		{

			// phase speed
			float k = (kx * kx + ky * ky);

			float cosphi = dot(float2(sin(waveAngle * UNITY_PI / 180 + UNITY_PI), cos(waveAngle * UNITY_PI / 180 + UNITY_PI)), normalize(float2(kx, ky)));

			//wind angle
			if (cosphi < waveDirFlow - 1) return 0.0;

			return max(waveAmp * exp(-1 / sqr(windSpeed * k)) / sqr(9.81 * Omega * k * k), 0); // Eq 67
		}
#endif

        float2 GetSpectrumSample(float2 uv, float lengthScale, float kMin, float2 rnd)
        {
            float kx = uv.x * fftresolution * lengthScale;
            float ky = uv.y * fftresolution * lengthScale;
            float2 result = 0;

			float S = Spectrum(kx, ky);
#if 1
			//A simple fix is to suppress waves smaller that a small length
			float h = sqrt(S / 2.0) * lengthScale * sqrt(-2 * log(clamp(rnd.y, 0.001, 1)));
#else
			float h = sqrt(S / 2.0) * lengthScale;
#endif
			//rand Phase
			float phi = rnd.x * twoPI;

            //if (abs(kx) >= kMin || abs(ky) >= kMin)
            {
                result.x = h * cos(phi);
                result.y = h * sin(phi);
            }

            return result;
        }

		 float randrg(float2 co)
		 {
			 return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
		 }

		 float randba(float2 co)
		 {
			 return frac(cos(dot(co, float2(20.983,97.081))) * 43758.5453);
		 }

		float4 frag (v2f_img i) : SV_Target {

			float2 uv = i.uv;
			
            uv.x = uv.x > 0.5 ? uv.x - 1.0 : uv.x;
            uv.y = uv.y > 0.5 ? uv.y - 1.0 : uv.y;
			
 			float2 rnd = float2(randrg(uv), randba(uv));

            float2 rg = GetSpectrumSample(uv, sampleFFTSize.x, sampleFFTSize.z, rnd.yx);
            float2 ba = GetSpectrumSample(uv, sampleFFTSize.y, sampleFFTSize.w, rnd.xy);

			return float4(rg, ba);
		}
		ENDHLSL
		}
		
	} 

Fallback off
}
