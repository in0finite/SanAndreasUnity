Shader "SanAndreasUnity/TransparentNoBackCull"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" }
        LOD 200
        
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        CGPROGRAM

        #pragma surface surf Lambert
        #pragma target 3.0

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
