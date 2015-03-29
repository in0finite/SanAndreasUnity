Shader "SanAndreasUnity/TransparentNoBackCull"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MaskTex ("Mask (A)", 2D) = "white" {}
        _NoiseTex ("Noise (A)", 2D) = "white" {}
        
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
        Cull Off
                
        CGPROGRAM

        #pragma surface surf Lambert alpha:fade noshadow
        #pragma target 3.0

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
