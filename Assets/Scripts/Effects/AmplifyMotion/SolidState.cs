// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
#define UNITY_5
#endif

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
namespace AmplifyMotion
{
internal class SolidState : AmplifyMotion.MotionState
{
	private MeshRenderer m_meshRenderer;

	private Matrix3x4 m_prevLocalToWorld;
	private Matrix3x4 m_currLocalToWorld;

	private Mesh m_mesh;

	private MaterialDesc[] m_sharedMaterials;

	public bool m_moved = false;
	private bool m_wasVisible;

	private static HashSet<AmplifyMotionObjectBase> m_uniqueWarnings = new HashSet<AmplifyMotionObjectBase>();

	public SolidState( AmplifyMotionCamera owner, AmplifyMotionObjectBase obj )
		: base( owner, obj )
	{
		m_meshRenderer = m_obj.GetComponent<MeshRenderer>();
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
		MeshFilter meshFilter = m_obj.GetComponent<MeshFilter>();
		if ( meshFilter == null || meshFilter.sharedMesh == null )
		{
			IssueError( "[AmplifyMotion] Invalid MeshFilter/Mesh in object " + m_obj.name + ". Skipping." );
			return;
		}

		base.Initialize();

		m_mesh = meshFilter.sharedMesh;

		m_sharedMaterials = ProcessSharedMaterials( m_meshRenderer.sharedMaterials );

		m_wasVisible = false;
	}

	internal override void UpdateTransform( CommandBuffer updateCB, bool starting )
	{
		if ( !m_initialized )
		{
			Initialize();
			return;
		}

		Profiler.BeginSample( "Solid.Update" );

		if ( !starting && m_wasVisible )
			m_prevLocalToWorld = m_currLocalToWorld;

		m_currLocalToWorld = m_transform.localToWorldMatrix;

		m_moved = true;
		if ( !m_owner.Overlay )
			m_moved = starting || MatrixChanged( m_currLocalToWorld, m_prevLocalToWorld );

		if ( starting || !m_wasVisible )
			m_prevLocalToWorld = m_currLocalToWorld;

		m_wasVisible = m_meshRenderer.isVisible;

		Profiler.EndSample();
	}

	internal override void RenderVectors( Camera camera, CommandBuffer renderCB, float scale, AmplifyMotion.Quality quality )
	{
		if ( m_initialized && !m_error && m_meshRenderer.isVisible )
		{
			Profiler.BeginSample( "Solid.Render" );

			bool mask = ( m_owner.Instance.CullingMask & ( 1 << m_obj.gameObject.layer ) ) != 0;
			if ( !mask || ( mask && m_moved ) )
			{
				const float rcp255 = 1 / 255.0f;
				int objectId = mask ? m_owner.Instance.GenerateObjectId( m_obj.gameObject ) : 255;

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

					renderCB.DrawMesh( m_mesh, m_transform.localToWorldMatrix, m_owner.Instance.SolidVectorsMaterial, i, pass, matDesc.propertyBlock );
				}
			}

			Profiler.EndSample();
		}
	}
}
}
