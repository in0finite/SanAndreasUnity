sampler2D _MainTex;
sampler2D _MaskTex;

struct Input
{
    float2 uv_MainTex;
    float4 color : COLOR;
};

void surf(Input IN, inout SurfaceOutput o)
{
    o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb * IN.color.rgb;
    o.Alpha = tex2D(_MaskTex, IN.uv_MainTex).a * IN.color.a;

    o.Specular = 0;
    o.Gloss = 0;
}
