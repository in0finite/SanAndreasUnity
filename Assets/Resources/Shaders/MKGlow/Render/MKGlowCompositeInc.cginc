//////////////////////////////////////////////////////
// MK Glow Free Composite Shader Inc    			//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de | www.michaelkremmel.store //
// Copyright © 2017 All rights reserved.            //
//////////////////////////////////////////////////////
#ifndef MK_GLOW_COMPOSITE_INC
	#define MK_GLOW_COMPOSITE_INC

	uniform sampler2D _MainTex;
	uniform float2 _MainTex_TexelSize;
	uniform float4 _MainTex_ST;
	uniform sampler2D _MKGlowTexInner;
	uniform float2 _MKGlowTexInner_TexelSize;
	uniform float4 _MKGlowTexInner_ST;
	uniform half _GlowIntensityInner;

	uniform fixed3 _GlowTint;

	struct Input
	{
		float2 texcoord : TEXCOORD0;
		float4 vertex : POSITION;
	};
			
	struct Output 
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 uv1 : TEXCOORD1;
	};
			
	Output vert (Input i)
	{
		Output o;
		UNITY_INITIALIZE_OUTPUT(Output,o);
		o.pos = UnityObjectToClipPos (i.vertex);
		o.uv = i.texcoord.xy;
		o.uv1 = i.texcoord.xy;
		#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0)
				o.uv.y = 1-o.uv.y;
			if (_MKGlowTexInner_TexelSize.y < 0)
				o.uv1.y = 1-o.uv1.y;
		#endif
				
		return o;
	}

	fixed4 frag( Output i ) : SV_TARGET
	{
		fixed3 m = tex2D(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _MainTex_ST)).rgb;		
		fixed3 g = tex2D(_MKGlowTexInner, UnityStereoScreenSpaceUVAdjust(i.uv1.xy, _MKGlowTexInner_ST)).rgb * _GlowIntensityInner;

		return fixed4(g*_GlowTint+m,1);
	}
#endif