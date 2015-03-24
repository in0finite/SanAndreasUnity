Shader "SanAndreasUnity/Default"
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
        Tags { "RenderType" = "AlphaTest" }
        LOD 200
        
        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows alphatest:_AlphaCutoff
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MaskTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            float alpha = tex2D(_MaskTex, IN.uv_MainTex).a;

            o.Metallic = 0;
            o.Smoothness = 0;
            o.Alpha = alpha;
        }

        ENDCG
    } 
    FallBack "Diffuse"
}
