sampler2D _MainTex;
sampler2D _MaskTex;
sampler2D _NoiseTex;

half _Fade;

struct Input
{
    float2 uv_MainTex;
    float4 screenPos;
    float4 color : COLOR;
};

void surf(Input IN, inout SurfaceOutput o)
{
    half noise = tex2D(_NoiseTex, IN.screenPos.xy / IN.screenPos.w).a * .99;
    half fade = half(_Fade < 0 ? noise > 1 + _Fade : noise < _Fade);

    half3 clr = tex2D(_MainTex, IN.uv_MainTex).rgb;
    half mask = tex2D(_MaskTex, IN.uv_MainTex).a;

    o.Albedo = clr * IN.color.rgb;
    o.Alpha = fade * mask * IN.color.a;

    o.Specular = 0;
    o.Gloss = 0;
}
