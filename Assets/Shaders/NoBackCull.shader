Shader "SanAndreasUnity/NoBackCull"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A)", 2D) = "white" {}
        _AlphaCutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        LOD 200

        Cull Off
        
        CGPROGRAM

        #pragma surface surf Lambert addshadow alphatest:_AlphaCutoff
        #pragma target 3.0

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
