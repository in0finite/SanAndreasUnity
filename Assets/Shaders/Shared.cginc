sampler2D _MainTex;
sampler2D _MaskTex;

fixed4 _Color;

#ifdef VEHICLE
int _CarColorIndex;

fixed3 _CarColor1;
fixed3 _CarColor2;
fixed3 _CarColor3;
fixed3 _CarColor4;

float _Metallic;
float _Smoothness;
#endif

#ifdef FADE
sampler2D _NoiseTex;

fixed _Fade;
#endif

struct Input
{
    float2 uv_MainTex;
    float4 screenPos;
    float4 color : COLOR;
};

void surf(Input IN, inout SurfaceOutputStandard o)
{
#ifdef FADE
    fixed noise = tex2D(_NoiseTex, IN.screenPos.xy / IN.screenPos.w).a * .99;
    fixed fade = fixed(_Fade < 0 ? noise > 1 + _Fade : noise < _Fade);
#else
    fixed fade = 1;
#endif

    fixed3 clr = tex2D(_MainTex, IN.uv_MainTex).rgb;
    fixed mask = tex2D(_MaskTex, IN.uv_MainTex).a;

    o.Albedo = clr
#ifdef VEHICLE
        * lerp(_CarColor1, fixed3(1, 1, 1), clamp(abs(_CarColorIndex - 1), 0, 1))
        * lerp(_CarColor2, fixed3(1, 1, 1), clamp(abs(_CarColorIndex - 2), 0, 1))
        * lerp(_CarColor3, fixed3(1, 1, 1), clamp(abs(_CarColorIndex - 3), 0, 1))
        * lerp(_CarColor4, fixed3(1, 1, 1), clamp(abs(_CarColorIndex - 4), 0, 1))
#endif
        * IN.color.rgb * _Color.rgb;

    o.Alpha = fade * mask * IN.color.a * _Color.a;
    
#ifdef VEHICLE
    o.Metallic = _Metallic * o.Alpha;
    o.Smoothness = _Smoothness;
#else
    o.Metallic = 0;
    o.Smoothness = 0;
#endif
}
