//////////////////////////////////////////////////////
// MK Glow Selective Render Shader    				//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de | www.michaelkremmel.store //
// Copyright © 2017 All rights reserved.            //
//////////////////////////////////////////////////////
Shader "Hidden/MK/Glow/SelectiveRender"
{
	SubShader 
	{
		Tags { "RenderType"="MKGlow" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Pass 
		{
			ZTest LEqual  
			Fog { Mode Off }
			Cull Back
			Lighting Off
			ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 2.0
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"
			
			uniform sampler2D _MKGlowTex;
			uniform float4 _MKGlowTex_ST;
			uniform fixed4 _MKGlowColor;
			uniform half _MKGlowPower;
			uniform half _MKGlowTexPower;
			uniform fixed4 _Color;
			
			struct Input
			{
				float2 texcoord : TEXCOORD0;
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct Output 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			Output vert (Input i)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				Output o;
				UNITY_INITIALIZE_OUTPUT(Output,o);
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos (i.vertex);
				o.uv = i.texcoord.xy;
				return o;
			}

			fixed4 frag (Output i) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 glow = tex2D(_MKGlowTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _MKGlowTex_ST));
				glow.rgb *= (_MKGlowColor * _MKGlowPower);
				glow.a = _Color.a;
				return glow;
			}
			ENDCG
		}
	}
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="MKGlowUIDefault" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				float2 uv_MKGlowTex : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			uniform sampler2D _MKGlowTex;
			uniform float4 _MKGlowTex_ST;
			uniform fixed4 _MKGlowColor;
			uniform half _MKGlowPower;
			uniform half _MKGlowTexPower;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = IN.texcoord;
				OUT.uv_MKGlowTex = TRANSFORM_TEX(IN.texcoord, _MKGlowTex);
				
				OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				
				fixed4 glow = tex2D(_MKGlowTex, IN.uv_MKGlowTex);
				glow.rgb *= (_MKGlowColor * _MKGlowPower);
				glow.a = _Color.a;

				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				glow.a = color.a;

				return glow;
			}
		ENDCG
		}
	}
	SubShader 
	{
		Tags { "RenderType"="MKGlowNC" "Queue"="Transparent"}
		Blend SrcAlpha OneMinusSrcAlpha
		Pass 
		{
			ZTest LEqual  
			Fog { Mode Off }
			Cull Off
			Lighting Off
			ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 2.0
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"
			
			uniform sampler2D _MKGlowTex;
			uniform float4 _MKGlowTex_ST;
			uniform fixed4 _MKGlowColor;
			uniform half _MKGlowPower;
			uniform half _MKGlowTexPower;
			uniform fixed4 _Color;
			
			struct Input
			{
				float2 texcoord : TEXCOORD0;
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct Output 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			Output vert (Input i)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				Output o;
				UNITY_INITIALIZE_OUTPUT(Output,o);
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos (i.vertex);
				o.uv = i.texcoord.xy;
				return o;
			}

			fixed4 frag (Output i) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				fixed4 glow = tex2D(_MKGlowTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _MKGlowTex_ST));
				glow.rgb *= (_MKGlowColor * _MKGlowPower);
				glow.a = _Color.a;
				return glow;
			}
			ENDCG
		}
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		Pass 
		{
			Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 2.0
			#pragma multi_compile_instancing
			
			#include "UnityCG.cginc"

			struct Input
			{
				float2 texcoord : TEXCOORD0;
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct Output 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			Output vert (Input i)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				Output o;
				UNITY_INITIALIZE_OUTPUT(Output,o);
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos (i.vertex);
				o.uv = i.texcoord;
				return o;
			}

			fixed4 frag (Output i) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				return fixed4(0,0,0,0);
			}
			
			ENDCG
		}
	}
	SubShader 
	{
		Tags { "RenderType"="Transparent" }
		Pass 
		{
			Fog { Mode Off }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 2.0
			#pragma multi_compile_instancing
			
			uniform fixed4 _Color;

			#include "UnityCG.cginc"

			struct Input
			{
				float2 texcoord : TEXCOORD0;
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct Output 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			Output vert (Input i)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				Output o;
				UNITY_INITIALIZE_OUTPUT(Output,o);
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos (i.vertex);
				o.uv = i.texcoord;
				o.color = _Color;
				return o;
			}

			fixed4 frag (Output i) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				return fixed4(0,0,0,i.color.a);
			}
			
			ENDCG
		}
	} 
	
	SubShader 
	{
		Tags { "RenderType"="TransparentCutout" }
		Pass 
		{
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma target 2.0
			#pragma multi_compile_instancing
			
			uniform fixed4 _Color;

			#include "UnityCG.cginc"

			struct Input
			{
				float2 texcoord : TEXCOORD0;
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct Output 
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			Output vert (Input i)
			{
				UNITY_SETUP_INSTANCE_ID(i);
				Output o;
				UNITY_INITIALIZE_OUTPUT(Output,o);
				UNITY_TRANSFER_INSTANCE_ID(i,o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos (i.vertex);
				o.uv = i.texcoord;
				return o;
			}

			fixed4 frag (Output i) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				return fixed4(0,0,0,i.color.a);
			}
			
			ENDCG
		}
	} 
} 

