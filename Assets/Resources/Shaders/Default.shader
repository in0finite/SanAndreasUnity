Shader "SanAndreasUnity/Default"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A)", 2D) = "white" {}
        _NoiseTex ("Noise (A)", 2D) = "white" {}
        
        _Color ("Color", Color) = (1, 1, 1, 1)

        _Fade ("Fade", Range(-1.0, 1.0)) = 1

        _AlphaCutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        LOD 200
        Cull Front

        CGPROGRAM

        #pragma vertex vert
        #pragma surface surf Standard addshadow alphatest:_AlphaCutoff
        #pragma target 3.0

        #define FADE

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
