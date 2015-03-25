Shader "SanAndreasUnity/NoBackCull"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A)", 2D) = "white" {}
        _AlphaCutoff ("Alpha Cutoff", float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        Cull Off
        
        CGPROGRAM

        #pragma surface surf Standard addshadow alphatest:_AlphaCutoff
        #pragma target 3.0

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
