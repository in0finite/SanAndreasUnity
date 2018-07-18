Shader "MK/Glow/Selective/Transparent/Diffuse" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,0.25)
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	
	_MKGlowColor ("Glow Color", Color) = (1,1,1,0.5)
	_MKGlowPower ("Glow Power", Range(0.0,5.0)) = 2.5
	_MKGlowTex ("Glow Texture", 2D) = "black" {}
	_MKGlowTexColor ("Glow Texture Color", Color) = (1,1,1,0.25)
	_MKGlowTexStrength ("Glow Texture Strength ", Range(0.0,10.0)) = 1.0
	
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="MKGlow"}
	LOD 200

CGPROGRAM
#pragma surface surf Lambert alpha:fade
#pragma target 2.0

sampler2D _MainTex;
fixed4 _Color;

sampler2D _MKGlowTex;
half _MKGlowTexStrength;
fixed4 _MKGlowTexColor;

struct Input {
	float2 uv_MainTex;
	float2 uv_MKGlowTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	fixed3 d = tex2D(_MKGlowTex, IN.uv_MKGlowTex) * _MKGlowTexColor;
	c.rgb += (d.rgb * _MKGlowTexStrength);
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Legacy Shaders/Transparent/VertexLit"
}