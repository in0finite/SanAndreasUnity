// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LemonSpawn/LazyClouds" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
//	    Tags {"Queue"="Transparent-1" "IgnoreProjector"="True" "RenderType"="Transparent"}
	    Tags {"Queue"="Transparent+100" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 400


		Lighting On
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
       Pass
         {

	Tags { "LightMode" = "ForwardBase" }
         
             CGPROGRAM
             
             #pragma target 3.0
             #pragma fragmentoption ARB_precision_hint_fastest
             
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile_fwdbase
                         
             #include "UnityCG.cginc"
             #include "AutoLight.cginc"
        sampler2D _MainTex;
		float ls_time;
		float ls_cloudscale;
		float ls_cloudscattering;
		float ls_cloudintensity;
		float ls_cloudsharpness;
		float ls_shadowscale;
		float ls_distScale;
		float ls_cloudthickness;
		float3 ls_cloudcolor;
		
	
		float getCloud(float2 uv, float scale, float disp) {
					float y = 0.0f;
					// Perlin octaves
					int NN = 5;
					for(int i=0;i < NN; i++) {
						float k = scale*i  + 0.11934;
						y+= tex2D( _MainTex, k*uv + float2(0.1234*i*ls_time*0.015 - 0.04234*i*i*ls_time*0.015 + 0.9123559 + 0.23411*i , 0.31342  + 0.5923*i + disp) ).x;
					}
					// Normalize
					y /= 0.5f*NN;
					return clamp( pow(ls_cloudscattering/y, ls_cloudsharpness),0,1.0);
				}
			
	 	// returns cloud value, outputs normal to N. 
	 	
		float getNormal(float2 uv, float scale, float dst, out float3 n, float nscale, float disp) {
					float height = getCloud(uv, scale, disp);
					int N =5;
					for (int i=0;i<N;i++) {
					
						float2 du1 = float2(dst*cos((i)*2*3.14159 / (N)), dst*sin(i*2*3.14159/(N)));
						float2 du2 = float2(dst*cos((i+1)*2*3.14159 / (N)), dst*sin((i+1)*2*3.14159/(N)));
						
						float hx = getCloud(uv + du1, scale, disp);
						float hy = getCloud(uv + du2, scale, disp);
					
						float3 d2 = float3(0,height*nscale,0) - float3(du1.x,hx*nscale,du1.y);
						float3 d1 = float3(0,height*nscale,0) - float3(du2.x,hy*nscale,du2.y);
					
						n = n + normalize(cross(d1,d2));
					}
					n = normalize(n);
					return height;
					
		}
				
				
             struct v2f
             {
                 float4 pos : POSITION;
                 float4 texcoord : TEXCOORD0;
                 float3 normal : TEXCOORD1;
                 float4 uv : TEXCOORD2;
                 float3 worldPosition: TEXCOORD3;
 
                 LIGHTING_COORDS(3,4)
             };
              
             v2f vert (appdata_base v)
             {
                 v2f o;
                 o.pos = UnityObjectToClipPos( v.vertex);
                 o.uv = v.texcoord;
                 o.normal = normalize(v.normal).xyz;
                 o.texcoord = v.texcoord;
 				 o.worldPosition = mul (unity_ObjectToWorld, v.vertex).xyz;

                 TRANSFER_VERTEX_TO_FRAGMENT(o);
                 
                 return o;
             }
             
		fixed4 frag(v2f IN) : COLOR {
			float3 worldSpacePosition = IN.worldPosition;
			float3 N;
			float3 lightDir = _WorldSpaceLightPos0;
					
			float3 viewDirection = normalize(_WorldSpaceCameraPos - worldSpacePosition);
			float dist = clamp(1.0/pow(length(0.5 + 0.0001*ls_distScale*(_WorldSpaceCameraPos - worldSpacePosition)),1.0),0,1);
 			float2 newPos = worldSpacePosition.xz*0.00005;
 					
			float x = getNormal(newPos, 1.73252*ls_cloudscale*0.1, 0.005*ls_shadowscale, N, 0.05*ls_shadowscale, worldSpacePosition.y/1381.1234f + ls_time*0.0002);//getCloud(IN.uv, 1.729134);
			float albedoColor = x*ls_cloudcolor;
			float3 norm=float3(0,1,0);
			float globalLight = saturate(dot(norm, lightDir));
			if (IN.normal.y<0) discard;
			float spec = pow(max(0.0, dot(
                  reflect(-lightDir, N), 
                  viewDirection)), 2);
            // spec = 1;
            float  NL = 0.3*ls_cloudintensity*(1 + spec + saturate((pow((dot(-N, lightDir)),1))));
			albedoColor*=NL*globalLight;
			
			float4 c = float4(0,0,0,0);
			c.a = ls_cloudthickness*pow(x,2)*dist;
			c.rgb = albedoColor;
			return c;
             }
             ENDCG
         }
     }
 Fallback "Diffuse"
 }