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
using UnityEngine.Rendering;

namespace AmplifyMotion
{

public enum ObjectType
{
	None,
	Solid,
	Skinned,
	Cloth,
#if !UNITY_PRE_5_3
	Particle
#endif
}

[Serializable]
internal abstract class MotionState
{
	protected struct MaterialDesc
	{
		public Material material;
		public MaterialPropertyBlock propertyBlock;
		public bool coverage;
		public bool cutoff;
	}

	protected struct Matrix3x4
	{
		public float m00, m01, m02, m03;
		public float m10, m11, m12, m13;
		public float m20, m21, m22, m23;

		public Vector4 GetRow( int i )
		{
			if ( i == 0 )
				return new Vector4( m00, m01, m02, m03 );
			else if ( i == 1 )
				return new Vector4( m10, m11, m12, m13 );
			else if ( i == 2 )
				return new Vector4( m20, m21, m22, m23 );
			else
				return new Vector4( 0, 0, 0, 1 );
		}

		public static implicit operator Matrix3x4( Matrix4x4 from )
		{
			Matrix3x4 to = new Matrix3x4();
			to.m00 = from.m00; to.m01 = from.m01; to.m02 = from.m02; to.m03 = from.m03;
			to.m10 = from.m10; to.m11 = from.m11; to.m12 = from.m12; to.m13 = from.m13;
			to.m20 = from.m20; to.m21 = from.m21; to.m22 = from.m22; to.m23 = from.m23;
			return to;
		}

		public static implicit operator Matrix4x4( Matrix3x4 from )
		{
			Matrix4x4 to = new Matrix4x4();
			to.m00 = from.m00; to.m01 = from.m01; to.m02 = from.m02; to.m03 = from.m03;
			to.m10 = from.m10; to.m11 = from.m11; to.m12 = from.m12; to.m13 = from.m13;
			to.m20 = from.m20; to.m21 = from.m21; to.m22 = from.m22; to.m23 = from.m23;
			to.m30 = to.m31 = to.m32 = 0;
			to.m33 = 1;
			return to;
		}

		public static Matrix3x4 operator * ( Matrix3x4 a, Matrix3x4 b )
		{
			Matrix3x4 to = new Matrix3x4();
			to.m00 = a.m00 * b.m00 + a.m01 * b.m10 + a.m02 * b.m20;
			to.m01 = a.m00 * b.m01 + a.m01 * b.m11 + a.m02 * b.m21;
			to.m02 = a.m00 * b.m02 + a.m01 * b.m12 + a.m02 * b.m22;
			to.m03 = a.m00 * b.m03 + a.m01 * b.m13 + a.m02 * b.m23 + a.m03;
			to.m10 = a.m10 * b.m00 + a.m11 * b.m10 + a.m12 * b.m20;
			to.m11 = a.m10 * b.m01 + a.m11 * b.m11 + a.m12 * b.m21;
			to.m12 = a.m10 * b.m02 + a.m11 * b.m12 + a.m12 * b.m22;
			to.m13 = a.m10 * b.m03 + a.m11 * b.m13 + a.m12 * b.m23 + a.m13;
			to.m20 = a.m20 * b.m00 + a.m21 * b.m10 + a.m22 * b.m20;
			to.m21 = a.m20 * b.m01 + a.m21 * b.m11 + a.m22 * b.m21;
			to.m22 = a.m20 * b.m02 + a.m21 * b.m12 + a.m22 * b.m22;
			to.m23 = a.m20 * b.m03 + a.m21 * b.m13 + a.m22 * b.m23 + a.m23;
			return to;
		}
	}

	public const int AsyncUpdateTimeout = 100;

	protected bool m_error;
	protected bool m_initialized;
	protected Transform m_transform;

	protected AmplifyMotionCamera m_owner;
	protected AmplifyMotionObjectBase m_obj;

	public AmplifyMotionCamera Owner { get { return m_owner; } }
	public bool Initialized { get { return m_initialized; } }
	public bool Error { get { return m_error; } }

	public MotionState( AmplifyMotionCamera owner, AmplifyMotionObjectBase obj )
	{
		m_error = false;
		m_initialized = false;

		m_owner = owner;
		m_obj = obj;
		m_transform = obj.transform;
	}

	internal virtual void Initialize() { m_initialized = true; }
	internal virtual void Shutdown() {}

	internal virtual void AsyncUpdate() {}
	internal abstract void UpdateTransform( CommandBuffer updateCB, bool starting );
	internal virtual void RenderVectors( Camera camera, CommandBuffer renderCB, float scale, AmplifyMotion.Quality quality ) {}
	internal virtual void RenderDebugHUD() {}

	private static HashSet<Material> m_materialWarnings = new HashSet<Material>();

	protected MaterialDesc[] ProcessSharedMaterials( Material[] mats )
	{
		MaterialDesc[] matsDesc = new MaterialDesc [ mats.Length ];
		for ( int i = 0; i < mats.Length; i++ )
		{
			matsDesc[ i ].material = mats[ i ];
			if ( mats[ i ] != null )
			{
				bool legacyCoverage = ( mats[ i ].GetTag( "RenderType", false ) == "TransparentCutout" );
				bool isCoverage = legacyCoverage || mats[ i ].IsKeywordEnabled( "_ALPHATEST_ON" );
				matsDesc[ i ].propertyBlock = new MaterialPropertyBlock();
				matsDesc[ i ].coverage = mats[ i ].HasProperty( "_MainTex" ) && isCoverage;
				matsDesc[ i ].cutoff = mats[ i ].HasProperty( "_Cutoff" );

				if ( isCoverage && !matsDesc[ i ].coverage && !m_materialWarnings.Contains( matsDesc[ i ].material ) )
				{
					Debug.LogWarning( "[AmplifyMotion] TransparentCutout material \"" + matsDesc[ i ].material.name + "\" {" + matsDesc[ i ].material.shader.name + "} not using _MainTex standard property." );
					m_materialWarnings.Add( matsDesc[ i ].material );
				}
			}
		}
		return matsDesc;
	}

	protected static bool MatrixChanged( Matrix3x4 a, Matrix3x4 b )
	{
		if ( Vector4.SqrMagnitude( new Vector4( a.m00 - b.m00, a.m01 - b.m01, a.m02 - b.m02, a.m03 - b.m03 ) ) > 0.0f )
			return true;
		if ( Vector4.SqrMagnitude( new Vector4( a.m10 - b.m10, a.m11 - b.m11, a.m12 - b.m12, a.m13 - b.m13 ) ) > 0.0f )
			return true;
		if ( Vector4.SqrMagnitude( new Vector4( a.m20 - b.m20, a.m21 - b.m21, a.m22 - b.m22, a.m23 - b.m23 ) ) > 0.0f )
			return true;
		return false;
	}

	protected static void MulPoint3x4_XYZ( ref Vector3 result, ref Matrix3x4 mat, Vector4 vec )
	{
		result.x = mat.m00 * vec.x + mat.m01 * vec.y + mat.m02 * vec.z + mat.m03;
		result.y = mat.m10 * vec.x + mat.m11 * vec.y + mat.m12 * vec.z + mat.m13;
		result.z = mat.m20 * vec.x + mat.m21 * vec.y + mat.m22 * vec.z + mat.m23;
	}

	protected static void MulPoint3x4_XYZW( ref Vector3 result, ref Matrix3x4 mat, Vector4 vec )
	{
		result.x = mat.m00 * vec.x + mat.m01 * vec.y + mat.m02 * vec.z + mat.m03 * vec.w;
		result.y = mat.m10 * vec.x + mat.m11 * vec.y + mat.m12 * vec.z + mat.m13 * vec.w;
		result.z = mat.m20 * vec.x + mat.m21 * vec.y + mat.m22 * vec.z + mat.m23 * vec.w;
	}

	protected static void MulAddPoint3x4_XYZW( ref Vector3 result, ref Matrix3x4 mat, Vector4 vec )
	{
		result.x += mat.m00 * vec.x + mat.m01 * vec.y + mat.m02 * vec.z + mat.m03 * vec.w;
		result.y += mat.m10 * vec.x + mat.m11 * vec.y + mat.m12 * vec.z + mat.m13 * vec.w;
		result.z += mat.m20 * vec.x + mat.m21 * vec.y + mat.m22 * vec.z + mat.m23 * vec.w;
	}
}
}

[AddComponentMenu( "" )]
public class AmplifyMotionObjectBase : MonoBehaviour
{
	public enum MinMaxCurveState
	{
		Scalar = 0,
		Curve = 1,
		TwoCurves = 2,
		TwoScalars = 3
	}

	internal static bool ApplyToChildren = true;
	[SerializeField] private bool m_applyToChildren = ApplyToChildren;

	private AmplifyMotion.ObjectType m_type = AmplifyMotion.ObjectType.None;
	private Dictionary<Camera, AmplifyMotion.MotionState> m_states = new Dictionary<Camera, AmplifyMotion.MotionState>();

	private bool m_fixedStep = false;
	private int m_objectId = 0;
	private Vector3 m_lastPosition = Vector3.zero;

	internal bool FixedStep { get { return m_fixedStep; } }
	internal int ObjectId { get { return m_objectId; } }

	private int m_resetAtFrame = -1;

	public AmplifyMotion.ObjectType Type { get { return m_type; } }

	internal void RegisterCamera( AmplifyMotionCamera camera )
	{
		Camera actual = camera.GetComponent<Camera>();
		if ( ( actual.cullingMask & ( 1 << gameObject.layer ) ) != 0 && !m_states.ContainsKey( actual ) )
		{
			AmplifyMotion.MotionState state = null;
			switch ( m_type )
			{
				case AmplifyMotion.ObjectType.Solid:
					state = new AmplifyMotion.SolidState( camera, this ); break;
				case AmplifyMotion.ObjectType.Skinned:
					state = new AmplifyMotion.SkinnedState( camera, this );	break;
				case AmplifyMotion.ObjectType.Cloth:
					state = new AmplifyMotion.ClothState( camera, this ); break;
			#if !UNITY_PRE_5_3
				case AmplifyMotion.ObjectType.Particle:
					state = new AmplifyMotion.ParticleState( camera, this ); break;
			#endif
				default:
					throw new Exception( "[AmplifyMotion] Invalid object type." );
			}

			camera.RegisterObject( this );

			m_states.Add( actual, state );
		}
	}

	internal void UnregisterCamera( AmplifyMotionCamera camera )
	{
		AmplifyMotion.MotionState state;
		Camera actual = camera.GetComponent<Camera>();
		if ( m_states.TryGetValue( actual, out state ) )
		{
			camera.UnregisterObject( this );

			if ( m_states.TryGetValue( actual, out state ) )
				state.Shutdown();

			m_states.Remove( actual );
		}
	}

	bool InitializeType()
	{
		Renderer renderer = GetComponent<Renderer>();
		if ( AmplifyMotionEffectBase.CanRegister( gameObject, false ) )
		{
		#if !UNITY_PRE_5_3
			ParticleSystem particleRenderer = GetComponent<ParticleSystem>();
			if ( particleRenderer != null )
			{
				m_type = AmplifyMotion.ObjectType.Particle;
				AmplifyMotionEffectBase.RegisterObject( this );
			}
			else
		#endif
			if ( renderer != null )
			{
				// At this point, Renderer is guaranteed to be one of the following
				if ( renderer.GetType() == typeof( MeshRenderer ) )
					m_type = AmplifyMotion.ObjectType.Solid;
				else if ( renderer.GetType() == typeof( SkinnedMeshRenderer ) )
				{
					if ( GetComponent<Cloth>() != null )
						m_type = AmplifyMotion.ObjectType.Cloth;
					else
						m_type = AmplifyMotion.ObjectType.Skinned;
				}

				AmplifyMotionEffectBase.RegisterObject( this );
			}
		}

		// No renderer? disable it, it is here just for adding children
		return ( renderer != null );
	}

	void OnEnable()
	{
		bool valid = InitializeType();
		if ( valid )
		{
			if ( m_type == AmplifyMotion.ObjectType.Cloth )
			{
				m_fixedStep = false;
			}
			else if ( m_type == AmplifyMotion.ObjectType.Solid )
			{
				Rigidbody rigidbody = GetComponent<Rigidbody>();
				if ( rigidbody != null && rigidbody.interpolation == RigidbodyInterpolation.None && !rigidbody.isKinematic )
					m_fixedStep = true;
			}
		}

		if ( m_applyToChildren )
		{
			foreach ( Transform child in gameObject.transform )
				AmplifyMotionEffectBase.RegisterRecursivelyS( child.gameObject );
		}

		if ( !valid )
			enabled = false;
	}

	void OnDisable()
	{
		AmplifyMotionEffectBase.UnregisterObject( this );
	}

	void TryInitializeStates()
	{
		var enumerator = m_states.GetEnumerator();
		while ( enumerator.MoveNext() )
		{
			AmplifyMotion.MotionState state = enumerator.Current.Value;
			if ( state.Owner.Initialized && !state.Error && !state.Initialized )
				state.Initialize();
		}
	}

	void Start()
	{
		if ( AmplifyMotionEffectBase.Instance != null )
			TryInitializeStates();

		m_lastPosition = transform.position;
	}

	void Update()
	{
		if ( AmplifyMotionEffectBase.Instance != null )
			TryInitializeStates();
	}

	static void RecursiveResetMotionAtFrame( Transform transform, AmplifyMotionObjectBase obj, int frame )
	{
		if ( obj != null )
			obj.m_resetAtFrame = frame;

		foreach ( Transform child in transform )
			RecursiveResetMotionAtFrame( child, child.GetComponent<AmplifyMotionObjectBase>(), frame );
	}

	public void ResetMotionNow()
	{
		RecursiveResetMotionAtFrame( transform, this, Time.frameCount );
	}

	public void ResetMotionAtFrame( int frame )
	{
		RecursiveResetMotionAtFrame( transform, this, frame );
	}

	private void CheckTeleportReset( AmplifyMotionEffectBase inst )
	{
		if ( Vector3.SqrMagnitude( transform.position - m_lastPosition ) > inst.MinResetDeltaDistSqr )
			RecursiveResetMotionAtFrame( transform, this, Time.frameCount + inst.ResetFrameDelay );
	}

	internal void OnUpdateTransform( AmplifyMotionEffectBase inst, Camera camera, CommandBuffer updateCB, bool starting )
	{
		AmplifyMotion.MotionState state;
		if ( m_states.TryGetValue( camera, out state ) )
		{
			if ( !state.Error )
			{
				CheckTeleportReset( inst );
				bool reset = ( m_resetAtFrame > 0 && Time.frameCount >= m_resetAtFrame );
				state.UpdateTransform( updateCB, starting || reset );
			}
		}
		m_lastPosition = transform.position;
	}

	internal void OnRenderVectors( Camera camera, CommandBuffer renderCB, float scale, AmplifyMotion.Quality quality )
	{
		AmplifyMotion.MotionState state;
		if ( m_states.TryGetValue( camera, out state ) )
		{
			if ( !state.Error )
			{
				state.RenderVectors( camera, renderCB, scale, quality );
				if ( m_resetAtFrame > 0 && Time.frameCount >= m_resetAtFrame )
					m_resetAtFrame = -1;
			}
		}
	}

#if UNITY_EDITOR
	internal void OnRenderDebugHUD( Camera camera )
	{
		AmplifyMotion.MotionState state;
		if ( m_states.TryGetValue( camera, out state ) )
		{
			if ( !state.Error )
				state.RenderDebugHUD();
		}
	}
#endif
}
