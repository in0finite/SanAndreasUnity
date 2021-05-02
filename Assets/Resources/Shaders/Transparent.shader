Shader "SanAndreasUnity/Transparent"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A)", 2D) = "white" {}
        _NoiseTex ("Noise (A)", 2D) = "white" {}
        
        _Color ("Color", Color) = (1, 1, 1, 1)
        
        _Fade ("Fade", Range(-1.0, 1.0)) = 1
    }

    SubShader
    {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "ForceNoShadowCasting" = "True"
        }

        LOD 200
        Cull Front
        ZWrite Off
                
        CGPROGRAM

        #pragma vertex vert
        #pragma surface surf Standard alpha:fade noshadow
        #pragma target 3.0
        
        #define FADE

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
