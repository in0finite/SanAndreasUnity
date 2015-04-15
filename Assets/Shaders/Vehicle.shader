Shader "SanAndreasUnity/Vehicle"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A)", 2D) = "white" {}
        _NoiseTex ("Noise (A)", 2D) = "white" {}

        _Specular ("Specular", Range(0.0, 1.0)) = 0.5
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5
        _Color ("Color", Color) = (1, 1, 1, 1)

        _AlphaCutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        LOD 200
        
        CGPROGRAM

        #pragma surface surf StandardSpecular addshadow alphatest:_AlphaCutoff
        #pragma target 3.0

        #define VEHICLE

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
