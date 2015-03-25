sampler2D _MainTex;
sampler2D _MaskTex;

struct Input
{
    float2 uv_MainTex;
    float4 color : COLOR;
};

half _Glossiness;
half _Metallic;
fixed4 _Color;

void surf(Input IN, inout SurfaceOutputStandard o)
{
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * IN.color;
    o.Albedo = c.rgb;

    o.Metallic = 0;
    o.Smoothness = 0;
    o.Alpha = tex2D(_MaskTex, IN.uv_MainTex).a * IN.color.a;
}
