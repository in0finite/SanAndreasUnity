Shader "SanAndreasUnity/VehicleTransparent"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A)", 2D) = "white" {}
        _NoiseTex ("Noise (A)", 2D) = "white" {}

        _Specular ("Specular", Range(0.0, 1.0)) = 0.25
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.75
        _Color ("Color", Color) = (1, 1, 1, 1)
        
        _CarColorIndex ("Car Color Index", Range(0, 3)) = 0
        
        _CarColor1 ("Car Color 1", Color) = (1, 1, 1, 1)
        _CarColor2 ("Car Color 2", Color) = (1, 1, 1, 1)
        _CarColor3 ("Car Color 3", Color) = (1, 1, 1, 1)
        _CarColor4 ("Car Color 4", Color) = (1, 1, 1, 1)

        _Alpha ("Alpha", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        LOD 200
        Cull Off
        
        CGPROGRAM

        #pragma surface surf StandardSpecular addshadow alpha
        #pragma target 3.0

        #define VEHICLE
        #define ALPHA

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
