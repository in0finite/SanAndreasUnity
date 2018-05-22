Shader "Lights/Omnidirectional"{

    Properties{

        _MainTex ("Light Texture", 2D) = "white" {} 
        [KeywordEnum(Doughnut, Equal)] _Lobe ("Radiation pattern", Float) = 0    
        _MinPixelSize ("Minimum screen size", FLOAT) = 5.0
        _Attenuation ("Attenuation", Range(0.01, 1)) = 0.37
        _BrightnessOffset ("Brightness offset", Range(-1, 1)) = 0
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
            #pragma multi_compile _LOBE_DOUGHNUT _LOBE_EQUAL
            #pragma multi_compile_fog //Enable fog
            #pragma glsl_no_auto_normalization
          //  #pragma enable_d3d11_debug_symbols //For debugging.
  
            uniform sampler2D _MainTex;        
            float _MinPixelSize;
            float angle;
            float _BrightnessOffset;
            float _Attenuation;

            //These global variables are set from a Unity script.
            float _ScaleFactor; 
            float _GlobalBrightnessOffset;

            struct vertexInput {

                float4 center : POSITION; //Mesh center position is stored in the position channel (vertices in Unity).
                float4 corner : TANGENT; //Mesh corner is stored in the tangent channel (tangent in Unity). The scale is stored in the w component.
                float4 normal : NORMAL; //Rotation forward vector is stored in the Normal channel (normals in Unity).
                float2 uvs : TEXCOORD0; //Texture coordinates (uv in Unity).   
                float2 RGback : TEXCOORD1; //RG(B) back color is stored in a UV channel (uv2 in Unity). 
                float2 up : TEXCOORD2; //Rotation up vector is stored in a UV channel (uv3 in Unity).   
                float2 z : TEXCOORD3; //Rotation up vector (3rd component) is stored in a UV channel (uv4 in Unity). The x component is used for the blue component of the back color.
                half4 RGBAfront : COLOR; //The front color is stored in the Color channel (colors32 in Unity). This channel is used as an ID for strobe lights.    
            };

            struct vertexOutput{

                float4 pos : SV_POSITION;
                float2 uvs : TEXCOORD0;
                half4 color : COLOR;

                //This is not a UV coordinate but it is just used to pass some variables
                //from the vertex shader to the fragment shader. x = gain.
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
                float scale;

                //Get a vector from the vertex to the camera and cache the result.
                float3 objSpaceViewDir = ObjSpaceViewDir(input.center);

                //Get the distance between the camera and the light.
                float distance = length(objSpaceViewDir);    
                
                //Use a fixed alpha because we use the alpha value to modulate the gain instead.
                half4 RGBAfont = half4(input.RGBAfront.r, input.RGBAfront.g, input.RGBAfront.b, 0.2f);  

                #if _LOBE_EQUAL
                    angleGain = GetEqualLobe();
                    output.color = RGBAfont;
                #else
                    //Compose the back color. There is no float4 slot left, so we use UV channels to store the colors.
                    float4 RGBAback = float4(input.RGback.x, input.RGback.y, input.z.x, 0.2f);  

                    float3 viewDir = normalize(objSpaceViewDir);

                    //Compose the up vector. A UV channel only holds two floats, 
                    //so we need to fetch the z coordinate from a third UV channel.
                    float3 upVector = float3(input.up.x, input.up.y, input.z.y);

                    //TODO: figure out a way to get the rotated normal from elsewhere instead.
                    float3 rotatedNormal = ProjectVectorOnPlane(upVector, viewDir);
                    dotProduct = dot(viewDir, rotatedNormal); 

                    //Use a Phase Function to simulate the light lens shape and its effect it has on the light brightness.            
                    angleGain = GetRoundLobe(dotProduct);
                    //angleGain = GetEggLobe(dotProduct);
                    //angleGain = GetEqualLobe();           
                    //angleGain = GetTearDropLobe(dotProduct);

                    float forwardDotProduct = dot(viewDir, input.normal); 

                    //Create a smooth transition between the two colors.  
                    //output.color = Get015DegreeTransition(forwardDotProduct, RGBAfont, RGBAback);
                    //output.color = Get03DegreeTransition(forwardDotProduct, RGBAfont, RGBAback);
                    //output.color = Get06DegreeTransition(forwardDotProduct, RGBAfont, RGBAback);
                    output.color = Get10DegreeTransition(forwardDotProduct, RGBAfont, RGBAback);
                #endif

                //Calculate the scale. If the light size is smaller then one pixel, scale it up
                //so it remains at least one pixel in size.
                scale = ScaleUp(distance, _ScaleFactor, input.corner.w, angleGain, _MinPixelSize);

                //Get the vertex offset to shift and scale the light.
                float4 offset = GetOffset(scale, input.corner);

                //Place the vertex by moving it away from the center.
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
