sampler2D _MainTex;
sampler2D _MaskTex;

fixed4 _Color;

#ifdef VEHICLE
int _CarColorIndex;

fixed3 _CarColor;
fixed _CarEmission;

float _Metallic;
float _Smoothness;
#endif

#ifdef FADE
sampler2D _NoiseTex;

fixed _Fade;
#endif

float _NightMultiplier = 0.5;
float _HasNightColors = 0;

struct Input
{
    float2 uv_MainTex;
    #ifdef FADE
    float4 screenPos;
    #endif
    float4 color : COLOR;
};

void vert (inout appdata_full v) {

    float4 c;
    c.rg = v.texcoord1.xy;
    c.ba = v.texcoord2.xy;

    v.color = lerp(v.color, c, _NightMultiplier * _HasNightColors);
}

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
    


    o.Albedo = clr
#ifdef VEHICLE
        * _CarColor
#endif
        * IN.color.rgb * _Color.rgb;

    o.Alpha = fade * mask * IN.color.a * _Color.a;
    
#ifdef VEHICLE
    o.Metallic = _Metallic * o.Alpha;
    o.Smoothness = _Smoothness;
    o.Emission = _CarEmission * o.Albedo;
#else
    o.Metallic = 0;
    o.Smoothness = 0;
#endif
}
