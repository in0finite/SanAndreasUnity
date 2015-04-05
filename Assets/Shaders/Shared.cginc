sampler2D _MainTex;
sampler2D _MaskTex;

#ifdef ENV_MAP
float _Specular;
float _Smoothness;
#endif

#ifdef FADE
sampler2D _NoiseTex;

half _Fade;
#endif

struct Input
{
    float2 uv_MainTex;
    float4 screenPos;
    float4 color : COLOR;
};

void surf(Input IN, inout SurfaceOutputStandardSpecular o)
{
#ifdef FADE
    half noise = tex2D(_NoiseTex, IN.screenPos.xy / IN.screenPos.w).a * .99;
    half fade = half(_Fade < 0 ? noise > 1 + _Fade : noise < _Fade);
#else
    half fade = 1;
#endif

    half3 clr = tex2D(_MainTex, IN.uv_MainTex).rgb;
    half mask = tex2D(_MaskTex, IN.uv_MainTex).a;

    o.Albedo = clr * IN.color.rgb;
    o.Alpha = fade * mask * IN.color.a;
    
#ifdef ENV_MAP
    o.Specular = _Specular;
    o.Smoothness = _Smoothness;
#else
    o.Specular = 0;
    o.Smoothness = 0;
#endif
}
