// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#ifndef AMPLIFY_MOTION_BLUR_SHARED_INCLUDED
#define AMPLIFY_MOTION_BLUR_SHARED_INCLUDED

#include "Shared.cginc"

sampler2D _DepthTex;
float4 _DepthTex_TexelSize;

half4 _AM_BLUR_STEP;
half2 _AM_DEPTH_THRESHOLD;

struct v2f
{
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD0;
};

v2f vert( appdata_img v )
{
	v2f o = ( v2f ) 0;
	o.pos = CustomObjectToClipPos( v.vertex );
	o.uv.xy = v.texcoord.xy;
	o.uv.zw = v.texcoord.xy;
#if UNITY_UV_STARTS_AT_TOP
	if ( _MainTex_TexelSize.y < 0 )
		o.uv.w = 1 - o.uv.w;
#endif
	return o;
}

inline half3 LinearEyeDepth( half3 z )
{
	return 1.0 / ( _ZBufferParams.zzz * z + _ZBufferParams.www );
}

inline half4 LinearEyeDepth( half4 z )
{
	return 1.0 / ( _ZBufferParams.zzzz * z + _ZBufferParams.wwww );
}

float ig_noise( float2 screenPos )
{
	const float3 magic = float3( 0.06711056, 0.00583715, 52.9829189 );
	return frac( magic.z * frac( dot( screenPos, magic.xy ) ) );
}

half4 motionblur_mobile( v2f i, bool noise )
{
	// 3-TAP
	half3 motion = tex2D( _MotionTex, i.uv.zw ).xyz;
	half4 color = tex2D( _MainTex, i.uv.xy );
	half4 accum = half4( color.xyz, 1 );

	half ref_depth = DecodeFloatRGBA( tex2D( _DepthTex, i.uv.xy ) );
	half ref_id = color.a;

	half id = floor( color.a * 255 + 0.5 );
	half ref_isobj = ( id > 1 ) * ( id < 254 );

	half2 dir_step0 = _AM_BLUR_STEP.xy * ( motion.xy * 2.0 - 1.0 ) * motion.z;

	if ( noise )
		dir_step0 *= ig_noise( ( i.uv.xy - dir_step0 ) * _MainTex_TexelSize.zw ) + 0.5;

	half sample_depth0 = DecodeFloatRGBA( tex2D( _DepthTex, i.uv.xy - dir_step0 ) );
	half sample_depth1 = DecodeFloatRGBA( tex2D( _DepthTex, i.uv.xy + dir_step0 ) );

	half4 sample_color0 = tex2D( _MainTex, i.uv.xy - dir_step0 );
	half4 sample_color1 = tex2D( _MainTex, i.uv.xy + dir_step0 );

	half3 depth = LinearEyeDepth( half3( sample_depth0, sample_depth1, ref_depth ) );
	half2 sample_id = half2( sample_color0.a, sample_color1.a );

	half2 depth_test = depth.xy > ( depth.zz - _AM_DEPTH_THRESHOLD.xx );
	half2 obj_test = ref_isobj.xx * ( sample_id == ref_id.xx );

	half2 sample_test = saturate( depth_test + obj_test );

	accum += sample_test.x * half4( sample_color0.xyz, 1 );
	accum += sample_test.y * half4( sample_color1.xyz, 1 );

	return half4( accum.xyz / accum.w, ref_id );
}

half4 motionblur_sm2( v2f i, bool noise )
{
	// 5-TAP
	half3 motion = tex2D( _MotionTex, i.uv.zw ).xyz;
	half4 color = tex2D( _MainTex, i.uv.xy );
	half4 accum = half4( color.xyz, 1 );

	half ref_depth = LinearEyeDepth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy ) );
	half ref_id = color.a;

	half id = floor( color.a * 255 + 0.5 );
	half ref_isobj = ( id > 1 ) * ( id < 254 );

	half2 dir_step0  = _AM_BLUR_STEP.xy * ( motion.xy * 2.0 - 1.0 ) * motion.z;
	half2 dir_step1 = dir_step0 * 0.5;

	if ( noise )
	{
		dir_step0 *= ig_noise( ( i.uv.xy - dir_step0 ) * _MainTex_TexelSize.zw + 0 ) + 0.5;
		dir_step1 *= ig_noise( ( i.uv.xy - dir_step1 ) * _MainTex_TexelSize.zw + 1 ) + 0.5;
	}

	half sample_depth0 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy - dir_step0 );
	half sample_depth1 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy - dir_step1 );
	half sample_depth2 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy + dir_step1 );
	half sample_depth3 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy + dir_step0 );

	half4 sample_color0 = tex2D( _MainTex, i.uv.xy - dir_step0 );
	half4 sample_color1 = tex2D( _MainTex, i.uv.xy - dir_step1 );
	half4 sample_color2 = tex2D( _MainTex, i.uv.xy + dir_step1 );
	half4 sample_color3 = tex2D( _MainTex, i.uv.xy + dir_step0 );

	half4 sample_depth = LinearEyeDepth( half4( sample_depth0, sample_depth1, sample_depth2, sample_depth3 ) );
	half4 sample_id = half4( sample_color0.a, sample_color1.a, sample_color2.a, sample_color3.a );

	half4 depth_test = sample_depth > ( ref_depth.xxxx - _AM_DEPTH_THRESHOLD.xxxx );
	half4 obj_test = ref_isobj.xxxx * ( sample_id == ref_id.xxxx );

	half4 sample_test = saturate( depth_test + obj_test );

	accum += sample_test.x * half4( sample_color0.xyz, 1 );
	accum += sample_test.y * half4( sample_color1.xyz, 1 );
	accum += sample_test.z * half4( sample_color2.xyz, 1 );
	accum += sample_test.w * half4( sample_color3.xyz, 1 );

	return half4( accum.xyz / accum.w, ref_id );
}

half4 motionblur_sm3( v2f i, bool noise )
{
	// 9-TAP
	half3 motion = tex2D( _MotionTex, i.uv.zw ).xyz;
	half4 color = tex2D( _MainTex, i.uv.xy );
	half4 accum = half4( color.xyz, 1 );

	half ref_depth = LinearEyeDepth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy ) );
	half ref_id = color.a;

	half id = floor( color.a * 255 + 0.5 );
	half ref_isobj = ( id > 1 ) * ( id < 254 );

	half2 dir_step0 = _AM_BLUR_STEP.xy * ( motion.xy * 2.0 - 1.0 ) * motion.z;
	half2 dir_step1 = dir_step0 * 0.75;
	half2 dir_step2 = dir_step0 * 0.50;
	half2 dir_step3 = dir_step0 * 0.25;

	if ( noise )
	{
		dir_step0 *= ig_noise( ( i.uv.xy - dir_step0 ) * _MainTex_TexelSize.zw + 0 ) + 0.5;
		dir_step1 *= ig_noise( ( i.uv.xy - dir_step1 ) * _MainTex_TexelSize.zw + 1 ) + 0.5;
		dir_step2 *= ig_noise( ( i.uv.xy - dir_step2 ) * _MainTex_TexelSize.zw + 2 ) + 0.5;
		dir_step3 *= ig_noise( ( i.uv.xy - dir_step3 ) * _MainTex_TexelSize.zw + 3 ) + 0.5;
	}

	half sample_depth0 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy - dir_step0 );
	half sample_depth1 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy - dir_step1 );
	half sample_depth2 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy - dir_step2 );
	half sample_depth3 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy - dir_step3 );
	half sample_depth4 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy + dir_step3 );
	half sample_depth5 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy + dir_step2 );
	half sample_depth6 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy + dir_step1 );
	half sample_depth7 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy + dir_step0 );

	half4 sample_color0 = tex2D( _MainTex, i.uv.xy - dir_step0 );
	half4 sample_color1 = tex2D( _MainTex, i.uv.xy - dir_step1 );
	half4 sample_color2 = tex2D( _MainTex, i.uv.xy - dir_step2 );
	half4 sample_color3 = tex2D( _MainTex, i.uv.xy - dir_step3 );
	half4 sample_color4 = tex2D( _MainTex, i.uv.xy + dir_step3 );
	half4 sample_color5 = tex2D( _MainTex, i.uv.xy + dir_step2 );
	half4 sample_color6 = tex2D( _MainTex, i.uv.xy + dir_step1 );
	half4 sample_color7 = tex2D( _MainTex, i.uv.xy + dir_step0 );

	half4 diffA = ref_depth.xxxx - LinearEyeDepth( half4( sample_depth0, sample_depth1, sample_depth2, sample_depth3 ) );
	half4 diffB = ref_depth.xxxx - LinearEyeDepth( half4( sample_depth4, sample_depth5, sample_depth6, sample_depth7 ) );
	half4 diff_testA = diffA < _AM_DEPTH_THRESHOLD.xxxx;
	half4 diff_testB = diffB < _AM_DEPTH_THRESHOLD.xxxx;
	half4 sample_testA = diff_testA - diff_testA * saturate( diffA * _AM_DEPTH_THRESHOLD.yyyy );
	half4 sample_testB = diff_testB - diff_testB * saturate( diffB * _AM_DEPTH_THRESHOLD.yyyy );

	half4 sample_idA = half4( sample_color0.a, sample_color1.a, sample_color2.a, sample_color3.a );
	half4 sample_idB = half4( sample_color4.a, sample_color5.a, sample_color6.a, sample_color7.a );
	half4 obj_testA = ref_isobj.xxxx * ( sample_idA == ref_id.xxxx );
	half4 obj_testB = ref_isobj.xxxx * ( sample_idB == ref_id.xxxx );

	sample_testA = saturate( sample_testA + obj_testA );
	sample_testB = saturate( sample_testB + obj_testB );

	accum += sample_testA.x * half4( sample_color0.xyz, 1 );
	accum += sample_testA.y * half4( sample_color1.xyz, 1 );
	accum += sample_testA.z * half4( sample_color2.xyz, 1 );
	accum += sample_testA.w * half4( sample_color3.xyz, 1 );
	accum += sample_testB.x * half4( sample_color4.xyz, 1 );
	accum += sample_testB.y * half4( sample_color5.xyz, 1 );
	accum += sample_testB.z * half4( sample_color6.xyz, 1 );
	accum += sample_testB.w * half4( sample_color7.xyz, 1 );

	return half4( accum.xyz / accum.w, ref_id );
}

half4 motionblur_soft_sm3( v2f i, bool noise )
{
	// 5-TAP
	half3 motion = tex2D( _MotionTex, i.uv.zw ).xyz;
	half4 color = tex2D( _MainTex, i.uv.xy );
	half4 accum = half4( color.xyz, 1 );

	half ref_depth = LinearEyeDepth( SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy ) );
	half ref_id = color.a;

	half id = floor( color.a * 255 + 0.5 );
	half ref_isobj = ( id > 1 ) * ( id < 254 );

	half2 dir_step0 = _AM_BLUR_STEP.xy * ( motion.xy * 2.0 - 1.0 ) * motion.z;
	half2 dir_step1 = dir_step0 * 0.5;

	if ( noise )
	{
		dir_step0 *= ig_noise( ( i.uv.xy - dir_step0 ) * _MainTex_TexelSize.zw + 0 ) + 0.5;
		dir_step1 *= ig_noise( ( i.uv.xy - dir_step1 ) * _MainTex_TexelSize.zw + 1 ) + 0.5;
	}

	half sample_depth0 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy - dir_step0 );
	half sample_depth1 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy - dir_step1 );
	half sample_depth2 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy + dir_step1 );
	half sample_depth3 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, i.uv.xy + dir_step0 );

	half4 sample_color0 = tex2D( _MainTex, i.uv.xy - dir_step0 );
	half4 sample_color1 = tex2D( _MainTex, i.uv.xy - dir_step1 );
	half4 sample_color2 = tex2D( _MainTex, i.uv.xy + dir_step1 );
	half4 sample_color3 = tex2D( _MainTex, i.uv.xy + dir_step0 );

	half sample_mag0 = tex2D( _MotionTex, i.uv.xy - dir_step0 ).z;
	half sample_mag1 = tex2D( _MotionTex, i.uv.xy - dir_step1 ).z;
	half sample_mag2 = tex2D( _MotionTex, i.uv.xy + dir_step1 ).z;
	half sample_mag3 = tex2D( _MotionTex, i.uv.xy + dir_step0 ).z;

	half4 sample_depth = LinearEyeDepth( half4( sample_depth0, sample_depth1, sample_depth2, sample_depth3 ) );
	half4 sample_id = half4( sample_color0.a, sample_color1.a, sample_color2.a, sample_color3.a );
	half4 sample_mag = half4( sample_mag0, sample_mag1, sample_mag2, sample_mag3 );

	half4 thres_mag = ( 0.5 ).xxxx;

	half4 depth_test = sample_depth > ( ref_depth.xxxx - _AM_DEPTH_THRESHOLD.xxxx );
	half4 obj_test = ref_isobj.xxxx * ( sample_id == ref_id.xxxx );
	half4 mag_test = sample_mag > thres_mag;

	half4 sample_test = saturate( depth_test + obj_test + mag_test );

	accum += sample_test.x * half4( sample_color0.xyz, 1 );
	accum += sample_test.y * half4( sample_color1.xyz, 1 );
	accum += sample_test.z * half4( sample_color2.xyz, 1 );
	accum += sample_test.w * half4( sample_color3.xyz, 1 );

	return half4( accum.xyz / accum.w, ref_id );
}

half4 frag_mobile( v2f i ) : SV_Target
{
	return motionblur_mobile( i, false );
}

half4 frag_sm2( v2f i ) : SV_Target
{
	return motionblur_sm2( i, false );
}

half4 frag_sm3( v2f i ) : SV_Target
{
	return motionblur_sm3( i, false );
}

half4 frag_soft_sm3( v2f i ) : SV_Target
{
	return motionblur_soft_sm3( i, false );
}

half4 frag_mobile_noise( v2f i ) : SV_Target
{
	return motionblur_mobile( i, true );
}

half4 frag_sm2_noise( v2f i ) : SV_Target
{
	return motionblur_sm2( i, true );
}

half4 frag_sm3_noise( v2f i ) : SV_Target
{
	return motionblur_sm3( i, true );
}

half4 frag_soft_sm3_noise( v2f i ) : SV_Target
{
	return motionblur_soft_sm3( i, true );
}

#endif
