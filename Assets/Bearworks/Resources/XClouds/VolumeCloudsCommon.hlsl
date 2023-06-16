#ifndef VOLUME_CLOUDS_COMMON
#define VOLUME_CLOUDS_COMMON

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"


#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/lighting.hlsl"

#define HALF_MAX    65504.0
#define UNITY_PI    3.14159

float4x4 _PreviousProjection;
float4x4 _PreviousRotation;
float4x4 _Projection;
float4x4 _InverseProjection;
float4x4 _InverseRotation;
float _SubFrameNumber;
float _SubPixelSize;
float2 _SubFrameSize;
float2 _FrameSize;

struct clouds_v2f {
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 cameraRay : TEXCOORD2;
};

struct appdata_img
{
	float4 vertex : POSITION;
	half2 texcoord : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

inline float3 UVToCameraRay(float2 uv)
{
	float4 cameraRay = float4(uv * 2.0 - 1.0, 1.0, 1.0);
	cameraRay = mul(_InverseProjection, cameraRay);
	cameraRay = cameraRay / cameraRay.w;

	return mul((float3x3)_InverseRotation, cameraRay.xyz);
}

inline float3 InternalRaySphereIntersect(float sphereRadius, float3 origin, float3 direction)
{
	float a0 = sphereRadius * sphereRadius - dot(origin, origin);
	float a1 = dot(origin, direction);
	float result = sqrt(a1 * a1 + a0) - a1;

	return origin + direction * result;
}

inline float InternalRaySphereIntersectLength(float sphereRadius, float3 origin, float3 direction)
{
	float a0 = sphereRadius * sphereRadius - dot(origin, origin);
	float a1 = dot(origin, direction);
	return sqrt(a1 * a1 + a0) - a1;
}

// Tranforms position from world to homogenous space
float4 UnityWorldToClipPos(in float3 pos)
{
	return mul(UNITY_MATRIX_VP, float4(pos, 1.0));
}

// Tranforms position from view to homogenous space
float4 UnityViewToClipPos(in float3 pos)
{
	return mul(UNITY_MATRIX_P, float4(pos, 1.0));
}

// Tranforms position from object to camera space
float3 UnityObjectToViewPos(in float3 pos)
{
	return mul(UNITY_MATRIX_V, mul(UNITY_MATRIX_M, float4(pos, 1.0))).xyz;
}

float3 UnityObjectToViewPos(float4 pos) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
	return UnityObjectToViewPos(pos.xyz);
}

// Tranforms position from world to camera space
float3 UnityWorldToViewPos(in float3 pos)
{
	return mul(UNITY_MATRIX_V, float4(pos, 1.0)).xyz;
}

// Transforms direction from object to world space
float3 UnityObjectToWorldDir(in float3 dir)
{
	return normalize(mul((float3x3)UNITY_MATRIX_M, dir));
}

// Transforms direction from world to object space
float3 UnityWorldToObjectDir(in float3 dir)
{
	return normalize(mul((float3x3)UNITY_MATRIX_I_M, dir));
}

// Transforms normal from object to world space
float3 UnityObjectToWorldNormal(in float3 norm)
{
#ifdef UNITY_ASSUME_UNIFORM_SCALING
	return UnityObjectToWorldDir(norm);
#else
	// mul(IT_M, norm) => mul(norm, I_M) => {dot(norm, I_M.col0), dot(norm, I_M.col1), dot(norm, I_M.col2)}
	return normalize(mul(norm, (float3x3)UNITY_MATRIX_I_M));
#endif
}

// Tranforms position from object to homogenous space
float4 UnityObjectToClipPos(in float3 pos)
{
	// More efficient than computing M*VP matrix product
	return mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(pos, 1.0)));
}

float4 UnityObjectToClipPos(float4 pos) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
	return UnityObjectToClipPos(pos.xyz);
}

float3 UnityWorldSpaceViewDir(float3 worldPos)
{
	return _WorldSpaceCameraPos.xyz - worldPos;
}

#endif