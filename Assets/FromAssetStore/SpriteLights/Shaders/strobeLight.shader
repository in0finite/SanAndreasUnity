Shader "Lights/Strobe"{

    Properties{

        _MainTex ("Texture", 2D) = "white" {} 
        [HDR]_FrontColor ("Front Color", Color) = (0.5,0.5,0.5,0.5)
        _Scale ("Scale", FLOAT) = 1.0
        _MinPixelSize ("Minimum screen size", FLOAT) = 5.0
        _Speed ("Speed", Float) = 1   
        _Persistence ("Persistence", Range(0.01, 1)) = 1       
        _Attenuation ("Attenuation", Range(0.01, 1)) = 0.37
        _Brightness ("Brightness", Range(0, 1)) = 0
        //_Time2 ("Time", Float) = 1 //For debugging.
    }	

    SubShader{

        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha One
        AlphaTest Greater .01
        ColorMask RGB
        Lighting Off ZWrite Off     
		
        Pass{

            CGPROGRAM
            
            #include "UnityCG.cginc"
            #include "lightFunctions.cginc"

            #pragma vertex vert  
            #pragma fragment frag 
            #pragma multi_compile_fog //Enable fog.
            #pragma glsl_no_auto_normalization
          //#pragma enable_d3d11_debug_symbols //For debugging.

            uniform sampler2D _MainTex;        
            half4 _FrontColor;
            float _Scale;       
            float _Speed;
            float _Persistence;
            float _MinPixelSize;
           //float _Time2; //For debugging.

            //These global variables are set from a Unity script.
            float _ScaleFactor; 
            float _GlobalBrightnessOffset;
            float _StrobeTimeStep; //0.04545454545

            float _Attenuation;
            float _Brightness;

            struct vertexInput {

                float4 center : POSITION; //Mesh center position is stored in the position channel (vertices in Unity).
                float4 corner : TANGENT; //Mesh corner is stored in the tangent channel (tangent in Unity). 
                float4 normal : NORMAL; //Rotation forward vector is stored in the Normal channel (normals in Unity).
                half4 id : COLOR; //The id is stored in the Color channel (colors32 in Unity). color.x = lightID, color.y = groupID  
                float2 uvs : TEXCOORD0; //Texture coordinates (uv in Unity).                
            };

            struct vertexOutput{

                float4 pos : SV_POSITION;
                half4 color : COLOR;
                float2 uvs : TEXCOORD0;

                //This is not a UV coordinate but it is just used to pass some variables
                //from the vertex shader to the fragment shader.
                //x = gain.
                float2 container : TEXCOORD1;

                //Enable fog.
                UNITY_FOG_COORDS(2)
            };			

            vertexOutput vert(vertexInput input){

                vertexOutput output;
                float gain;
                float distanceGain;
                float angleGain;
                float dotProduct;
                float3 viewDir;
                float f = 1.0f;
                float scale;
                float side;
                float4 invisibleColor = float4(0, 0, 0, 0);

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

                //Make sure the angle is always positive.
                dotProduct = abs(dotProduct); 

                //Convert from range -1, 1 to 0, 1
                float clampedSide = clamp(side, 0.0f, 1.0f);

                //Make invisible when viewed from the back.
                output.color = lerp(invisibleColor, _FrontColor, clampedSide);

                //Use a Phase Function to simulate the light lens shape and its effect it has on the light brightness.
                //To visualize a phase function, plot it on a polar graph. A nice online grapher is available at fooplot.com  
                //angleGain = GetRoundLobe(dotProduct);
                angleGain = GetEggLobe(dotProduct);
                //angleGain = GetEqualLobe();           
                //angleGain = GetTearDropLobe(dotProduct);

                //Calculate the scale. If the light size is smaller then one pixel, scale it up
                //so it remains at least one pixel in size.
                scale = ScaleUp(distance, _ScaleFactor, _Scale, angleGain, _MinPixelSize);

                //Get the fractional part of the time. So 1.23 becomes 0.23     
                //The group id is added to the time to create a more random look.  
                //float fracTime = frac((_Time2 + input.id.y) * _Speed); //For debugging.
                float fracTime = frac((_Time.y + input.id.y) * _Speed);  

                //Output 1 if fracTime is bigger then the id, otherwise output 0.
                //A and B look like this after the step functions:
                //_____-------
                //-------_____
                float A = step(input.id.x, fracTime);
                float B = 1.0f - step((input.id.x + (_StrobeTimeStep * _Persistence)), fracTime);

                //The time that both A and B are true is the time the strobe should be on.
                f = (A == B);

                //Switch the strobe off by setting the scale to 0. This avoids using if statements.
                scale = lerp(0.0f, scale, f);

                //Get the vertex offset to shift and scale the light.
                float4 offset = GetOffset(scale, input.corner * _Scale);

                //Place the vertex by moving it away from the center.
                //output.pos = mul(UNITY_MATRIX_MVP, input.center + offset);
                //Rotate the billboard towards the camera.
                output.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, input.center) + offset);

                //Far away lights should be less bright. Attenuate with the inverse square law.
                distanceGain = Attenuate(distance, _Attenuation);

                //Merge the distance gain (attenuation), angle gain (lens simulation), and light brightness into a single gain value.
                gain = MergeGain(distanceGain, angleGain, _GlobalBrightnessOffset, _Brightness);

                //Send the gain to the fragment shader.
                output.container = float2(gain, 0.0f);

                //UV mapping.
                output.uvs = input.uvs;

                //Enable fog.
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
