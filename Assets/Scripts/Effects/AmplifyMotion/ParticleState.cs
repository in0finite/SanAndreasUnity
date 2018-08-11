// Amplify Motion - Full-scene Motion Blur for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
#define UNITY_5
#endif
#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_PRE_5_3
#endif

using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif

#if !UNITY_PRE_5_3
using UnityEngine.Rendering;

namespace AmplifyMotion
{
internal class ParticleState : AmplifyMotion.MotionState
{
	protected class Particle
	{
		public int refCount;
		public Matrix3x4 prevLocalToWorld;
		public Matrix3x4 currLocalToWorld;
	}

	public ParticleSystem m_particleSystem;
	public ParticleSystemRenderer m_renderer;

	private Mesh m_mesh;

	private ParticleSystem.RotationOverLifetimeModule rotationOverLifetime;
	private ParticleSystem.RotationBySpeedModule rotationBySpeed;

	private ParticleSystem.Particle[] m_particles;
	private Dictionary<uint, Particle> m_particleDict;
	private List<uint> m_listToRemove;
	private Stack<Particle> m_particleStack;
	private int m_capacity;

	private MaterialDesc[] m_sharedMaterials;

	private bool m_moved = false;
	private bool m_wasVisible;

	private static HashSet<AmplifyMotionObjectBase> m_uniqueWarnings = new HashSet<AmplifyMotionObjectBase>();

	public ParticleState( AmplifyMotionCamera owner, AmplifyMotionObjectBase obj )
		: base( owner, obj )
	{
		m_particleSystem = m_obj.GetComponent<ParticleSystem>();
		m_renderer = m_particleSystem.GetComponent<ParticleSystemRenderer>();
		rotationOverLifetime = m_particleSystem.rotationOverLifetime;
		rotationBySpeed = m_particleSystem.rotationBySpeed;
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

	private Mesh CreateBillboardMesh()
	{
		int[] tris = new int[ 6 ] { 0, 1, 2, 2, 3, 0 };

		Vector3[] vertices = new Vector3[ 4 ] {
			new Vector3( -0.5f, -0.5f, 0 ),
			new Vector3( 0.5f, -0.5f, 0 ),
			new Vector3( 0.5f, 0.5f, 0 ),
			new Vector3( -0.5f, 0.5f, 0 ) };

		Vector2[] uv = new Vector2[ 4 ] {
			new Vector2( 0, 0 ),
			new Vector2( 1, 0 ),
			new Vector2( 1, 1 ),
			new Vector2( 0, 1 ) };

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = tris;
		return mesh;
	}

	private Mesh CreateStretchedBillboardMesh()
	{
		int[] tris = new int[ 6 ] { 0, 1, 2, 2, 3, 0 };

		Vector3[] vertices = new Vector3[ 4 ] {
			new Vector3( 0, -0.5f, -1.0f ),
			new Vector3( 0, -0.5f, 0.0f ),
			new Vector3( 0, 0.5f, 0.0f ),
			new Vector3( 0, 0.5f, -1.0f )
		};

		Vector2[] uv = new Vector2[ 4 ] {
			new Vector2( 1, 1 ),
			new Vector2( 0, 1 ),
			new Vector2( 0, 0 ),
			new Vector2( 1, 0 ) };

		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = tris;
		return mesh;
	}

	internal override void Initialize()
	{
		if ( m_renderer == null )
		{
			IssueError( "[AmplifyMotion] Missing/Invalid Particle Renderer in object " + m_obj.name + ". Skipping." );
			return;
		}

		base.Initialize();

		if ( m_renderer.renderMode == ParticleSystemRenderMode.Mesh )
			m_mesh = m_renderer.mesh;
		else if ( m_renderer.renderMode == ParticleSystemRenderMode.Stretch )
			m_mesh = CreateStretchedBillboardMesh();
		else
			m_mesh = CreateBillboardMesh();

		m_sharedMaterials = ProcessSharedMaterials( m_renderer.sharedMaterials );
#if UNITY_5_5_OR_NEWER
			m_capacity = m_particleSystem.main.maxParticles;
#else
			m_capacity = m_particleSystem.maxParticles;
#endif
			m_particleDict = new Dictionary<uint, Particle>( m_capacity );
		m_particles = new ParticleSystem.Particle[ m_capacity ];
		m_listToRemove = new List<uint>( m_capacity );
		m_particleStack = new Stack<Particle>( m_capacity );

		for ( int k = 0; k < m_capacity; k++ )
			m_particleStack.Push( new Particle() );

		m_wasVisible = false;
	}

	void RemoveDeadParticles()
	{
		m_listToRemove.Clear();

		var enumerator = m_particleDict.GetEnumerator();
		while ( enumerator.MoveNext() )
		{
			KeyValuePair<uint, Particle> pair = enumerator.Current;

			if ( pair.Value.refCount <= 0 )
			{
				m_particleStack.Push( pair.Value );
				if(!m_listToRemove.Contains(pair.Key))
					m_listToRemove.Add( pair.Key );
			}
			else
				pair.Value.refCount = 0;
		}

		for ( int i = 0; i < m_listToRemove.Count; i++ )
			m_particleDict.Remove( m_listToRemove[ i ] );
	}

	internal override void UpdateTransform( CommandBuffer updateCB, bool starting )
	{
#if UNITY_5_5_OR_NEWER
		int particleCount = m_particleSystem.main.maxParticles;
#else
		int particleCount = m_particleSystem.maxParticles;
#endif

			if ( !m_initialized || m_capacity != particleCount )
		{
			Initialize();
			return;
		}

		Profiler.BeginSample( "Particle.Update" );

		if ( !starting && m_wasVisible )
		{
			var enumerator = m_particleDict.GetEnumerator();
			while ( enumerator.MoveNext() )
			{
				Particle particle = enumerator.Current.Value;
				particle.prevLocalToWorld = particle.currLocalToWorld;
			}
		}

		m_moved = true;

		int numAlive = m_particleSystem.GetParticles( m_particles );

		Matrix4x4 transformLocalToWorld = Matrix4x4.TRS( m_transform.position, m_transform.rotation, Vector3.one );

		bool separateAxes = ( rotationOverLifetime.enabled && rotationOverLifetime.separateAxes ) ||
							( rotationBySpeed.enabled && rotationBySpeed.separateAxes );

		for ( int i = 0; i < numAlive; i++ )
		{
			uint seed = m_particles[ i ].randomSeed;
			Particle particle;

			bool justSpawned = false;
			if ( !m_particleDict.TryGetValue( seed, out particle ) && m_particleStack.Count > 0 )
			{
				m_particleDict[ seed ] = particle = m_particleStack.Pop();
				justSpawned = true;
			}

			if ( particle == null )
				continue;

			float currentSize = m_particles[ i ].GetCurrentSize( m_particleSystem );
			Vector3 size = new Vector3( currentSize, currentSize, currentSize );

			Matrix4x4 particleCurrLocalToWorld;
			if ( m_renderer.renderMode == ParticleSystemRenderMode.Mesh )
			{
				Quaternion rotation;
				if ( separateAxes )
					rotation = Quaternion.Euler( m_particles[ i ].rotation3D );
				else
					rotation = Quaternion.AngleAxis( m_particles[ i ].rotation, m_particles[ i ].axisOfRotation );

				Matrix4x4 particleMatrix = Matrix4x4.TRS( m_particles[ i ].position, rotation, size );
#if UNITY_5_5_OR_NEWER
				if ( m_particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.World )
#else
				if ( m_particleSystem.simulationSpace == ParticleSystemSimulationSpace.World )
#endif
					particleCurrLocalToWorld = particleMatrix;
				else
					particleCurrLocalToWorld = transformLocalToWorld * particleMatrix;
			}
			else if ( m_renderer.renderMode == ParticleSystemRenderMode.Billboard )
			{
#if UNITY_5_5_OR_NEWER
				if ( m_particleSystem.main.simulationSpace == ParticleSystemSimulationSpace.Local )
#else
				if ( m_particleSystem.simulationSpace == ParticleSystemSimulationSpace.Local )
#endif
						m_particles[ i ].position = transformLocalToWorld.MultiplyPoint( m_particles[ i ].position );

				Quaternion rotation;
				if ( separateAxes )
					rotation = Quaternion.Euler( -m_particles[ i ].rotation3D.x, -m_particles[ i ].rotation3D.y, m_particles[ i ].rotation3D.z );
				else
					rotation = Quaternion.AngleAxis( m_particles[ i ].rotation, Vector3.back );

				particleCurrLocalToWorld = Matrix4x4.TRS( m_particles[ i ].position, m_owner.Transform.rotation * rotation, size );
			}
			else
			{
				// unsupported
				particleCurrLocalToWorld = Matrix4x4.identity;
			}

			particle.refCount = 1;
			particle.currLocalToWorld = particleCurrLocalToWorld;
			if ( justSpawned )
				particle.prevLocalToWorld = particle.currLocalToWorld;
		}

		if ( starting || !m_wasVisible )
		{
			var enumerator = m_particleDict.GetEnumerator();
			while ( enumerator.MoveNext() )
			{
				Particle particle = enumerator.Current.Value;
				particle.prevLocalToWorld = particle.currLocalToWorld;
			}
		}

		RemoveDeadParticles();

		m_wasVisible = m_renderer.isVisible;

		Profiler.EndSample();
	}

	internal override void RenderVectors( Camera camera, CommandBuffer renderCB, float scale, AmplifyMotion.Quality quality )
	{
		Profiler.BeginSample( "Particle.Render" );

		// TODO: batch

		if ( m_initialized && !m_error && m_renderer.isVisible )
		{
			bool mask = ( m_owner.Instance.CullingMask & ( 1 << m_obj.gameObject.layer ) ) != 0;
			if ( !mask || ( mask && m_moved ) )
			{
				const float rcp255 = 1 / 255.0f;
				int objectId = mask ? m_owner.Instance.GenerateObjectId( m_obj.gameObject ) : 255;

				renderCB.SetGlobalFloat( "_AM_OBJECT_ID", objectId * rcp255 );
				renderCB.SetGlobalFloat( "_AM_MOTION_SCALE", mask ? scale : 0 );

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

					var enumerator = m_particleDict.GetEnumerator();
					while ( enumerator.MoveNext() )
					{
						KeyValuePair<uint, Particle> pair = enumerator.Current;

						Matrix4x4 prevModelViewProj = m_owner.PrevViewProjMatrixRT * ( Matrix4x4 ) pair.Value.prevLocalToWorld;
						renderCB.SetGlobalMatrix( "_AM_MATRIX_PREV_MVP", prevModelViewProj );

						renderCB.DrawMesh( m_mesh, pair.Value.currLocalToWorld, m_owner.Instance.SolidVectorsMaterial, i, pass, matDesc.propertyBlock );
					}
				}
			}
		}

		Profiler.EndSample();
	}
}
}

#endif
