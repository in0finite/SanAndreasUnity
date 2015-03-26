Shader "SanAndreasUnity/Transparent"
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

        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM

        #pragma surface surf Lambert alpha noshadow
        #pragma target 3.0

        #include "Shared.cginc"

        ENDCG
    } 

    FallBack "Diffuse"
}
