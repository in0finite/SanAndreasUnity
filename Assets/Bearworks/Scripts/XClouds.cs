using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace XClouds
{
    [ExecuteInEditMode]
	public class XClouds : MonoBehaviour 
	{	
		public enum SubPixelSize {
			Sub1x1,
			Sub2x2,
			Sub4x4,
			Sub8x8,	
		}

		public SubPixelSize subPixelSize = SubPixelSize.Sub2x2;
		public int maxIterations = 128;
		[Range( 1.0f, 8.0f)]
		public int downsample = 2;

		[HeaderAttribute("Lighting")]

		public Gradient cloudSunColor = new Gradient();
		public Gradient cloudBaseColor = new Gradient();
		public Gradient cloudTopColor = new Gradient();

		[Range( 0.0f, 0.5f)]
		public float sunScalar = 1.0f;
		[Range( 0.0f, 0.5f)]
		public float ambientScalar = 1.0f;
		[Range( 0.0f, 1.0f)]
		public float sunRayLength = 0.08f;
		[Range( 0.0f, 1.0f)]
		public float coneRadius = 0.08f;
		[Range( 0.0f, 30.0f)]
		public float density = 1.0f;

		[HeaderAttribute("Animation")]
		[Range( -0.1f, 0.1f)]
		public float animationScale = 0.1f;
		public Vector2 coverageOffsetPerFrame;
		public Vector3 baseOffsetPerFrame = new Vector3( 0.0f, -0.001f, 0.0f);
		public Vector3 detailOffsetPerFrame;

		[HeaderAttribute("Modeling (Base)")]
		public float baseScale = 1.0f;

        public Texture3D _perlin3D;
        public Texture3D _detail3D;
        public Texture2D _curlTexture;

		public Texture cloudCoverage;
		[Range(0.0f, 1.0f)]
		public float coverageOffsetX;
		[Range(0.0f, 1.0f)]
		public float coverageOffsetY;

		[Space(10)]
		[Range( 0.0f, 1.0f)]
		public float sampleScalar = 1.0f;
		[Range( 0.0f, 1.0f)]
		public float sampleThreshold = 0.05f;
		[RangeAttribute(0.0f, 1.0f)]
		public float erosionEdgeSize = 0.5f;
		[Range( 0.0f, 1.0f)]
		public float bottomFade = 0.3f;

		[HeaderAttribute("Modeling (Detail)")]
		public float detailScale = 8.0f;
		[RangeAttribute( 0.1f, 0.5f)]
		public float detailStrength = 0.25f;
		[RangeAttribute( 0.0f, 1.0f)]
		public float cloudDistortion = 0.45f;
		public float cloudDistortionScale = 0.5f;

		[HeaderAttribute("Optimization")]
		[RangeAttribute( 0.0f, 1.0f)]
		public float lodDistance = 0.3f;
		[Range( 0.0f, 1.0f)]
		public float horizonFade = 0.25f;
        [Range(0.0f, 1.0f)]
        public float horizonFadeStartAlpha = 0.9f;
        
        [HeaderAttribute("Atmosphere:")]	
		public float earthRadius = 607583.3f;
		public float atmosphereStartHeight = 1500.0f;
		public float atmosphereEndHeight = 4000.0f;

		
		private Material _cloudMaterial;
		private Material _cloudCombinerMaterial;

        private Vector2 _coverageOffset;
		public Vector2 coverageOffset { get { return _coverageOffset; } }

        private Vector3 _baseOffset;
        private Vector3 _detailOffset;

		private Vector3[] _randomVectors;

		private Dictionary<Camera, SharedProperties> _frameSharedProperties = new Dictionary<Camera, SharedProperties>();

		void OnEnable()
        {
			RenderPipelineManager.beginCameraRendering += ExecuteOnPreCull;
			RenderPipelineManager.endCameraRendering += ExecuteOnEnd;

			CreateMaterialsIfNeeded();
		}
		
		void OnDisable()
		{
			DestroyMaterials();
			foreach (var frameproperty in _frameSharedProperties)
            {
				if (frameproperty.Value != null)
					frameproperty.Value.DestroyRenderTextures();
			}

			_frameSharedProperties.Clear();

			RenderPipelineManager.beginCameraRendering -= ExecuteOnPreCull;
			RenderPipelineManager.endCameraRendering -= ExecuteOnEnd;
		}

		void OnValidate()
		{
			UpdateSharedFromPublicProperties();
		}
		
		void Start () 
		{
			UpdateSharedFromPublicProperties();
			CreateMaterialsIfNeeded();
        }

        void Update()
        {
			if (animationScale != 0.0f)
			{
				float animation = animationScale * Time.smoothDeltaTime;
				_coverageOffset += coverageOffsetPerFrame * animation;
				_baseOffset += baseOffsetPerFrame * animation;
				_detailOffset += detailOffsetPerFrame * animation;
			}

			CreateMaterialsIfNeeded();
			UpdateMaterialsPublicProperties();
		}

		private bool _isFirstFrame;

		void ExecuteOnPreCull(
			ScriptableRenderContext context,
			Camera camera)
		{

			if (camera.cameraType == CameraType.Preview)
				return;

			if (camera.cameraType == CameraType.Reflection)
				return;

			if (RenderSettings.skybox)
			{
				if (_isFirstFrame)
                {
                    //warn up
                    for (int i = 0; i < 4; i++)
						RenderClouds(camera, RenderSettings.skybox);

					_isFirstFrame = false;
				}

				RenderClouds(camera, RenderSettings.skybox);
			}
        }

		void ExecuteOnEnd(ScriptableRenderContext context, Camera camera)
		{
			if (camera.cameraType == CameraType.Preview)
				return;

			if (camera.cameraType == CameraType.Reflection)
				return;

			if (RenderSettings.skybox)
			{
				RenderSettings.skybox.SetTexture("_CloudTex", Texture2D.blackTexture);
			}
		}

		private float _maxDistance;
		private float CalculateHorizonDistance(float innerRadius, float outerRadius)
		{
			return Mathf.Sqrt((outerRadius * outerRadius) - (innerRadius * innerRadius));
		}
		
		private float CalculateMaxDistance()
		{
			return CalculateHorizonDistance(earthRadius, earthRadius + atmosphereEndHeight);
		}

		public void UpdateSharedFromPublicProperties()
		{
			_isFirstFrame = true;
			_coverageOffset.Set(coverageOffsetX, coverageOffsetY);

			foreach (var frameproperty in _frameSharedProperties)
			{
				frameproperty.Value.subPixelSize = SubPixelSizeToInt(subPixelSize);
				frameproperty.Value.downsample = downsample;
			}
		}
		
		private int SubPixelSizeToInt( SubPixelSize size)
		{
			int value = 2;

			switch( size)
			{
				case SubPixelSize.Sub1x1: value = 1; break;
				case SubPixelSize.Sub2x2: value = 2; break;
				case SubPixelSize.Sub4x4: value = 4; break;
				case SubPixelSize.Sub8x8: value = 8; break;
			}

			return value;
		}

        public void RenderClouds(Camera camera, Material skybox)
		{	
			SharedProperties frameproperty = null;
			if(!_frameSharedProperties.TryGetValue(camera, out frameproperty))
            {
				frameproperty = new SharedProperties();
				frameproperty.subPixelSize = SubPixelSizeToInt(subPixelSize);
				frameproperty.downsample = downsample;
				_frameSharedProperties.Add(camera, frameproperty);
			}

			if (frameproperty != null && skybox != null)
			{
				frameproperty.BeginFrame(camera);
				frameproperty.ApplyToMaterial(_cloudMaterial, true);
				frameproperty.EndFrame(_cloudMaterial, _cloudCombinerMaterial);
				skybox.SetTexture("_CloudTex", frameproperty.currentFrame);
			}
		}
        
        private void CreateMaterialsIfNeeded()
		{
			if( _randomVectors == null || _randomVectors.Length < 1)
			{
				Random.InitState(0);
				_randomVectors = new Vector3[] { Random.onUnitSphere,
					Random.onUnitSphere,
					Random.onUnitSphere,
					Random.onUnitSphere,
					Random.onUnitSphere,
					Random.onUnitSphere};
			}

			if( _cloudMaterial == null)
			{
				_cloudMaterial = new Material( Shader.Find( "Hidden/XVolumeClouds"));
				_cloudMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			
			if( _cloudCombinerMaterial == null)
			{
				_cloudCombinerMaterial = new Material( Shader.Find( "Hidden/XCloudCombiner"));
				_cloudCombinerMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
		}

		private void DestroyMaterials()
		{
			DestroyImmediate( _cloudMaterial);
			_cloudMaterial = null;
			
			DestroyImmediate( _cloudCombinerMaterial);
			_cloudCombinerMaterial = null;
		}

        private void UpdateMaterialsPublicProperties()
		{
			if( _cloudMaterial)
			{
				_maxDistance = CalculateMaxDistance();

				Vector3 cameraPosition = Vector3.up * earthRadius;
				float localBaseScale = 1.0f / atmosphereEndHeight * baseScale;

				_cloudMaterial.SetFloat( "_CloudBottomFade", bottomFade);

				_cloudMaterial.SetVector("_CameraPosition", cameraPosition);
				_cloudMaterial.SetFloat("_MaxDistance", _maxDistance);
				_cloudMaterial.SetFloat( "_MaxIterations", maxIterations);
				_cloudMaterial.SetFloat( "_SampleScalar", sampleScalar);
				_cloudMaterial.SetFloat( "_SampleThreshold", sampleThreshold);
				_cloudMaterial.SetFloat( "_LODDistance", lodDistance);
                _cloudMaterial.SetFloat( "_DetailScale", localBaseScale * detailScale);
                _cloudMaterial.SetFloat( "_DetailStrength", detailStrength);

				_cloudMaterial.SetFloat( "_ErosionEdgeSize", erosionEdgeSize);
				_cloudMaterial.SetFloat( "_CloudDistortion", cloudDistortion);
				_cloudMaterial.SetFloat( "_CloudDistortionScale", localBaseScale * cloudDistortionScale);
				_cloudMaterial.SetFloat( "_HorizonFadeScalar", horizonFade);
                _cloudMaterial.SetFloat("_HorizonFadeStartAlpha", horizonFadeStartAlpha);
                _cloudMaterial.SetTexture( "_Perlin3D", _perlin3D);
				_cloudMaterial.SetTexture( "_Detail3D", _detail3D);
				_cloudMaterial.SetVector( "_BaseOffset", _baseOffset);
				_cloudMaterial.SetVector( "_DetailOffset", _detailOffset);
				_cloudMaterial.SetFloat( "_BaseScale", localBaseScale);
				_cloudMaterial.SetFloat( "_LightScalar", sunScalar);
				_cloudMaterial.SetFloat( "_AmbientScalar", ambientScalar);
				_cloudMaterial.SetTexture( "_Coverage", cloudCoverage);

				float ForwardY = 0.75f;
				float sunIntensity = 1f;
				if (RenderSettings.sun != null)
				{
					Vector3 Forward = (RenderSettings.sun.transform.rotation * Vector3.forward).normalized;
					ForwardY = -Forward.y;
					ForwardY = Mathf.Max(ForwardY * 0.5f + 0.5f, 0f);
					sunIntensity = RenderSettings.sun.intensity;
				}

				_cloudMaterial.SetColor( "_CloudBaseColor", cloudBaseColor.Evaluate(ForwardY));
				_cloudMaterial.SetColor( "_CloudTopColor", cloudTopColor.Evaluate(ForwardY));
				_cloudMaterial.SetColor("_SunLightColor", sunIntensity * cloudSunColor.Evaluate(ForwardY));

				float atmosphereThickness = atmosphereEndHeight - atmosphereStartHeight;
				_cloudMaterial.SetFloat("_StartHeight", atmosphereStartHeight + earthRadius);
				_cloudMaterial.SetFloat("_AtmosphereThickness", atmosphereThickness);

				_cloudMaterial.SetFloat( "_Density", density);

                _cloudMaterial.SetFloat( "_SunRayLength", sunRayLength * atmosphereThickness);
				_cloudMaterial.SetFloat( "_ConeRadius", coneRadius * atmosphereThickness);
                _cloudMaterial.SetFloat( "_RayStepLength", atmosphereThickness / Mathf.Floor(maxIterations / 2.0f));

                _cloudMaterial.SetTexture( "_Curl2D", _curlTexture);
				_cloudMaterial.SetFloat( "_CoverageScale", 1.0f / _maxDistance);
				_cloudMaterial.SetVector( "_CoverageOffset", _coverageOffset);
                
				_cloudMaterial.SetVector( "_Random0", _randomVectors[0]);
				_cloudMaterial.SetVector( "_Random1", _randomVectors[1]);
				_cloudMaterial.SetVector( "_Random2", _randomVectors[2]);
				_cloudMaterial.SetVector( "_Random3", _randomVectors[3]);
				_cloudMaterial.SetVector( "_Random4", _randomVectors[4]);
				_cloudMaterial.SetVector( "_Random5", _randomVectors[5]);
			}
		}

	}
}