// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
#define UNITY_5
#endif

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
namespace AmplifyMotion
{
internal class SkinnedState : AmplifyMotion.MotionState
{
	private SkinnedMeshRenderer m_renderer;

	private int m_boneCount;
	private Transform[] m_boneTransforms;
	private Matrix3x4[] m_bones;

	private int m_weightCount;
	private int[] m_boneIndices;
	private float[] m_boneWeights;

	private int m_vertexCount;
	private Vector4[] m_baseVertices;
	private Vector3[] m_prevVertices;
	private Vector3[] m_currVertices;

	private int m_gpuBoneTexWidth;
	private int m_gpuBoneTexHeight;
	private int m_gpuVertexTexWidth;
	private int m_gpuVertexTexHeight;
	private Material m_gpuSkinDeformMat;
	private Color[] m_gpuBoneData;
	private Texture2D m_gpuBones;
	private Texture2D m_gpuBoneIndices;
	private Texture2D[] m_gpuBaseVertices;
	private RenderTexture m_gpuPrevVertices;
	private RenderTexture m_gpuCurrVertices;

	private Mesh m_clonedMesh;
	private Matrix3x4 m_worldToLocalMatrix;
	private Matrix3x4 m_prevLocalToWorld;
	private Matrix3x4 m_currLocalToWorld;

	private MaterialDesc[] m_sharedMaterials;

	private ManualResetEvent m_asyncUpdateSignal = null;
	private bool m_asyncUpdateTriggered = false;

	private bool m_starting;
	private bool m_wasVisible;
	private bool m_useFallback;
	private bool m_useGPU = false;

	private static HashSet<AmplifyMotionObjectBase> m_uniqueWarnings = new HashSet<AmplifyMotionObjectBase>();

	public SkinnedState( AmplifyMotionCamera owner, AmplifyMotionObjectBase obj )
		: base( owner, obj )
	{
		m_renderer = m_obj.GetComponent<SkinnedMeshRenderer>();
	}

	void IssueWarning( string message )
	{
		if ( !m_uniqueWarnings.Contains( m_obj ) )
		{
			Debug.LogWarning( message );
			m_uniqueWarnings.Add( m_obj );
		}
	}

	void IssueError( string message )
	{
		IssueWarning( message );
		m_error = true;
	}

	internal override void Initialize()
	{
		if ( !m_renderer.sharedMesh.isReadable )
		{
			IssueError( "[AmplifyMotion] Read/Write Import Setting disabled in object " + m_obj.name + ". Skipping." );
			return;
		}

		// find out if we're forced to use the fallback path
		Transform[] bones = m_renderer.bones;
		m_useFallback = ( bones == null || bones.Length == 0 );

		if ( !m_useFallback )
			m_useGPU = m_owner.Instance.CanUseGPU; // use GPU, if allowed

		base.Initialize();

		m_vertexCount = m_renderer.sharedMesh.vertexCount;
		m_prevVertices = new Vector3[ m_vertexCount ];
		m_currVertices = new Vector3[ m_vertexCount ];
		m_clonedMesh = new Mesh();

		if ( !m_useFallback )
		{
			if ( m_renderer.quality == SkinQuality.Auto )
				m_weightCount = ( int ) QualitySettings.blendWeights;
			else
				m_weightCount = ( int ) m_renderer.quality;

			m_boneTransforms = m_renderer.bones;
			m_boneCount = m_renderer.bones.Length;
			m_bones = new Matrix3x4[ m_boneCount ];

			Vector4[] baseVertices = new Vector4[ m_vertexCount * m_weightCount ];
			int[] boneIndices = new int[ m_vertexCount * m_weightCount ];
			float[] boneWeights = ( m_weightCount > 1 ) ? new float[ m_vertexCount * m_weightCount ] : null;

			if ( m_weightCount == 1 )
				InitializeBone1( baseVertices, boneIndices );
			else if ( m_weightCount == 2 )
				InitializeBone2( baseVertices, boneIndices, boneWeights );
			else
				InitializeBone4( baseVertices, boneIndices, boneWeights );

			m_baseVertices = baseVertices;
			m_boneIndices = boneIndices;
			m_boneWeights = boneWeights;

			Mesh skinnedMesh = m_renderer.sharedMesh;

			m_clonedMesh.vertices = skinnedMesh.vertices;
			m_clonedMesh.normals = skinnedMesh.vertices;
			m_clonedMesh.uv = skinnedMesh.uv;
			m_clonedMesh.subMeshCount = skinnedMesh.subMeshCount;
			for ( int i = 0; i < skinnedMesh.subMeshCount; i++ )
				m_clonedMesh.SetTriangles( skinnedMesh.GetTriangles( i ), i );

			if ( m_useGPU )
			{
				if ( !InitializeGPUSkinDeform() )
				{
					// fallback
					Debug.LogWarning( "[AmplifyMotion] Failed initializing GPU skin deform for object " + m_obj.name + ". Falling back to CPU path." );
					m_useGPU = false;
				}
				else
				{
					// release unnecessary data
					m_boneIndices = null;
					m_boneWeights = null;

					m_baseVertices = null;
					m_prevVertices = null;
					m_currVertices = null;
				}
			}

			if ( !m_useGPU )
			{
				m_asyncUpdateSignal = new ManualResetEvent( false );
				m_asyncUpdateTriggered = false;
			}
		}

		m_sharedMaterials = ProcessSharedMaterials( m_renderer.sharedMaterials );

		m_wasVisible = false;
	}

	internal override void Shutdown()
	{
		if ( !m_useFallback && !m_useGPU )
			WaitForAsyncUpdate();

		if ( m_useGPU )
			ShutdownGPUSkinDeform();

		if ( m_clonedMesh != null )
		{
			Mesh.Destroy( m_clonedMesh );
			m_clonedMesh = null;
		}

		m_boneTransforms = null;
		m_bones = null;
		m_boneIndices = null;
		m_boneWeights = null;
		m_baseVertices = null;
		m_prevVertices = null;
		m_currVertices = null;
		m_sharedMaterials = null;
	}

	private bool InitializeGPUSkinDeform()
	{
		bool succeeded = true;
		try
		{
			m_gpuBoneTexWidth = m_boneCount;
			m_gpuBoneTexHeight = 3;
			m_gpuVertexTexWidth = Mathf.CeilToInt( Mathf.Sqrt( m_vertexCount ) );
			m_gpuVertexTexHeight = Mathf.CeilToInt( m_vertexCount / ( float ) m_gpuVertexTexWidth );

			// gpu skin deform material
			m_gpuSkinDeformMat = new Material( Shader.Find( "Hidden/Amplify Motion/GPUSkinDeform" ) ) { hideFlags = HideFlags.DontSave };

			// bone matrix texture
			m_gpuBones = new Texture2D( m_gpuBoneTexWidth, m_gpuBoneTexHeight, TextureFormat.RGBAFloat, false, true );
			m_gpuBones.hideFlags = HideFlags.DontSave;
			m_gpuBones.name = "AM-" + m_obj.name + "-Bones";
			m_gpuBones.filterMode = FilterMode.Point;

			m_gpuBoneData = new Color[ m_gpuBoneTexWidth * m_gpuBoneTexHeight ];

			UpdateBonesGPU();

			// vertex bone index/weight textures
			TextureFormat boneIDWFormat = TextureFormat.RHalf;
			boneIDWFormat = ( m_weightCount == 2 ) ? TextureFormat.RGHalf : boneIDWFormat;
			boneIDWFormat = ( m_weightCount == 4 ) ? TextureFormat.RGBAHalf : boneIDWFormat;

			m_gpuBoneIndices = new Texture2D( m_gpuVertexTexWidth, m_gpuVertexTexHeight, boneIDWFormat, false, true );
			m_gpuBoneIndices.hideFlags = HideFlags.DontSave;
			m_gpuBoneIndices.name = "AM-" + m_obj.name + "-Bones";
			m_gpuBoneIndices.filterMode = FilterMode.Point;
			m_gpuBoneIndices.wrapMode = TextureWrapMode.Clamp;

			BoneWeight[] meshBoneWeights = m_renderer.sharedMesh.boneWeights;
			Color[] boneIndices = new Color[ m_gpuVertexTexWidth * m_gpuVertexTexHeight ];

			for ( int v = 0; v < m_vertexCount; v++ )
			{
				int x = v % m_gpuVertexTexWidth;
				int y = v / m_gpuVertexTexWidth;
				int offset = y * m_gpuVertexTexWidth + x;

				BoneWeight boneWeight = meshBoneWeights[ v ];
				boneIndices[ offset ] = new Vector4( boneWeight.boneIndex0, boneWeight.boneIndex1, boneWeight.boneIndex2, boneWeight.boneIndex3 );
			}
			m_gpuBoneIndices.SetPixels( boneIndices );
			m_gpuBoneIndices.Apply();

			// base vertex textures
			m_gpuBaseVertices = new Texture2D[ m_weightCount ];
			for ( int w = 0; w < m_weightCount; w++ )
			{
				m_gpuBaseVertices[ w ] = new Texture2D( m_gpuVertexTexWidth, m_gpuVertexTexHeight, TextureFormat.RGBAFloat, false, true );
				m_gpuBaseVertices[ w ].hideFlags = HideFlags.DontSave;
				m_gpuBaseVertices[ w ].name = "AM-" + m_obj.name + "-BaseVerts";
				m_gpuBaseVertices[ w ].filterMode = FilterMode.Point;
			}

			List<Color[]> baseVertices = new List<Color[]>( m_weightCount );
			for ( int w = 0; w < m_weightCount; w++ )
				baseVertices.Add( new Color[ m_gpuVertexTexWidth * m_gpuVertexTexHeight ] );

			for ( int v = 0; v < m_vertexCount; v++ )
			{
				int x = v % m_gpuVertexTexWidth;
				int y = v / m_gpuVertexTexWidth;
				int offset = y * m_gpuVertexTexWidth + x;

				for ( int w = 0; w < m_weightCount; w++ )
					baseVertices[ w ][ offset ] = m_baseVertices[ v * m_weightCount + w ];
			}
			for ( int w = 0; w < m_weightCount; w++ )
			{
				m_gpuBaseVertices[ w ].SetPixels( baseVertices[ w ] );
				m_gpuBaseVertices[ w ].Apply();
			}

			// create output/target vertex render textures
			m_gpuPrevVertices = new RenderTexture( m_gpuVertexTexWidth, m_gpuVertexTexHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear );
			m_gpuPrevVertices.hideFlags = HideFlags.DontSave;
			m_gpuPrevVertices.name = "AM-" + m_obj.name + "-PrevVerts";
			m_gpuPrevVertices.filterMode = FilterMode.Point;
			m_gpuPrevVertices.wrapMode = TextureWrapMode.Clamp;
			m_gpuPrevVertices.Create();

			m_gpuCurrVertices = new RenderTexture( m_gpuVertexTexWidth, m_gpuVertexTexHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear );
			m_gpuCurrVertices.hideFlags = HideFlags.DontSave;
			m_gpuCurrVertices.name = "AM-" + m_obj.name + "-CurrVerts";
			m_gpuCurrVertices.filterMode = FilterMode.Point;
			m_gpuCurrVertices.wrapMode = TextureWrapMode.Clamp;
			m_gpuCurrVertices.Create();

			// assign local material constants
			m_gpuSkinDeformMat.SetTexture( "_AM_BONE_TEX", m_gpuBones );
			m_gpuSkinDeformMat.SetTexture( "_AM_BONE_INDEX_TEX", m_gpuBoneIndices );
			for ( int w = 0; w < m_weightCount; w++ )
				m_gpuSkinDeformMat.SetTexture( "_AM_BASE_VERTEX" + w + "_TEX", m_gpuBaseVertices[ w ] );

			// assign global shader constants
			Vector4 boneTexelSize = new Vector4( 1.0f / m_gpuBoneTexWidth, 1.0f / m_gpuBoneTexHeight, m_gpuBoneTexWidth, m_gpuBoneTexHeight );
			Vector4 vertexTexelSize = new Vector4( 1.0f / m_gpuVertexTexWidth, 1.0f / m_gpuVertexTexHeight, m_gpuVertexTexWidth, m_gpuVertexTexHeight );

			m_gpuSkinDeformMat.SetVector( "_AM_BONE_TEXEL_SIZE", boneTexelSize );
			m_gpuSkinDeformMat.SetVector( "_AM_BONE_TEXEL_HALFSIZE", boneTexelSize * 0.5f );
			m_gpuSkinDeformMat.SetVector( "_AM_VERTEX_TEXEL_SIZE", vertexTexelSize );
			m_gpuSkinDeformMat.SetVector( "_AM_VERTEX_TEXEL_HALFSIZE", vertexTexelSize * 0.5f );

			// assign vertex x/y offsets packed into second uv channel
			Vector2[] indexCoords = new Vector2[ m_vertexCount ];
			for ( int v = 0; v < m_vertexCount; v++ )
			{
				int x = v % m_gpuVertexTexWidth;
				int y = v / m_gpuVertexTexWidth;
				float x_norm = ( x / ( float ) m_gpuVertexTexWidth ) + vertexTexelSize.x * 0.5f;
				float y_norm = ( y / ( float ) m_gpuVertexTexHeight ) + vertexTexelSize.y * 0.5f;
				indexCoords[ v ] = new Vector2( x_norm, y_norm );
			}
			m_clonedMesh.uv2 = indexCoords;
		}
		catch ( Exception )
		{
			succeeded = false;
		}
		return succeeded;
	}

	private void ShutdownGPUSkinDeform()
	{
		if ( m_gpuSkinDeformMat != null )
		{
			Material.DestroyImmediate( m_gpuSkinDeformMat );
			m_gpuSkinDeformMat = null;
		}

		m_gpuBoneData = null;

		if ( m_gpuBones != null )
		{
			Texture2D.DestroyImmediate( m_gpuBones );
			m_gpuBones = null;
		}

		if ( m_gpuBoneIndices != null )
		{
			Texture2D.DestroyImmediate( m_gpuBoneIndices );
			m_gpuBoneIndices = null;
		}

		if ( m_gpuBaseVertices != null )
		{
			for ( int i = 0; i < m_gpuBaseVertices.Length; i++ )
				Texture2D.DestroyImmediate( m_gpuBaseVertices[ i ] );
			m_gpuBaseVertices = null;
		}

		if ( m_gpuPrevVertices != null )
		{
			RenderTexture.active = null;
			m_gpuPrevVertices.Release();
			RenderTexture.DestroyImmediate( m_gpuPrevVertices );
			m_gpuPrevVertices = null;
		}

		if ( m_gpuCurrVertices != null )
		{
			RenderTexture.active = null;
			m_gpuCurrVertices.Release();
			RenderTexture.DestroyImmediate( m_gpuCurrVertices );
			m_gpuCurrVertices = null;
		}
	}

	private void UpdateBonesGPU()
	{
		for ( int b = 0; b < m_boneCount; b++ )
		{
			for ( int r = 0; r < m_gpuBoneTexHeight; r++ )
				m_gpuBoneData[ r * m_gpuBoneTexWidth + b ] = m_bones[ b ].GetRow( r );
		}
		m_gpuBones.SetPixels( m_gpuBoneData );
		m_gpuBones.Apply();
	}

	private void UpdateVerticesGPU( CommandBuffer updateCB, bool starting )
	{
		if ( !starting && m_wasVisible )
		{
			m_gpuPrevVertices.DiscardContents();
			updateCB.Blit( new RenderTargetIdentifier( m_gpuCurrVertices ), m_gpuPrevVertices );
		}

		updateCB.SetGlobalMatrix( "_AM_WORLD_TO_LOCAL_MATRIX", m_worldToLocalMatrix );

		m_gpuCurrVertices.DiscardContents();
		RenderTexture dummy = null;
		updateCB.Blit( new RenderTargetIdentifier( dummy ), m_gpuCurrVertices, m_gpuSkinDeformMat, Mathf.Min( m_weightCount - 1, 2 ) );

		if ( starting || !m_wasVisible )
		{
			m_gpuPrevVertices.DiscardContents();
			updateCB.Blit( new RenderTargetIdentifier( m_gpuCurrVertices ), m_gpuPrevVertices );
		}
	}

	private void UpdateBones()
	{
		for ( int b = 0; b < m_boneCount; b++ )
			m_bones[ b ] = ( m_boneTransforms[ b ] != null ) ? m_boneTransforms[ b ].localToWorldMatrix : Matrix4x4.identity;

		m_worldToLocalMatrix = m_transform.worldToLocalMatrix;

		if ( m_useGPU )
		{
			Profiler.BeginSample( "UpdateBonesGPU" );
			UpdateBonesGPU();
			Profiler.EndSample();
		}
	}

	private void UpdateVerticesFallback( bool starting )
	{
		if ( !starting && m_wasVisible )
			Array.Copy( m_currVertices, m_prevVertices, m_vertexCount );

		m_renderer.BakeMesh( m_clonedMesh );

		if ( m_clonedMesh.vertexCount == 0 || m_clonedMesh.vertexCount != m_prevVertices.Length )
		{
			IssueError( "[AmplifyMotion] Invalid mesh obtained from SkinnedMeshRenderer.BakeMesh in object " + m_obj.name + ". Skipping." );
			return;
		}

		Array.Copy( m_clonedMesh.vertices, m_currVertices, m_vertexCount );

		if ( starting || !m_wasVisible )
			Array.Copy( m_currVertices, m_prevVertices, m_vertexCount );
	}

	private void AsyncUpdateVertices( bool starting )
	{
		if ( !starting && m_wasVisible )
			Array.Copy( m_currVertices, m_prevVertices, m_vertexCount );

		for ( int i = 0; i < m_boneCount; i++ )
			m_bones[ i ] = m_worldToLocalMatrix * m_bones[ i ];

		if ( m_weightCount == 1 )
			UpdateVerticesBone1();
		else if ( m_weightCount == 2 )
			UpdateVerticesBone2();
		else
			UpdateVerticesBone4();

		if ( starting || !m_wasVisible )
			Array.Copy( m_currVertices, m_prevVertices, m_vertexCount );
	}

	private void InitializeBone1( Vector4[] baseVertices, int[] boneIndices )
	{
		Vector3[] meshVertices = m_renderer.sharedMesh.vertices;
		Matrix4x4[] meshBindPoses = m_renderer.sharedMesh.bindposes;
		BoneWeight[] meshBoneWeights = m_renderer.sharedMesh.boneWeights;

		for ( int i = 0; i < m_vertexCount; i++ )
		{
			int offset0 = i * m_weightCount;

			int bone0 = boneIndices[ offset0 ] = meshBoneWeights[ i ].boneIndex0;
			Vector3 baseVertex0 = meshBindPoses[ bone0 ].MultiplyPoint3x4( meshVertices[ i ] );

			baseVertices[ offset0 ] = new Vector4( baseVertex0.x, baseVertex0.y, baseVertex0.z, 1.0f );
		}
	}

	private void InitializeBone2( Vector4[] baseVertices, int[] boneIndices, float[] boneWeights )
	{
		Vector3[] meshVertices = m_renderer.sharedMesh.vertices;
		Matrix4x4[] meshBindPoses = m_renderer.sharedMesh.bindposes;
		BoneWeight[] meshBoneWeights = m_renderer.sharedMesh.boneWeights;

		for ( int i = 0; i < m_vertexCount; i++ )
		{
			int offset0 = i * m_weightCount;
			int offset1 = offset0 + 1;

			BoneWeight boneWeight = meshBoneWeights[ i ];
			int bone0 = boneIndices[ offset0 ] = boneWeight.boneIndex0;
			int bone1 = boneIndices[ offset1 ] = boneWeight.boneIndex1;

			float weight0 = boneWeight.weight0;
			float weight1 = boneWeight.weight1;

			float rcpSum = 1.0f / ( weight0 + weight1 );
			boneWeights[ offset0 ] = weight0 = weight0 * rcpSum;
			boneWeights[ offset1 ] = weight1 = weight1 * rcpSum;

			Vector3 baseVertex0 = weight0 * meshBindPoses[ bone0 ].MultiplyPoint3x4( meshVertices[ i ] );
			Vector3 baseVertex1 = weight1 * meshBindPoses[ bone1 ].MultiplyPoint3x4( meshVertices[ i ] );

			baseVertices[ offset0 ] = new Vector4( baseVertex0.x, baseVertex0.y, baseVertex0.z, weight0 );
			baseVertices[ offset1 ] = new Vector4( baseVertex1.x, baseVertex1.y, baseVertex1.z, weight1 );
		}
	}

	private void InitializeBone4( Vector4[] baseVertices, int[] boneIndices, float[] boneWeights )
	{
		Vector3[] meshVertices = m_renderer.sharedMesh.vertices;
		Matrix4x4[] meshBindPoses = m_renderer.sharedMesh.bindposes;
		BoneWeight[] meshBoneWeights = m_renderer.sharedMesh.boneWeights;

		for ( int i = 0; i < m_vertexCount; i++ )
		{
			int offset0 = i * m_weightCount;
			int offset1 = offset0 + 1;
			int offset2 = offset0 + 2;
			int offset3 = offset0 + 3;

			BoneWeight boneWeight = meshBoneWeights[ i ];
			int bone0 = boneIndices[ offset0 ] = boneWeight.boneIndex0;
			int bone1 = boneIndices[ offset1 ] = boneWeight.boneIndex1;
			int bone2 = boneIndices[ offset2 ] = boneWeight.boneIndex2;
			int bone3 = boneIndices[ offset3 ] = boneWeight.boneIndex3;

			float weight0 = boneWeights[ offset0 ] = boneWeight.weight0;
			float weight1 = boneWeights[ offset1 ] = boneWeight.weight1;
			float weight2 = boneWeights[ offset2 ] = boneWeight.weight2;
			float weight3 = boneWeights[ offset3 ] = boneWeight.weight3;

			Vector3 baseVertex0 = weight0 * meshBindPoses[ bone0 ].MultiplyPoint3x4( meshVertices[ i ] );
			Vector3 baseVertex1 = weight1 * meshBindPoses[ bone1 ].MultiplyPoint3x4( meshVertices[ i ] );
			Vector3 baseVertex2 = weight2 * meshBindPoses[ bone2 ].MultiplyPoint3x4( meshVertices[ i ] );
			Vector3 baseVertex3 = weight3 * meshBindPoses[ bone3 ].MultiplyPoint3x4( meshVertices[ i ] );

			baseVertices[ offset0 ] = new Vector4( baseVertex0.x, baseVertex0.y, baseVertex0.z, weight0 );
			baseVertices[ offset1 ] = new Vector4( baseVertex1.x, baseVertex1.y, baseVertex1.z, weight1 );
			baseVertices[ offset2 ] = new Vector4( baseVertex2.x, baseVertex2.y, baseVertex2.z, weight2 );
			baseVertices[ offset3 ] = new Vector4( baseVertex3.x, baseVertex3.y, baseVertex3.z, weight3 );
		}
	}

	private void UpdateVerticesBone1()
	{
		for ( int i = 0; i < m_vertexCount; i++ )
			MulPoint3x4_XYZ( ref m_currVertices[ i ], ref m_bones[ m_boneIndices[ i ] ], m_baseVertices[ i ] );
	}

	private void UpdateVerticesBone2()
	{
		Vector3 deformedVertex = Vector3.zero;
		for ( int i = 0; i < m_vertexCount; i++ )
		{
			int offset0 = i * 2;
			int offset1 = offset0 + 1;

			int b0 = m_boneIndices[ offset0 ];
			int b1 = m_boneIndices[ offset1 ];
			float weight1 = m_boneWeights[ offset1 ];

			MulPoint3x4_XYZW( ref deformedVertex, ref m_bones[ b0 ], m_baseVertices[ offset0 ] );
			if ( weight1 != 0 )
				MulAddPoint3x4_XYZW( ref deformedVertex, ref m_bones[ b1 ], m_baseVertices[ offset1 ] );

			m_currVertices[ i ] = deformedVertex;
		}
	}

	private void UpdateVerticesBone4()
	{
		Vector3 deformedVertex = Vector3.zero;
		for ( int i = 0; i < m_vertexCount; i++ )
		{
			int offset0 = i * 4;
			int offset1 = offset0 + 1;
			int offset2 = offset0 + 2;
			int offset3 = offset0 + 3;

			int b0 = m_boneIndices[ offset0 ];
			int b1 = m_boneIndices[ offset1 ];
			int b2 = m_boneIndices[ offset2 ];
			int b3 = m_boneIndices[ offset3 ];

			float weight1 = m_boneWeights[ offset1 ];
			float weight2 = m_boneWeights[ offset2 ];
			float weight3 = m_boneWeights[ offset3 ];

			MulPoint3x4_XYZW( ref deformedVertex, ref m_bones[ b0 ], m_baseVertices[ offset0 ] );
			if ( weight1 != 0 )
				MulAddPoint3x4_XYZW( ref deformedVertex, ref m_bones[ b1 ], m_baseVertices[ offset1 ] );
			if ( weight2 != 0 )
				MulAddPoint3x4_XYZW( ref deformedVertex, ref m_bones[ b2 ], m_baseVertices[ offset2 ] );
			if ( weight3 != 0 )
				MulAddPoint3x4_XYZW( ref deformedVertex, ref m_bones[ b3 ], m_baseVertices[ offset3 ] );

			m_currVertices[ i ] = deformedVertex;
		}
	}

	internal override void AsyncUpdate()
	{
		try
		{
			AsyncUpdateVertices( m_starting );
		}
		catch ( System.Exception e )
		{
			IssueError( "[AmplifyMotion] Failed on SkinnedMeshRenderer data. Please contact support.\n" + e.Message );
		}
		finally
		{
			m_asyncUpdateSignal.Set();
		}
	}

	internal override void UpdateTransform( CommandBuffer updateCB, bool starting )
	{
		if ( !m_initialized )
		{
			Initialize();
			return;
		}

		Profiler.BeginSample( "Skinned.Update" );

		if ( !starting && m_wasVisible )
			m_prevLocalToWorld = m_currLocalToWorld;

		bool isVisible = m_renderer.isVisible;

		if ( !m_error && ( isVisible || starting ) )
		{
			UpdateBones();

			m_starting = !m_wasVisible || starting;

			if ( !m_useFallback )
			{
				if ( !m_useGPU )
				{
					m_asyncUpdateSignal.Reset();
					m_asyncUpdateTriggered = true;
					m_owner.Instance.WorkerPool.EnqueueAsyncUpdate( this );
				}
				else
					UpdateVerticesGPU( updateCB, m_starting );
			}
			else
				UpdateVerticesFallback( m_starting );
		}

		if ( !m_useFallback )
		{
			m_currLocalToWorld = m_transform.localToWorldMatrix;
		}
		else
		{
		#if UNITY_2017_1_OR_NEWER
			m_currLocalToWorld = m_transform.localToWorldMatrix;
		#else
			m_currLocalToWorld = Matrix4x4.TRS( m_transform.position, m_transform.rotation, Vector3.one );
		#endif
		}

		if ( starting || !m_wasVisible )
			m_prevLocalToWorld = m_currLocalToWorld;

		m_wasVisible = isVisible;

		Profiler.EndSample();
	}

	private void WaitForAsyncUpdate()
	{
		if ( m_asyncUpdateTriggered )
		{
			if ( !m_asyncUpdateSignal.WaitOne( MotionState.AsyncUpdateTimeout ) )
			{
				Debug.LogWarning( "[AmplifyMotion] Aborted abnormally long Async Skin deform operation. Not a critical error but might indicate a problem. Please contact support." );
				return;
			}
			m_asyncUpdateTriggered = false;
		}
	}

	internal override void RenderVectors( Camera camera, CommandBuffer renderCB, float scale, AmplifyMotion.Quality quality )
	{
		if ( m_initialized && !m_error && m_renderer.isVisible )
		{
			Profiler.BeginSample( "Skinned.Update" );

			if ( !m_useFallback )
			{
				if ( !m_useGPU )
					WaitForAsyncUpdate();
			}

			Profiler.EndSample();

			Profiler.BeginSample( "Skinned.Render" );
			if ( !m_useGPU )
			{
				if ( !m_useFallback )
					m_clonedMesh.vertices = m_currVertices;
				m_clonedMesh.normals = m_prevVertices;
			}

			const float rcp255 = 1 / 255.0f;
			bool mask = ( m_owner.Instance.CullingMask & ( 1 << m_obj.gameObject.layer ) ) != 0;
			int objectId = mask ? m_owner.Instance.GenerateObjectId( m_obj.gameObject ) : 255;

			float cameraMotionScale = mask ? m_owner.Instance.CameraMotionMult * scale : 0;
			float objectMotionScale = mask ? m_owner.Instance.ObjectMotionMult * scale : 0;

			renderCB.SetGlobalMatrix( "_AM_MATRIX_PREV_M", ( Matrix4x4 )m_prevLocalToWorld );
			renderCB.SetGlobalMatrix( "_AM_MATRIX_CURR_M", ( Matrix4x4 )m_currLocalToWorld );
			renderCB.SetGlobalVector( "_AM_MOTION_PARAMS", new Vector4( cameraMotionScale, objectMotionScale, objectId * rcp255, 0 ) );

			if ( m_useGPU )
			{
				Vector4 vertexTexelSize = new Vector4( 1.0f / m_gpuVertexTexWidth, 1.0f / m_gpuVertexTexHeight, m_gpuVertexTexWidth, m_gpuVertexTexHeight );

				renderCB.SetGlobalVector( "_AM_VERTEX_TEXEL_SIZE", vertexTexelSize );
				renderCB.SetGlobalVector( "_AM_VERTEX_TEXEL_HALFSIZE", vertexTexelSize * 0.5f );

				renderCB.SetGlobalTexture( "_AM_PREV_VERTEX_TEX", m_gpuPrevVertices );
				renderCB.SetGlobalTexture( "_AM_CURR_VERTEX_TEX", m_gpuCurrVertices );
			}

			int hardwarePass = m_useGPU ? 4 : 0;
			int qualityPass = ( quality == AmplifyMotion.Quality.Mobile ) ? 0 : 2;
			int basePass = hardwarePass + qualityPass;

			for ( int i = 0; i < m_sharedMaterials.Length; i++ )
			{
				MaterialDesc matDesc = m_sharedMaterials[ i ];
				int pass = basePass + ( matDesc.coverage ? 1 : 0 );

				if ( matDesc.coverage )
				{
					Texture mainTex = matDesc.material.mainTexture;
					if ( mainTex != null )
						matDesc.propertyBlock.SetTexture( "_MainTex", mainTex );
					if ( matDesc.cutoff )
						matDesc.propertyBlock.SetFloat( "_Cutoff", matDesc.material.GetFloat( "_Cutoff" ) );
				}

				renderCB.DrawMesh( m_clonedMesh, m_currLocalToWorld, m_owner.Instance.SkinnedVectorsMaterial, i, pass, matDesc.propertyBlock );
			}

			Profiler.EndSample();
		}
	}
}
}
