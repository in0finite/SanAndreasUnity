sampler2D _MainTex;
sampler2D _MaskTex;

fixed4 _Color;

#ifdef VEHICLE
int _CarColorIndex;

fixed3 _CarColor1;
fixed3 _CarColor2;
fixed3 _CarColor3;
fixed3 _CarColor4;

fixed3 _HeadLightColor;
fixed3 _TailLightColor;

fixed4 _Lights;

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
    fixed noise = tex2D(_NoiseTex, IN.screenPos.xy / (IN.screenPos.w == 0 ? 1 : IN.screenPos.w)).a * .99;
    fixed fade = fixed(_Fade < 0 ? noise > 1 + _Fade : noise < _Fade);
#else
    fixed fade = 1;
#endif

    fixed3 clr = tex2D(_MainTex, IN.uv_MainTex).rgb;
    fixed mask = tex2D(_MaskTex, IN.uv_MainTex).a;
    
#ifdef VEHICLE
    fixed3 carColors[9] = {
        fixed3(1, 1, 1),
        _CarColor1,
        _CarColor2,
        _CarColor3,
        _CarColor4,
        _HeadLightColor,
        _HeadLightColor,
        _TailLightColor,
        _TailLightColor
    };

    fixed carEmission[9] = {
        0,
        0,
        0,
        0,
        0,
        exp(_Lights.x * 2) - 1,
        exp(_Lights.y * 2) - 1,
        exp(_Lights.z * 2) - 1,
        exp(_Lights.w * 2) - 1
    };
#endif

    o.Albedo = clr
#ifdef VEHICLE
        * carColors[_CarColorIndex]
#endif
        * IN.color.rgb * _Color.rgb;

    o.Alpha = fade * mask * IN.color.a * _Color.a;
    
#ifdef VEHICLE
    o.Metallic = _Metallic * o.Alpha;
    o.Smoothness = _Smoothness;
    o.Emission = carEmission[_CarColorIndex] * o.Albedo;
#else
    o.Metallic = 0;
    o.Smoothness = 0;
#endif
}
