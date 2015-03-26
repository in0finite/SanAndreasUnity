Shader "SanAndreasUnity/TransparentNoBackCull"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A)", 2D) = "white" {}
    }

    SubShader
    {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "ForceNoShadowCasting" = "True"
        }
        
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        LOD 200
        CGPROGRAM

        #pragma surface surf Lambert alpha
        #pragma target 3.0

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
