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
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace AmplifyMotion
{
	public enum Quality
	{
		Mobile = 0,
		Standard = 1,
		Standard_SM3 = 2,
		SoftEdge_SM3 = 3
	}
}

[RequireComponent( typeof( Camera ) )]
[AddComponentMenu( "" )]
public class AmplifyMotionEffectBase : MonoBehaviour
{
	[Header("Motion Blur")]
	public AmplifyMotion.Quality QualityLevel = AmplifyMotion.Quality.Standard;
	public int QualitySteps = 1;
	public float MotionScale = 3.0f;
	public float CameraMotionMult = 1.0f;
	public float ObjectMotionMult = 1.0f;
	public float MinVelocity = 1.0f;
	public float MaxVelocity = 10.0f;
	public float DepthThreshold = 0.01f;
	public bool Noise = false;

	[Header( "Camera" )]
	public Camera[] OverlayCameras = new Camera[ 0 ];
	public LayerMask CullingMask = ~0;

	[Header( "Objects" )]
	public bool AutoRegisterObjs = true;
	public float MinResetDeltaDist = 1000.0f;
	[NonSerialized] public float MinResetDeltaDistSqr = 0;
	public int ResetFrameDelay = 1;

	[Header( "Low-Level" )]
	[FormerlySerializedAs( "workerThreads" )] public int WorkerThreads = 0;
	public bool SystemThreadPool = false;
	public bool ForceCPUOnly = false;
	public bool DebugMode = false;

	// For compatibility
	[Obsolete( "workerThreads is deprecated, please use WorkerThreads instead." )]
	public int workerThreads { get { return WorkerThreads; } set { WorkerThreads = value; } }

	private Camera m_camera;
	private bool m_starting = true;

	private int m_width, m_height;
	private RenderTexture m_motionRT;
	private Material m_blurMaterial;
	private Material m_solidVectorsMaterial;
	private Material m_skinnedVectorsMaterial;
	private Material m_clothVectorsMaterial;
	private Material m_reprojectionMaterial;
	private Material m_combineMaterial;
	private Material m_dilationMaterial;
	private Material m_depthMaterial;
	private Material m_debugMaterial;

	internal Material ReprojectionMaterial { get { return m_reprojectionMaterial; } }
	internal Material SolidVectorsMaterial { get { return m_solidVectorsMaterial; } }
	internal Material SkinnedVectorsMaterial { get { return m_skinnedVectorsMaterial; } }
	internal Material ClothVectorsMaterial { get { return m_clothVectorsMaterial; } }

	internal RenderTexture MotionRenderTexture { get { return m_motionRT; } }

#if TRIAL
	private Texture2D m_watermark;
#endif

	private Dictionary<Camera, AmplifyMotionCamera> m_linkedCameras = new Dictionary<Camera, AmplifyMotionCamera>();
	public Dictionary<Camera, AmplifyMotionCamera> LinkedCameras { get { return m_linkedCameras; } }
	internal Camera[] m_linkedCameraKeys = null;
	internal AmplifyMotionCamera[] m_linkedCameraValues = null;
	internal bool m_linkedCamerasChanged = true;

	private AmplifyMotionPostProcess m_currentPostProcess = null;

	private int m_globalObjectId = 1;

	private float m_deltaTime;
	private float m_fixedDeltaTime;

	private float m_motionScaleNorm;
	private float m_fixedMotionScaleNorm;

	internal float MotionScaleNorm { get { return m_motionScaleNorm; } }
	internal float FixedMotionScaleNorm { get { return m_fixedMotionScaleNorm; } }

	private AmplifyMotion.Quality m_qualityLevel;

	private AmplifyMotionCamera m_baseCamera = null;
	public AmplifyMotionCamera BaseCamera { get { return m_baseCamera; } }

	private AmplifyMotion.WorkerThreadPool m_workerThreadPool = null;
	internal AmplifyMotion.WorkerThreadPool WorkerPool { get { return m_workerThreadPool; } }

	// GLOBAL OBJECT MANAGEMENT
	public static Dictionary<GameObject, AmplifyMotionObjectBase> m_activeObjects = new Dictionary<GameObject, AmplifyMotionObjectBase>();
	public static Dictionary<Camera, AmplifyMotionCamera> m_activeCameras = new Dictionary<Camera, AmplifyMotionCamera>();

	private static bool m_isD3D = false;
	public static bool IsD3D { get { return m_isD3D; } }

	private bool m_canUseGPU = false;
	public bool CanUseGPU { get { return m_canUseGPU; } }

	private const CameraEvent m_updateCBEvent = CameraEvent.BeforeImageEffectsOpaque;
	private CommandBuffer m_updateCB = null;

	private const CameraEvent m_fixedUpdateCBEvent = CameraEvent.BeforeImageEffectsOpaque;
	private CommandBuffer m_fixedUpdateCB = null;

	private static bool m_ignoreMotionScaleWarning = false;
	public static bool IgnoreMotionScaleWarning { get { return m_ignoreMotionScaleWarning; } }

	private static AmplifyMotionEffectBase m_firstInstance = null;
	public static AmplifyMotionEffectBase FirstInstance { get { return m_firstInstance; } }
	public static AmplifyMotionEffectBase Instance { get { return m_firstInstance; } }

	void Awake()
	{
		if ( m_firstInstance == null )
			m_firstInstance = this;

		m_isD3D = SystemInfo.graphicsDeviceVersion.StartsWith( "Direct3D" );
		m_globalObjectId = 1;
		m_width = m_height = 0;

		if ( ForceCPUOnly )
			m_canUseGPU = false;
		else
		{
			bool hasSM3 = ( SystemInfo.graphicsShaderLevel >= 30 );
			bool hasRHalfTex = SystemInfo.SupportsTextureFormat( TextureFormat.RHalf );
			bool hasRGHalfTex = SystemInfo.SupportsTextureFormat( TextureFormat.RGHalf );
			bool hasARGBHalfTex = SystemInfo.SupportsTextureFormat( TextureFormat.RGBAHalf );
			bool hasARGBFloatRT = SystemInfo.SupportsRenderTextureFormat( RenderTextureFormat.ARGBFloat );
		#if UNITY_5_5_OR_NEWER
			m_canUseGPU = hasSM3 && hasRHalfTex && hasRGHalfTex && hasARGBHalfTex && hasARGBFloatRT;
		#else
			bool hasRTs = SystemInfo.supportsRenderTextures;
			m_canUseGPU = hasRTs && hasSM3 && hasRHalfTex && hasRGHalfTex && hasARGBHalfTex && hasARGBFloatRT;
		#endif
		}
	}

	internal void ResetObjectId()
	{
		m_globalObjectId = 1;
	}

	internal int GenerateObjectId( GameObject obj )
	{
		// id = 0, static objs
		// id = 255, excluded objs

		if ( obj.isStatic )
			return 0; // same as background

		m_globalObjectId++;

		// TEMPORARY FIX: wrap around; may cause artifacts on id collision of nearby objs
		if ( m_globalObjectId > 254 )
			m_globalObjectId = 1;

		return m_globalObjectId;
	}

	void SafeDestroyMaterial( ref Material mat )
	{
		if ( mat != null )
		{
			DestroyImmediate( mat );
			mat = null;
		}
	}

	bool CheckMaterialAndShader( Material material, string name )
	{
		bool ok = true;
		if ( material == null || material.shader == null )
		{
			Debug.LogWarning( "[AmplifyMotion] Error creating " + name + " material" );
			ok = false;
		}
		else if ( !material.shader.isSupported )
		{
			Debug.LogWarning( "[AmplifyMotion] " + name + " shader not supported on this platform" );
			ok = false;
		}
		return ok;
	}

	void DestroyMaterials()
	{
		SafeDestroyMaterial( ref m_blurMaterial );
		SafeDestroyMaterial( ref m_solidVectorsMaterial );
		SafeDestroyMaterial( ref m_skinnedVectorsMaterial );
		SafeDestroyMaterial( ref m_clothVectorsMaterial );
		SafeDestroyMaterial( ref m_reprojectionMaterial );
		SafeDestroyMaterial( ref m_combineMaterial );
		SafeDestroyMaterial( ref m_dilationMaterial );
		SafeDestroyMaterial( ref m_depthMaterial );
		SafeDestroyMaterial( ref m_debugMaterial );
	}

	bool CreateMaterials()
	{
		DestroyMaterials();

		int shaderModel = ( SystemInfo.graphicsShaderLevel >= 30 ) ? 3 : 2;

		string blurShader = "Hidden/Amplify Motion/MotionBlurSM" + shaderModel;
		string solidVectorsShader = "Hidden/Amplify Motion/SolidVectors";
		string skinnedVectorsShader = "Hidden/Amplify Motion/SkinnedVectors";
		string clothVectorsShader = "Hidden/Amplify Motion/ClothVectors";
		string reprojectionVectorsShader = "Hidden/Amplify Motion/ReprojectionVectors";
		string combineShader = "Hidden/Amplify Motion/Combine";
		string dilationShader = "Hidden/Amplify Motion/Dilation";
		string depthShader = "Hidden/Amplify Motion/Depth";
		string debugShader = "Hidden/Amplify Motion/Debug";

		try
		{
			m_blurMaterial = new Material( Shader.Find( blurShader ) ) { hideFlags = HideFlags.DontSave };
			m_solidVectorsMaterial = new Material( Shader.Find( solidVectorsShader ) ) { hideFlags = HideFlags.DontSave };
			m_skinnedVectorsMaterial = new Material( Shader.Find( skinnedVectorsShader ) ) { hideFlags = HideFlags.DontSave };
			m_clothVectorsMaterial = new Material( Shader.Find( clothVectorsShader ) ) { hideFlags = HideFlags.DontSave };
			m_reprojectionMaterial = new Material( Shader.Find( reprojectionVectorsShader ) ) { hideFlags = HideFlags.DontSave };
			m_combineMaterial = new Material( Shader.Find( combineShader ) ) { hideFlags = HideFlags.DontSave };
			m_dilationMaterial = new Material( Shader.Find( dilationShader ) ) { hideFlags = HideFlags.DontSave };
			m_depthMaterial = new Material( Shader.Find( depthShader ) ) { hideFlags = HideFlags.DontSave };
			m_debugMaterial = new Material( Shader.Find( debugShader ) ) { hideFlags = HideFlags.DontSave };
		}
		catch ( Exception )
		{
		}

		// even if we fail, we still need to know which one failed
		bool ok = CheckMaterialAndShader( m_blurMaterial, blurShader );
		ok = ok && CheckMaterialAndShader( m_solidVectorsMaterial, solidVectorsShader );
		ok = ok && CheckMaterialAndShader( m_skinnedVectorsMaterial, skinnedVectorsShader );
		ok = ok && CheckMaterialAndShader( m_clothVectorsMaterial, clothVectorsShader );
		ok = ok && CheckMaterialAndShader( m_reprojectionMaterial, reprojectionVectorsShader );
		ok = ok && CheckMaterialAndShader( m_combineMaterial, combineShader );
		ok = ok && CheckMaterialAndShader( m_dilationMaterial, dilationShader );
		ok = ok && CheckMaterialAndShader( m_depthMaterial, depthShader );
		ok = ok && CheckMaterialAndShader( m_debugMaterial, debugShader );
		return ok;
	}

	RenderTexture CreateRenderTexture( string name, int depth, RenderTextureFormat fmt, RenderTextureReadWrite rw, FilterMode fm )
	{
		RenderTexture rt = new RenderTexture( m_width, m_height, depth, fmt, rw );
		rt.hideFlags = HideFlags.DontSave;
		rt.name = name;
		rt.wrapMode = TextureWrapMode.Clamp;
		rt.filterMode = fm;
		rt.Create();
		return rt;
	}

	void SafeDestroyRenderTexture( ref RenderTexture rt )
	{
		if ( rt != null )
		{
			RenderTexture.active = null;
			rt.Release();
			DestroyImmediate( rt );
			rt = null;
		}
	}

	void SafeDestroyTexture( ref Texture tex )
	{
		if ( tex != null )
		{
			DestroyImmediate( tex );
			tex = null;
		}
	}

	void DestroyRenderTextures()
	{
		RenderTexture.active = null;
		SafeDestroyRenderTexture( ref m_motionRT );
	}

	void UpdateRenderTextures( bool qualityChanged )
	{
		int screenWidth = Mathf.Max( Mathf.FloorToInt( m_camera.pixelWidth + 0.5f ), 1 );
		int screenHeight = Mathf.Max( Mathf.FloorToInt( m_camera.pixelHeight + 0.5f ), 1 );

		if ( QualityLevel == AmplifyMotion.Quality.Mobile )
		{
			screenWidth /= 2;
			screenHeight /= 2;
		}

		if ( m_width != screenWidth || m_height != screenHeight )
		{
			m_width = screenWidth;
			m_height = screenHeight;

			DestroyRenderTextures();
		}

		if ( m_motionRT == null )
		{
			m_motionRT = CreateRenderTexture( "AM-MotionVectors", 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, FilterMode.Point );
		}
	}

	public bool CheckSupport()
	{
#if UNITY_5_5_OR_NEWER
		if ( !SystemInfo.supportsImageEffects )
#else
		if ( !SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures )
#endif
		{
			Debug.LogError( "[AmplifyMotion] Initialization failed. This plugin requires support for Image Effects and Render Textures." );
			return false;
		}
		return true;
	}

	void InitializeThreadPool()
	{
		if ( WorkerThreads <= 0 )
			WorkerThreads = Mathf.Max( Environment.ProcessorCount / 2, 1 ); // half of CPU threads; non-busy idle

		m_workerThreadPool = new AmplifyMotion.WorkerThreadPool();
		m_workerThreadPool.InitializeAsyncUpdateThreads( WorkerThreads, SystemThreadPool );
	}

	void ShutdownThreadPool()
	{
		if ( m_workerThreadPool != null )
		{
			m_workerThreadPool.FinalizeAsyncUpdateThreads();
			m_workerThreadPool = null;
		}
	}

	void InitializeCommandBuffers()
	{
		ShutdownCommandBuffers();

		m_updateCB = new CommandBuffer();
		m_updateCB.name = "AmplifyMotion.Update";
		m_camera.AddCommandBuffer( m_updateCBEvent, m_updateCB );

		m_fixedUpdateCB = new CommandBuffer();
		m_fixedUpdateCB.name = "AmplifyMotion.FixedUpdate";
		m_camera.AddCommandBuffer( m_fixedUpdateCBEvent, m_fixedUpdateCB );
	}

	void ShutdownCommandBuffers()
	{
		if ( m_updateCB != null )
		{
			m_camera.RemoveCommandBuffer( m_updateCBEvent, m_updateCB );
			m_updateCB.Release();
			m_updateCB = null;
		}
		if ( m_fixedUpdateCB != null )
		{
			m_camera.RemoveCommandBuffer( m_fixedUpdateCBEvent, m_fixedUpdateCB );
			m_fixedUpdateCB.Release();
			m_fixedUpdateCB = null;
		}
	}

	void OnEnable()
	{
		m_camera = GetComponent<Camera>();

		if ( !CheckSupport() )
		{
			enabled = false;
			return;
		}

		InitializeThreadPool();

		m_starting = true;

		if ( !CreateMaterials() )
		{
			Debug.LogError( "[AmplifyMotion] Failed loading or compiling necessary shaders. Please try reinstalling Amplify Motion or contact support@amplify.pt" );
			enabled = false;
			return;
		}

		if ( AutoRegisterObjs )
			UpdateActiveObjects();

		InitializeCameras();
		InitializeCommandBuffers();

		UpdateRenderTextures( true );

		m_linkedCameras.TryGetValue( m_camera, out m_baseCamera );

		if ( m_baseCamera == null )
		{
			Debug.LogError( "[AmplifyMotion] Failed setting up Base Camera. Please contact support@amplify.pt" );
			enabled = false;
			return;
		}
		if ( m_currentPostProcess != null )
			m_currentPostProcess.enabled = true;

		m_qualityLevel = QualityLevel;
	}

	void OnDisable()
	{
		if ( m_currentPostProcess != null )
			m_currentPostProcess.enabled = false;

		ShutdownCommandBuffers();
		ShutdownThreadPool();
	}

	void Start()
	{
		UpdatePostProcess();

#if TRIAL
	m_watermark = new Texture2D( 4, 4 );
	m_watermark.LoadImage( AmplifyMotion.Watermark.ImageData );
#endif
	}

	internal void RemoveCamera( Camera reference )
	{
		m_linkedCameras.Remove( reference );
	}

	void OnDestroy()
	{
		AmplifyMotionCamera[] prevLinkedCams = m_linkedCameras.Values.ToArray<AmplifyMotionCamera>();

		foreach ( AmplifyMotionCamera cam in prevLinkedCams )
		{
			if ( cam != null && cam.gameObject != gameObject )
			{
				Camera actual = cam.GetComponent<Camera>();
				if ( actual != null )
					actual.targetTexture = null;
				DestroyImmediate( cam );
			}
		}

		DestroyRenderTextures();
		DestroyMaterials();
#if TRIAL
	DestroyImmediate( m_watermark );
#endif
	}

	GameObject RecursiveFindCamera( GameObject obj, string auxCameraName )
	{
		GameObject cam = null;
		if ( obj.name == auxCameraName )
			cam = obj;
		else
		{
			foreach ( Transform child in obj.transform )
			{
				cam = RecursiveFindCamera( child.gameObject, auxCameraName );
				if ( cam != null )
					break;
			}
		}
		return cam;
	}

	void InitializeCameras()
	{
		List<Camera> cleanOverlayCameras = new List<Camera>( OverlayCameras.Length );
		for ( int i = 0; i < OverlayCameras.Length; i++ )
		{
			if ( OverlayCameras[ i ] != null )
				cleanOverlayCameras.Add( OverlayCameras[ i ] );
		}

		Camera[] referenceCameras = new Camera[ cleanOverlayCameras.Count + 1 ];

		referenceCameras[ 0 ] = m_camera;
		for ( int i = 0; i < cleanOverlayCameras.Count; i++ )
			referenceCameras[ i + 1 ] = cleanOverlayCameras[ i ];

		m_linkedCameras.Clear();

		for ( int i = 0; i < referenceCameras.Length; i++ )
		{
			Camera reference = referenceCameras[ i ];
			if ( !m_linkedCameras.ContainsKey( reference ) )
			{
				AmplifyMotionCamera cam = reference.gameObject.GetComponent<AmplifyMotionCamera>();
				if ( cam != null )
				{
					cam.enabled = false;
					cam.enabled = true;
				}
				else
					cam = reference.gameObject.AddComponent<AmplifyMotionCamera>();

				cam.LinkTo( this, i > 0 );

				m_linkedCameras.Add( reference, cam );
				m_linkedCamerasChanged = true;
			}
		}
	}

	public void UpdateActiveCameras()
	{
		InitializeCameras();
	}

	internal static void RegisterCamera( AmplifyMotionCamera cam )
	{
		if ( !m_activeCameras.ContainsValue( cam ) )
			m_activeCameras.Add( cam.GetComponent<Camera>(), cam );

		foreach ( AmplifyMotionObjectBase obj in m_activeObjects.Values )
			obj.RegisterCamera( cam );
	}

	internal static void UnregisterCamera( AmplifyMotionCamera cam )
	{
		foreach ( AmplifyMotionObjectBase obj in m_activeObjects.Values )
			obj.UnregisterCamera( cam );

		m_activeCameras.Remove( cam.GetComponent<Camera>() );
	}

	public void UpdateActiveObjects()
	{
		GameObject[] gameObjs = FindObjectsOfType( typeof( GameObject ) ) as GameObject[];
		for ( int i = 0; i < gameObjs.Length; i++ )
		{
			if ( !m_activeObjects.ContainsKey( gameObjs[ i ] ) )
				TryRegister( gameObjs[ i ], true );
		}
	}

	internal static void RegisterObject( AmplifyMotionObjectBase obj )
	{
		m_activeObjects.Add( obj.gameObject, obj );
		foreach ( AmplifyMotionCamera cam in m_activeCameras.Values )
			obj.RegisterCamera( cam );
	}

	internal static void UnregisterObject( AmplifyMotionObjectBase obj )
	{
		foreach ( AmplifyMotionCamera cam in m_activeCameras.Values )
			obj.UnregisterCamera( cam );
		m_activeObjects.Remove( obj.gameObject );
	}

	internal static bool FindValidTag( Material[] materials )
	{
		for ( int i = 0; i < materials.Length; i++ )
		{
			Material mat = materials[ i ];
			if ( mat != null )
			{
				string tag = mat.GetTag( "RenderType", false );
				if ( tag == "Opaque" || tag == "TransparentCutout" )
				{
					return !mat.IsKeywordEnabled( "_ALPHABLEND_ON" ) && !mat.IsKeywordEnabled( "_ALPHAPREMULTIPLY_ON" );
				}
			}
		}
		return false;
	}

	internal static bool CanRegister( GameObject gameObj, bool autoReg )
	{
		// Ignore static objects
		if ( gameObj.isStatic )
			return false;

		// Ignore invalid materials; Ignore static batches
		Renderer renderer = gameObj.GetComponent<Renderer>();
		if ( renderer == null || renderer.sharedMaterials == null || renderer.isPartOfStaticBatch )
			return false;

		// Ignore disabled renderer
		if ( !renderer.enabled )
			return false;

		// Ignore if visible only for shadows
		if ( renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly )
		{
			return false;
		}

		if ( renderer.GetType() == typeof( SpriteRenderer ) )
		{
			return false;
		}
		else
		{
			// Ignore unsupported RenderType
			if ( !FindValidTag( renderer.sharedMaterials ) )
				return false;

			// Only valid and supported renderers
			Type type = renderer.GetType();
			if ( type == typeof( MeshRenderer ) || type == typeof( SkinnedMeshRenderer ) )
			{
				return true;
			}

		#if !UNITY_PRE_5_3
			if ( type == typeof( ParticleSystemRenderer ) && !autoReg )
			{
				// Only supported ParticleSystem modes
				ParticleSystemRenderMode mode = ( renderer as ParticleSystemRenderer ).renderMode;
				return ( mode == ParticleSystemRenderMode.Mesh || mode == ParticleSystemRenderMode.Billboard );
			}
		#endif
		}

		return false;
	}

	internal static void TryRegister( GameObject gameObj, bool autoReg )
	{
		if ( CanRegister( gameObj, autoReg ) && gameObj.GetComponent<AmplifyMotionObjectBase>() == null )
		{
			AmplifyMotionObjectBase.ApplyToChildren = false;
			gameObj.AddComponent<AmplifyMotionObjectBase>();
			AmplifyMotionObjectBase.ApplyToChildren = true;
		}
	}

	internal static void TryUnregister( GameObject gameObj )
	{
		AmplifyMotionObjectBase comp = gameObj.GetComponent<AmplifyMotionObjectBase>();
		if ( comp != null )
			Destroy( comp );
	}

	public void Register( GameObject gameObj )
	{
		if ( !m_activeObjects.ContainsKey( gameObj ) )
			TryRegister( gameObj, false );
	}

	public static void RegisterS( GameObject gameObj )
	{
		if ( !m_activeObjects.ContainsKey( gameObj ) )
			TryRegister( gameObj, false );
	}

	public void RegisterRecursively( GameObject gameObj )
	{
		if ( !m_activeObjects.ContainsKey( gameObj ) )
			TryRegister( gameObj, false );

		foreach ( Transform child in gameObj.transform )
			RegisterRecursively( child.gameObject );
	}

	public static void RegisterRecursivelyS( GameObject gameObj )
	{
		if ( !m_activeObjects.ContainsKey( gameObj ) )
			TryRegister( gameObj, false );

		foreach ( Transform child in gameObj.transform )
			RegisterRecursivelyS( child.gameObject );
	}

	public void Unregister( GameObject gameObj )
	{
		if ( m_activeObjects.ContainsKey( gameObj ) )
			TryUnregister( gameObj );
	}

	public static void UnregisterS( GameObject gameObj )
	{
		if ( m_activeObjects.ContainsKey( gameObj ) )
			TryUnregister( gameObj );
	}

	public void UnregisterRecursively( GameObject gameObj )
	{
		if ( m_activeObjects.ContainsKey( gameObj ) )
			TryUnregister( gameObj );

		foreach ( Transform child in gameObj.transform )
			UnregisterRecursively( child.gameObject );
	}

	public static void UnregisterRecursivelyS( GameObject gameObj )
	{
		if ( m_activeObjects.ContainsKey( gameObj ) )
			TryUnregister( gameObj );

		foreach ( Transform child in gameObj.transform )
			UnregisterRecursivelyS( child.gameObject );
	}

	void UpdatePostProcess()
	{
		Camera highestReference = null;
		float highestDepth = -float.MaxValue;

		if ( m_linkedCamerasChanged )
			UpdateLinkedCameras();

		for ( int i = 0; i < m_linkedCameraKeys.Length; i++ )
		{
			if ( m_linkedCameraKeys[ i ] != null && m_linkedCameraKeys[ i ].isActiveAndEnabled && m_linkedCameraKeys[ i ].depth > highestDepth )
			{
				highestReference = m_linkedCameraKeys[ i ];
				highestDepth = m_linkedCameraKeys[ i ].depth;
			}
		}

		if ( m_currentPostProcess != null && m_currentPostProcess.gameObject != highestReference.gameObject )
		{
			DestroyImmediate( m_currentPostProcess );
			m_currentPostProcess = null;
		}

		if ( m_currentPostProcess == null && highestReference != null && highestReference != m_camera )
		{
			AmplifyMotionPostProcess[] runtimes = gameObject.GetComponents<AmplifyMotionPostProcess>();
			if ( runtimes != null && runtimes.Length > 0 )
			{
				for ( int i = 0; i < runtimes.Length; i++ )
					DestroyImmediate( runtimes[ i ] );
			}
			m_currentPostProcess = highestReference.gameObject.AddComponent<AmplifyMotionPostProcess>();
			m_currentPostProcess.Instance = this;
		}
	}

	void LateUpdate()
	{
		if ( m_baseCamera.AutoStep )
		{
			float delta = Application.isPlaying ? Time.unscaledDeltaTime : Time.fixedDeltaTime;
			float fixedDelta = Time.fixedDeltaTime;

			m_deltaTime = ( delta > float.Epsilon ) ? delta : m_deltaTime;
			m_fixedDeltaTime = ( delta > float.Epsilon ) ? fixedDelta : m_fixedDeltaTime;
		}

		QualitySteps = Mathf.Clamp( QualitySteps, 0, 16 );
		MotionScale = Mathf.Max( MotionScale, 0 );
		MinVelocity = Mathf.Min( MinVelocity, MaxVelocity );
		DepthThreshold = Mathf.Max( DepthThreshold, 0 );
		MinResetDeltaDist = Mathf.Max( MinResetDeltaDist, 0 );
		MinResetDeltaDistSqr = MinResetDeltaDist * MinResetDeltaDist;
		ResetFrameDelay = Mathf.Max( ResetFrameDelay, 0 );

		UpdatePostProcess();
	}

	public void StopAutoStep()
	{
		foreach ( AmplifyMotionCamera cam in m_linkedCameras.Values )
			cam.StopAutoStep();
	}

	public void StartAutoStep()
	{
		foreach ( AmplifyMotionCamera cam in m_linkedCameras.Values )
			cam.StartAutoStep();
	}

	public void Step( float delta )
	{
		m_deltaTime = delta;
		m_fixedDeltaTime = delta;
		foreach ( AmplifyMotionCamera cam in m_linkedCameras.Values )
			cam.Step();
	}

	void UpdateLinkedCameras()
	{
		Dictionary<Camera, AmplifyMotionCamera>.KeyCollection keys = m_linkedCameras.Keys;
		Dictionary<Camera, AmplifyMotionCamera>.ValueCollection values = m_linkedCameras.Values;

		if ( m_linkedCameraKeys == null || keys.Count != m_linkedCameraKeys.Length )
			m_linkedCameraKeys = new Camera[ keys.Count ];

		if ( m_linkedCameraValues == null || values.Count != m_linkedCameraValues.Length )
			m_linkedCameraValues = new AmplifyMotionCamera[ values.Count ];

		keys.CopyTo( m_linkedCameraKeys, 0 );
		values.CopyTo( m_linkedCameraValues, 0 );

		m_linkedCamerasChanged = false;
	}

	void FixedUpdate()
	{
		if ( m_camera.enabled )
		{
			if ( m_linkedCamerasChanged )
				UpdateLinkedCameras();

			m_fixedUpdateCB.Clear();

			for ( int i = 0; i < m_linkedCameraValues.Length; i++ )
			{
				if ( m_linkedCameraValues[ i ] != null && m_linkedCameraValues[ i ].isActiveAndEnabled )
				{
					m_linkedCameraValues[ i ].FixedUpdateTransform( this, m_fixedUpdateCB );
				}
			}
		}
	}

	void OnPreRender()
	{
		if ( m_camera.enabled && ( Time.frameCount == 1 || Mathf.Abs( Time.unscaledDeltaTime ) > float.Epsilon ) )
		{
			if ( m_linkedCamerasChanged )
				UpdateLinkedCameras();

			m_updateCB.Clear();

			for ( int i = 0; i < m_linkedCameraValues.Length; i++ )
			{
				if ( m_linkedCameraValues[ i ] != null && m_linkedCameraValues[ i ].isActiveAndEnabled )
				{
					m_linkedCameraValues[ i ].UpdateTransform( this, m_updateCB );
				}
			}
		}
	}

	void OnPostRender()
	{
		bool qualityChanged = ( QualityLevel != m_qualityLevel );
		m_qualityLevel = QualityLevel;

		UpdateRenderTextures( qualityChanged );

		ResetObjectId();

		bool cameraMotion = ( CameraMotionMult > float.Epsilon );
		bool clearColor = !cameraMotion || m_starting;

		float rcpDepthThreshold = ( DepthThreshold > float.Epsilon ) ? 1.0f / DepthThreshold : float.MaxValue;

		m_motionScaleNorm = ( m_deltaTime >= float.Epsilon ) ? MotionScale * ( 1.0f / m_deltaTime ) : 0;
		m_fixedMotionScaleNorm = ( m_fixedDeltaTime >= float.Epsilon ) ? MotionScale * ( 1.0f / m_fixedDeltaTime ) : 0;

		float objectScale = !m_starting ? m_motionScaleNorm : 0;
		float objectFixedScale = !m_starting ? m_fixedMotionScaleNorm : 0;

		Shader.SetGlobalFloat( "_AM_MIN_VELOCITY", MinVelocity );
		Shader.SetGlobalFloat( "_AM_MAX_VELOCITY", MaxVelocity );
		Shader.SetGlobalFloat( "_AM_RCP_TOTAL_VELOCITY", 1.0f / ( MaxVelocity - MinVelocity ) );
		Shader.SetGlobalVector( "_AM_DEPTH_THRESHOLD", new Vector2( DepthThreshold, rcpDepthThreshold ) );

		// pre-render base camera
		m_motionRT.DiscardContents();
		m_baseCamera.PreRenderVectors( m_motionRT, clearColor, rcpDepthThreshold );

		// pre-render overlay cameras
		for ( int i = 0; i < m_linkedCameraValues.Length; i++ )
		{
			AmplifyMotionCamera cam = m_linkedCameraValues[ i ];
			if ( cam != null && cam.Overlay && cam.isActiveAndEnabled )
			{
				cam.PreRenderVectors( m_motionRT, clearColor, rcpDepthThreshold );
				cam.RenderVectors( objectScale, objectFixedScale, QualityLevel );
			}
		}

		if ( cameraMotion )
		{
			float cameraMotionScaleNorm = ( m_deltaTime >= float.Epsilon ) ? MotionScale * CameraMotionMult * ( 1.0f / m_deltaTime ) : 0;
			float cameraScale = !m_starting ? cameraMotionScaleNorm : 0;

			m_motionRT.DiscardContents();
			m_baseCamera.RenderReprojectionVectors( m_motionRT, cameraScale );
		}

		// render base camera
		m_baseCamera.RenderVectors( objectScale, objectFixedScale, QualityLevel );

		// render overlay cameras
		for ( int i = 0; i < m_linkedCameraValues.Length; i++ )
		{
			AmplifyMotionCamera cam = m_linkedCameraValues[ i ];
			if ( cam != null && cam.Overlay && cam.isActiveAndEnabled )
				cam.RenderVectors( objectScale, objectFixedScale, QualityLevel );
		}

		m_starting = false;
	}

	void ApplyMotionBlur( RenderTexture source, RenderTexture destination, Vector4 blurStep )
	{
		bool mobile = ( QualityLevel == AmplifyMotion.Quality.Mobile );
		int pass = ( ( int ) QualityLevel ) + ( Noise ? 4 : 0 );

		RenderTexture depthRT = null;
		if ( mobile )
		{
			depthRT = RenderTexture.GetTemporary( m_width, m_height, 0, RenderTextureFormat.ARGB32 );
			depthRT.name = "AM-DepthTemp";
			depthRT.wrapMode = TextureWrapMode.Clamp;
			depthRT.filterMode = FilterMode.Point;
		}

		RenderTexture combinedRT = RenderTexture.GetTemporary( m_width, m_height, 0, source.format );
		combinedRT.name = "AM-CombinedTemp";
		combinedRT.wrapMode = TextureWrapMode.Clamp;
		combinedRT.filterMode = FilterMode.Point;

		combinedRT.DiscardContents();
		m_combineMaterial.SetTexture( "_MotionTex", m_motionRT );
		source.filterMode = FilterMode.Point;
		Graphics.Blit( source, combinedRT, m_combineMaterial, 0 );

		m_blurMaterial.SetTexture( "_MotionTex", m_motionRT );
		if ( mobile )
		{
			Graphics.Blit( null, depthRT, m_depthMaterial, 0 );
			m_blurMaterial.SetTexture( "_DepthTex", depthRT );
		}

		if ( QualitySteps > 1 )
		{
			RenderTexture temp = RenderTexture.GetTemporary( m_width, m_height, 0, source.format );
			temp.name = "AM-CombinedTemp2";
			temp.filterMode = FilterMode.Point;

			float step = 1.0f / QualitySteps;
			float scale = 1.0f;
			RenderTexture src = combinedRT;
			RenderTexture dst = temp;

			for ( int i = 0; i < QualitySteps; i++ )
			{
				if ( dst != destination )
					dst.DiscardContents();

				m_blurMaterial.SetVector( "_AM_BLUR_STEP", blurStep * scale );
				Graphics.Blit( src, dst, m_blurMaterial, pass );

				if ( i < QualitySteps - 2 )
				{
					RenderTexture tmp = dst;
					dst = src;
					src = tmp;
				}
				else
				{
					src = dst;
					dst = destination;
				}
				scale -= step;
			}

			RenderTexture.ReleaseTemporary( temp );
		}
		else
		{
			m_blurMaterial.SetVector( "_AM_BLUR_STEP", blurStep );
			Graphics.Blit( combinedRT, destination, m_blurMaterial, pass );
		}

		if ( mobile )
		{
			// we need the full res here
			m_combineMaterial.SetTexture( "_MotionTex", m_motionRT );
			Graphics.Blit( source, destination, m_combineMaterial, 1 );
		}

		RenderTexture.ReleaseTemporary( combinedRT );
		if ( depthRT != null )
			RenderTexture.ReleaseTemporary( depthRT );
	}

	void OnRenderImage( RenderTexture source, RenderTexture destination )
	{
		if ( m_currentPostProcess == null )
			PostProcess( source, destination );
		else
			Graphics.Blit( source, destination );
	}

	public void PostProcess( RenderTexture source, RenderTexture destination )
	{
		Vector4 blurStep = Vector4.zero;
		blurStep.x = MaxVelocity / 1000.0f;
		blurStep.y = MaxVelocity / 1000.0f;

		RenderTexture dilatedRT = null;
		if ( QualitySettings.antiAliasing > 1 )
		{
			dilatedRT = RenderTexture.GetTemporary( m_width, m_height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear );
			dilatedRT.name = "AM-DilatedTemp";
			dilatedRT.filterMode = FilterMode.Point;

			m_dilationMaterial.SetTexture( "_MotionTex", m_motionRT );
			Graphics.Blit( m_motionRT, dilatedRT, m_dilationMaterial, 0 );
			m_dilationMaterial.SetTexture( "_MotionTex", dilatedRT );
			Graphics.Blit( dilatedRT, m_motionRT, m_dilationMaterial, 1 );
		}

		if ( DebugMode )
		{
			m_debugMaterial.SetTexture( "_MotionTex", m_motionRT );
			Graphics.Blit( source, destination, m_debugMaterial );
		}
		else
		{
			ApplyMotionBlur( source, destination, blurStep );
		}

		if ( dilatedRT != null )
			RenderTexture.ReleaseTemporary( dilatedRT );
	}

#if TRIAL
	void OnGUI()
	{
		GUI.DrawTexture( new Rect( 15, Screen.height - m_watermark.height - 12, m_watermark.width, m_watermark.height ), m_watermark );
	}
#endif
}
