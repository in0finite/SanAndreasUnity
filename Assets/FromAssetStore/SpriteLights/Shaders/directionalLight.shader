Shader "Lights/Directional"{

    Properties{

        _MainTex ("Texture", 2D) = "white" {}
        [KeywordEnum(Teardrop, Round, Egg)] _Lobe ("Radiation pattern", Float) = 0    
        _MinPixelSize ("Minimum screen size", FLOAT) = 5.0
        _Attenuation ("Attenuation", Range(0.01, 1)) = 0.37
        _BrightnessOffset ("Brightness offset", Range(-1, 1)) = 0
    }	

    SubShader{

        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha One
        //Blend SrcAlpha OneMinusSrcAlpha
        AlphaTest Greater .01
        ColorMask RGB
        Lighting Off ZWrite Off        
		
        Pass{

            CGPROGRAM
            
            #include "UnityCG.cginc"
            #include "lightFunctions.cginc"

            #pragma vertex vert  
            #pragma fragment frag          
            #pragma multi_compile _LOBE_TEARDROP _LOBE_ROUND _LOBE_EGG
            #pragma multi_compile_fog //Enable fog
            #pragma glsl_no_auto_normalization
          //  #pragma enable_d3d11_debug_symbols //For debugging.
  
            uniform sampler2D _MainTex;        
            float _MinPixelSize;
            float _Attenuation;
            float _BrightnessOffset;

            //These global variables are set from a Unity script.
            float _ScaleFactor; 
            float _GlobalBrightnessOffset;

            struct vertexInput {

                float4 center : POSITION; //Mesh center position is stored in the position channel (vertices in Unity).
                float4 corner : TANGENT; //Mesh corner is stored in the tangent channel (tangent in Unity). The scale is stored in the w component.
                float4 normal : NORMAL; //Rotation forward vector is stored in the Normal channel (normals in Unity).
                float2 uvs : TEXCOORD0; //Texture coordinates (uv in Unity).   
                float2 RGback : TEXCOORD1; //RG(BA) back color is stored in a UV channel (uv2 in Unity). This channel is used as a rotation vector for PAPI lights.
                float2 BAback : TEXCOORD2; //(RG)BA back color is stored in a UV channel(uv3 in Unity). Alpha is for invisibility. This channel is used as a rotation vector for PAPI lights.
                half4 RGBAfront : COLOR; //The front color is stored in the Color channel (colors32 in Unity). Alpha is for brightness reduction (random brightness). This channel is used as an ID for strobe lights.
            };

            struct vertexOutput{

                float4 pos : SV_POSITION;
                float2 uvs : TEXCOORD0;                
                half4 color : COLOR;

                //This is not a UV coordinate but it is just used to pass some variables
                //from the vertex shader to the fragment shader. x = gain.
                float2 container : TEXCOORD1;

                //Enable fog
                UNITY_FOG_COORDS(2)
            };			

            vertexOutput vert(vertexInput input){

                vertexOutput output;
                float gain;
                float distanceGain;
                float angleGain;
                float dotProduct;
                float3 viewDir;
                float scale;
                float side; 

                //Get a vector from the vertex to the camera and cache the result.
                float3 objSpaceViewDir = ObjSpaceViewDir(input.center);

                //Get the distance between the camera and the light.
                float distance = length(objSpaceViewDir);

                viewDir = normalize(objSpaceViewDir);

                //This is used to determine the light viewing angle.
                dotProduct = dot(viewDir, input.normal); 

                //Make light visible from the backside as well. Use this rather than Cull Off because
                //that can't be switched off using a preprocessor.
                side = sign(dotProduct);

                //Convert from range -1, 1 to 0, 1
                float clampedSide = clamp(side, 0.0f, 1.0f);

                //Compose the back color. There is no float4 slot left, so we use a UV channel to store the colors.
                float4 RGBAback = float4(input.RGback.x, input.RGback.y, input.BAback.x, 0.2f);

                //Use a fixed alpha because we use the alpha value to modulate the gain instead.
                half4 RGBAfont = half4(input.RGBAfront.r, input.RGBAfront.g, input.RGBAfront.b, 0.2f);

                //Create a smooth transition between the two colors.  
                //output.color = Get015DegreeTransition(dotProduct, RGBAfont, RGBAback);
                //output.color = Get03DegreeTransition(dotProduct, RGBAfont, RGBAback);
                //output.color = Get06DegreeTransition(dotProduct, RGBAfont, RGBAback);
                //output.color = Get10DegreeTransition(dotProduct, RGBAfont, RGBAback);

                //Create a sharp transition between the two colors.
                output.color = lerp(RGBAback, RGBAfont, clampedSide);

                //Make sure the angle is always positive.
                dotProduct = abs(dotProduct); 

                //Use a Phase Function to simulate the light lens shape and its effect it has on the light brightness.  
                #if _LOBE_TEARDROP     
                    angleGain = GetTearDropLobe(dotProduct);
                #endif

                #if _LOBE_ROUND
                    angleGain = GetRoundLobe(dotProduct);
                #endif

                #if _LOBE_EGG
                    angleGain = GetEggLobe(dotProduct);
                #endif

                //Calculate the scale. If the light size is smaller then one pixel, scale it up
                //so it remains at least one pixel in size.
                scale = ScaleUp(distance, _ScaleFactor, input.corner.w, angleGain, _MinPixelSize);

                //Get the vertex offset to shift and scale the light.
                float4 offset = GetOffset(scale, input.corner);

                //Rotate the billboard towards the camera.
                output.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, input.center) + offset);

                //Far away lights should be less bright. Attenuate with the inverse square law.
                distanceGain = Attenuate(distance, _Attenuation);

                //Merge the distance gain (attenuation), angle gain (lens simulation), and light brightness into a single gain value.
                //Note that the individual light brightness is stored in the front color alpha channel.
                gain = MergeGain(distanceGain, angleGain, _GlobalBrightnessOffset, input.RGBAfront.a + _BrightnessOffset);

                //Send the gain to the fragment shader.
                output.container = float2(gain, 0.0f);

                //UV mapping.
                output.uvs = input.uvs;

                //Enable fog
                UNITY_TRANSFER_FOG(output, output.pos);

                return output;
            }

            half4 frag(vertexOutput input) : COLOR{

                //Compute the final color.
                //Note: input.container.x fetches the gain from the vertex shader. No need to calculate this for each fragment.
				half4 col = 2.0f * input.color * tex2D(_MainTex, input.uvs) * (exp(input.container.x * 5.0f));	
                
                //Enable fog. Use black due to the blend mode used.			
				UNITY_APPLY_FOG_COLOR(input.fogCoord, col, half4(0,0,0,0)); 

				return col;
            }

	        ENDCG
        }
    }
	
    FallBack "Diffuse"
}
