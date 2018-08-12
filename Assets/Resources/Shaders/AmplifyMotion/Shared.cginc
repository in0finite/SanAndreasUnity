// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#ifndef AMPLIFY_MOTION_SHARED_INCLUDED
#define AMPLIFY_MOTION_SHARED_INCLUDED

#include "UnityCG.cginc"

sampler2D_float _CameraDepthTexture;
float4 _CameraDepthTexture_TexelSize;

float4x4 _AM_MATRIX_PREV_M;
float4x4 _AM_MATRIX_CURR_M;
float4x4 _AM_MATRIX_PREV_VP;
float4 _AM_MOTION_PARAMS;	// camera motion scale, object motion scale, object id

float4 _AM_ZBUFFER_PARAMS;
float _AM_OBJECT_ID;
float _AM_MOTION_SCALE;
float _AM_MIN_VELOCITY;
float _AM_MAX_VELOCITY;
float _AM_RCP_TOTAL_VELOCITY;

sampler2D _AM_PREV_VERTEX_TEX;
sampler2D _AM_CURR_VERTEX_TEX;

float4 _AM_VERTEX_TEXEL_SIZE;
float4 _AM_VERTEX_TEXEL_HALFSIZE;

sampler2D _MotionTex;
sampler2D _MainTex;
float4 _MainTex_TexelSize;
float4 _MainTex_ST;
float _Cutoff;

struct VertexInput
{
	float4 vertex : POSITION;
#if defined( AM_CUTOUT )
	float2 texcoord : TEXCOORD0;
#endif
#if defined( AM_DEFORM ) || defined( AM_DEFORM_GPU )
	float3 prev_vertex : NORMAL;
	float2 indexCoords : TEXCOORD1;
#endif
};

struct MotionVertexToFragment
{
	float4 pos : SV_POSITION;
	float4 screen_pos : TEXCOORD0;

#if defined( AM_MOBILE )
	#if defined( AM_CUTOUT )
		float2 uv : TEXCOORD1;
		float4 motion : TEXCOORD2;
	#else
		float4 motion : TEXCOORD1;
	#endif
#else
	#if defined( AM_CUTOUT )
		float2 uv : TEXCOORD1;
		float4 cam_prev : TEXCOORD2;
		float4 obj_prev : TEXCOORD3;
		float4 pos_curr : TEXCOORD4;
	#else
		float4 cam_prev : TEXCOORD1;
		float4 obj_prev : TEXCOORD2;
		float4 pos_curr : TEXCOORD3;
	#endif
#endif
};

inline float4 CustomObjectToClipPos( in float3 pos )
{
#if UNITY_VERSION >= 540
	return UnityObjectToClipPos( pos );
#else
	return mul( UNITY_MATRIX_VP, mul( unity_ObjectToWorld, float4( pos, 1.0 ) ) );
#endif
}

inline bool DepthTest( float4 screen_pos )
{
	const float epsilon = 0.001f;
	float3 uv = screen_pos.xyz / screen_pos.w;
	float behind = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, uv.xy );

#if defined( SHADER_API_OPENGL ) || defined( SHADER_API_GLES ) || defined( SHADER_API_GLES3 ) || defined( SHADER_API_GLCORE )
	float front = uv.z * 0.5 + 0.5;
#else
	float front = uv.z;
#endif

#if defined( UNITY_REVERSED_Z )
	return ( behind <= front + epsilon );
#else
	return ( behind >= front - epsilon );
#endif
}

inline half2 ComputeMotionVector( half4 pos_prev, half4 pos_curr, half scale )
{
	pos_prev = pos_prev / pos_prev.w;
	pos_curr = pos_curr / pos_curr.w;
	return ( pos_curr - pos_prev ).xy * scale;
}

inline half4 PackMotionVector( half2 motion, half obj_id )
{
	half3 packed;
	packed.z = length( motion.xy );
	packed.xy = ( motion.xy / packed.z ) * 0.5f + 0.5f;
	packed.z = ( packed.z < _AM_MIN_VELOCITY ) ? 0 : packed.z;
	packed.z = max( min( packed.z, _AM_MAX_VELOCITY ) - _AM_MIN_VELOCITY, 0 ) * _AM_RCP_TOTAL_VELOCITY;
	return half4( packed, obj_id );
}

MotionVertexToFragment MotionVertex( VertexInput v )
{
	MotionVertexToFragment o;
	UNITY_INITIALIZE_OUTPUT( MotionVertexToFragment, o );

	float4 curr_vertex = float4( v.vertex.xyz, 1 );
	float4 prev_vertex = curr_vertex;

#if defined( AM_DEFORM ) || defined( AM_DEFORM_GPU )
	prev_vertex = float4( v.prev_vertex.xyz, 1 );
	#if defined( AM_DEFORM_GPU )
		prev_vertex = 0;
		curr_vertex *= 0.00000001; // trick compiler into behaving
		prev_vertex += tex2Dlod( _AM_PREV_VERTEX_TEX, float4( v.indexCoords, 0, 0 ) );
		curr_vertex += tex2Dlod( _AM_CURR_VERTEX_TEX, float4( v.indexCoords, 0, 0 ) );
	#endif
#endif

	float3 world_prev = mul( _AM_MATRIX_PREV_M, prev_vertex );
	float3 world_curr = mul( _AM_MATRIX_CURR_M, curr_vertex );

	float4 pos = o.pos = mul( UNITY_MATRIX_VP, float4( world_curr, 1 ) );
	o.screen_pos = ComputeScreenPos( pos );
#if defined( AM_CUTOUT )
	o.uv = TRANSFORM_TEX( v.texcoord, _MainTex );
#endif

	float4 cam_prev = mul( _AM_MATRIX_PREV_VP, float4( world_curr, 1 ) );
	float4 obj_prev = mul( UNITY_MATRIX_VP, float4( world_prev, 1 ) );
	float4 pos_curr = pos;

#if UNITY_UV_STARTS_AT_TOP
	cam_prev.y = -cam_prev.y;
	obj_prev.y = -obj_prev.y;
	pos_curr.y = -pos_curr.y;
	pos.y = ( _ProjectionParams.x > 0 ) ? -pos.y : pos.y;
#endif

#if defined( AM_MOBILE )
	half2 cameraMotion = ComputeMotionVector( cam_prev, pos_curr, _AM_MOTION_PARAMS.x );
	half2 objectMotion = ComputeMotionVector( obj_prev, pos_curr, _AM_MOTION_PARAMS.y );
	half2 motion = objectMotion + cameraMotion;
	#if defined( AM_PACKED )
		o.motion = PackMotionVector( motion, _AM_MOTION_PARAMS.z );
	#else
		o.motion = half4( motion.xy, _AM_MOTION_PARAMS.z, 0 );
	#endif
#else
	o.cam_prev = cam_prev;
	o.obj_prev = obj_prev;
	o.pos_curr = pos_curr;
#endif
	return o;
}

half4 MotionFragment( MotionVertexToFragment i ) : SV_Target
{
	if ( !DepthTest( i.screen_pos ) )
	{
		discard;
	}

#if defined( AM_CUTOUT )
	if ( tex2D( _MainTex, i.uv ).a < _Cutoff )
	{
		discard;
	}
#endif

	half4 res;
#if defined( AM_MOBILE )
	res = i.motion;
#else
	half2 cameraMotion = ComputeMotionVector( i.cam_prev, i.pos_curr, _AM_MOTION_PARAMS.x );
	half2 objectMotion = ComputeMotionVector( i.obj_prev, i.pos_curr, _AM_MOTION_PARAMS.y );
	half2 motion = objectMotion + cameraMotion;
	#if defined( AM_PACKED )
		res = PackMotionVector( motion, _AM_MOTION_PARAMS.z );
	#else
		res = half4( motion.xy, _AM_MOTION_PARAMS.z, 0 );
	#endif
#endif
	return res;
}

#endif
