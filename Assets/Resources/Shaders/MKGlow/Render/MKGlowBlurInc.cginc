//////////////////////////////////////////////////////
// MK Glow Free Blur Shader Inc						//
//					                                //
// Created by Michael Kremmel                       //
// www.michaelkremmel.de | www.michaelkremmel.store //
// Copyright © 2017 All rights reserved.            //
//////////////////////////////////////////////////////
#ifndef MK_GLOW_BLUR_INC
	#define MK_GLOW_BLUR_INC

	v2fBlur vertBlur (appdata_img v) 
	{
		v2fBlur o;
		o.pos = UnityObjectToClipPos(v.vertex);

		o.uv.xy = v.texcoord.xy;

		#if V
			o.uv01 =  v.texcoord.xyxy + float4(_Offset, 0, _Offset, 0) * float4(1,1, -1,-1) * _MainTex_TexelSize.x;
			o.uv23 =  v.texcoord.xyxy + float4(_Offset, 0, _Offset, 0) * float4(1,1, -1,-1) * 2.0 * _MainTex_TexelSize.x;
			o.uv45 =  v.texcoord.xyxy + float4(_Offset, 0, _Offset, 0) * float4(1,1, -1,-1) * 3.0 * _MainTex_TexelSize.x;
		#else
			o.uv01 =  v.texcoord.xyxy + float4(0, _Offset, 0, _Offset) * float4(1,1, -1,-1) * _MainTex_TexelSize.y * _VRMult;
			o.uv23 =  v.texcoord.xyxy + float4(0, _Offset, 0, _Offset) * float4(1,1, -1,-1) * 2.0 * _MainTex_TexelSize.y * _VRMult;
			o.uv45 =  v.texcoord.xyxy + float4(0, _Offset, 0, _Offset) * float4(1,1, -1,-1) * 3.0 * _MainTex_TexelSize.y * _VRMult;
		#endif

		return o;
	}

	half4 fragBlur (v2fBlur o) : SV_Target 
	{
		half4 color = half4 (0,0,0,0);

		#if _MK_HQ_BLUR
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv, _MainTex_ST));
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv01.xy, _MainTex_ST));
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv01.zw, _MainTex_ST));
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv23.xy, _MainTex_ST));
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv23.zw, _MainTex_ST));
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv45.xy, _MainTex_ST));
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv45.zw, _MainTex_ST));
			color /= 14.0;
		#else
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv, _MainTex_ST));
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv01.xy, _MainTex_ST));
			color += tex2D (_MainTex, UnityStereoScreenSpaceUVAdjust(o.uv01.zw, _MainTex_ST));
			color /= 6.0;
		#endif

		return color;
	}
#endif