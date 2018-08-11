// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
#define UNITY_5
#endif

using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

namespace AmplifyMotion
{
internal class ClothState : AmplifyMotion.MotionState
{
	private Cloth m_cloth;
	private Renderer m_renderer;

	private Matrix3x4 m_prevLocalToWorld;
	private Matrix3x4 m_currLocalToWorld;

	private int m_targetVertexCount;
	private int[] m_targetRemap;
	private Vector3[] m_prevVertices;
	private Vector3[] m_currVertices;

	private Mesh m_clonedMesh;

	private MaterialDesc[] m_sharedMaterials;

	private bool m_starting;
	private bool m_wasVisible;

	private static HashSet<AmplifyMotionObjectBase> m_uniqueWarnings = new HashSet<AmplifyMotionObjectBase>();

	public ClothState( AmplifyMotionCamera owner, AmplifyMotionObjectBase obj )
		: base( owner, obj )
	{
		m_cloth = m_obj.GetComponent<Cloth>();
	}

	void IssueError( string message )
	{
		if ( !m_uniqueWarnings.Contains( m_obj ) )
		{
			Debug.LogWarning( message );
			m_uniqueWarnings.Add( m_obj );
		}
		m_error = true;
	}

	internal override void Initialize()
	{
		if ( m_cloth.vertices == null )
		{
			IssueError( "[AmplifyMotion] Invalid " + m_cloth.GetType().Name + " vertices in object " + m_obj.name + ". Skipping." );
			return;
		}

		SkinnedMeshRenderer skinnedRenderer = m_cloth.gameObject.GetComponent<SkinnedMeshRenderer>();
		Mesh clothMesh = skinnedRenderer.sharedMesh;

		if ( clothMesh == null || clothMesh.vertices == null || clothMesh.triangles == null )
		{
			IssueError( "[AmplifyMotion] Invalid Mesh on Cloth-enabled object " + m_obj.name );
			return;
		}

		base.Initialize();

		m_renderer = m_cloth.gameObject.GetComponent<Renderer>();

		int meshVertexCount = clothMesh.vertexCount;
		Vector3[] meshVertices = clothMesh.vertices;
		Vector2[] meshTexcoords = clothMesh.uv;
		int[] meshTriangles = clothMesh.triangles;

		m_targetRemap = new int[ meshVertexCount ];

		if ( m_cloth.vertices.Length == clothMesh.vertices.Length )
		{
			for ( int i = 0; i < meshVertexCount; i++ )
				m_targetRemap[ i ] = i;
		}
		else
		{
			// a) May contains duplicated verts, optimization/cleanup is required
			Dictionary<Vector3, int> dict = new Dictionary<Vector3, int>();
			int original, vertexCount = 0;

			for ( int i = 0; i < meshVertexCount; i++ )
			{
				if ( dict.TryGetValue( meshVertices[ i ], out original ) )
					m_targetRemap[ i ] = original;
				else
				{
					m_targetRemap[ i ] = vertexCount;
					dict.Add( meshVertices[ i ], vertexCount++ );
				}
			}

			// b) Tear is activated, creates extra verts (NOT SUPPORTED, POOL OF VERTS USED, NO ACCESS TO TRIANGLES)
		}

		m_targetVertexCount = meshVertexCount;
		m_prevVertices = new Vector3[ m_targetVertexCount ];
		m_currVertices = new Vector3[ m_targetVertexCount ];

		m_clonedMesh = new Mesh();
		m_clonedMesh.vertices = meshVertices;
		m_clonedMesh.normals = meshVertices;
		m_clonedMesh.uv = meshTexcoords;
		m_clonedMesh.triangles = meshTriangles;

		m_sharedMaterials = ProcessSharedMaterials( m_renderer.sharedMaterials );

		m_wasVisible = false;
	}

	internal override void Shutdown()
	{
		Mesh.Destroy( m_clonedMesh );
	}

	internal override void UpdateTransform( CommandBuffer updateCB, bool starting )
	{
		if ( !m_initialized )
		{
			Initialize();
			return;
		}

		Profiler.BeginSample( "Cloth.Update" );

		if ( !starting && m_wasVisible )
			m_prevLocalToWorld = m_currLocalToWorld;

	    bool isVisible = m_renderer.isVisible;
		if ( !m_error && ( isVisible || starting ) )
		{
			if ( !starting && m_wasVisible )
				Array.Copy( m_currVertices, m_prevVertices, m_targetVertexCount );
		}

		m_currLocalToWorld = Matrix4x4.TRS( m_transform.position, m_transform.rotation, Vector3.one );

		if ( starting || !m_wasVisible )
			m_prevLocalToWorld = m_currLocalToWorld;

		m_starting = starting;
		m_wasVisible = isVisible;

		Profiler.EndSample();
	}

	internal override void RenderVectors( Camera camera, CommandBuffer renderCB, float scale, AmplifyMotion.Quality quality )
	{
		if ( m_initialized && !m_error && m_renderer.isVisible )
		{
			Profiler.BeginSample( "Cloth.Render" );

			const float rcp255 = 1 / 255.0f;
			bool mask = ( m_owner.Instance.CullingMask & ( 1 << m_obj.gameObject.layer ) ) != 0;
			int objectId = mask ? m_owner.Instance.GenerateObjectId( m_obj.gameObject ) : 255;

			Vector3[] clothVertices = m_cloth.vertices;
			for ( int i = 0; i < m_targetVertexCount; i++ )
				m_currVertices[ i ] = clothVertices[ m_targetRemap[ i ] ];

			if ( m_starting || !m_wasVisible )
				Array.Copy( m_currVertices, m_prevVertices, m_targetVertexCount );

			m_clonedMesh.vertices = m_currVertices;
			m_clonedMesh.normals = m_prevVertices;

			float cameraMotionScale = mask ? m_owner.Instance.CameraMotionMult * scale : 0;
			float objectMotionScale = mask ? m_owner.Instance.ObjectMotionMult * scale : 0;

			renderCB.SetGlobalMatrix( "_AM_MATRIX_PREV_M", ( Matrix4x4 )m_prevLocalToWorld );
			renderCB.SetGlobalMatrix( "_AM_MATRIX_CURR_M", ( Matrix4x4 )m_currLocalToWorld );
			renderCB.SetGlobalVector( "_AM_MOTION_PARAMS", new Vector4( cameraMotionScale, objectMotionScale, objectId * rcp255, 0 ) );

			int qualityPass = ( quality == AmplifyMotion.Quality.Mobile ) ? 0 : 2;

			for ( int i = 0; i < m_sharedMaterials.Length; i++ )
			{
				MaterialDesc matDesc = m_sharedMaterials[ i ];
				int pass = qualityPass + ( matDesc.coverage ? 1 : 0 );

				if ( matDesc.coverage )
				{
					Texture mainTex = matDesc.material.mainTexture;
					if ( mainTex != null )
						matDesc.propertyBlock.SetTexture( "_MainTex", mainTex );
					if ( matDesc.cutoff )
						matDesc.propertyBlock.SetFloat( "_Cutoff", matDesc.material.GetFloat( "_Cutoff" ) );
				}

				renderCB.DrawMesh( m_clonedMesh, m_currLocalToWorld, m_owner.Instance.ClothVectorsMaterial, i, pass, matDesc.propertyBlock );
			}

			Profiler.EndSample();
		}
	}
}
}
