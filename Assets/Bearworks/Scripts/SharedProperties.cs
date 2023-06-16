using UnityEngine;
using System.Collections;

namespace XClouds
{
	public class SharedProperties
	{
		private Camera _camera;
		private RenderTexture _subFrame;
		private RenderTexture _previousFrame;
		public RenderTexture currentFrame { get { return _previousFrame; } }
		private bool _isFirstFrame;

		public Matrix4x4 jitter;
		public Matrix4x4 previousProjection;
		public Matrix4x4 previousRotation;
		public Matrix4x4 projection;
		public Matrix4x4 inverseRotation;
		public Matrix4x4 rotation;

		private int _subFrameNumber;
		public int subFrameNumber { get { return _subFrameNumber; } }

		private int _downsample;
		public int downsample {
			get { return _downsample; }
			set {
				_downsample = value;
			}
		}

		private int _subPixelSize;
		public int subPixelSize { 
			get { return _subPixelSize; }
			set {
				_subPixelSize = value;
				_frameNumbers = CreateFrameNumbers( _subPixelSize);
				_subFrameNumber = 0;
			}
		}

		private bool _dimensionsChangedSinceLastFrame;
		public bool dimensionsChangedSinceLastFrame { get { return _dimensionsChangedSinceLastFrame; } }
		
		private int _subFrameWidth;
		public int subFrameWidth { get { return _subFrameWidth; } }

		private int _subFrameHeight;
		public int subFrameHeight { get { return _subFrameHeight; } }

		private int _frameWidth;
		public int frameWidth { get { return _frameWidth; } }

		private int _frameHeight;
		public int frameHeight { get { return _frameHeight; } }
        
		private int[] _frameNumbers;
		int _renderCount;

		public SharedProperties()
		{
			_renderCount = 0;
			downsample = 2;
			subPixelSize = 2;
		}

		private void CreateRenderTextures()
		{
			if (_subFrame == null && _camera != null)
			{
				RenderTextureFormat format = _camera.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
				_subFrame = new RenderTexture(subFrameWidth,
					subFrameHeight, 0, format, RenderTextureReadWrite.Linear);
				_subFrame.filterMode = FilterMode.Bilinear;
				_subFrame.hideFlags = HideFlags.HideAndDontSave;
				_isFirstFrame = true;
			}

			if (_previousFrame == null && _camera != null)
			{
				RenderTextureFormat format = _camera.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
				_previousFrame = new RenderTexture(frameWidth,
					frameHeight, 0, format, RenderTextureReadWrite.Linear);
				_previousFrame.filterMode = FilterMode.Bilinear;
				_previousFrame.hideFlags = HideFlags.HideAndDontSave;
				_isFirstFrame = true;
			}
		}

		public void DestroyRenderTextures()
		{
			Object.DestroyImmediate(_subFrame);
			_subFrame = null;

			Object.DestroyImmediate(_previousFrame);
			_previousFrame = null;
		}

		public void BeginFrame(Camera camera)
		{
			_camera = camera;
	
			if (_camera == null)
				return;

			UpdateFrameDimensions();

			if (_subFrame == null || _previousFrame == null || dimensionsChangedSinceLastFrame)
			{
				DestroyRenderTextures();
				CreateRenderTextures();
			}

			projection = _camera.projectionMatrix;
			rotation = _camera.worldToCameraMatrix;
			inverseRotation = _camera.cameraToWorldMatrix;
			jitter = CreateJitterMatrix();
		}

		public void EndFrame(Material material, Material combinerMaterial)
		{
			if (_camera == null)
				return;

			_subFrame.DiscardContents();
			Graphics.Blit(null, _subFrame, material);

			if (_isFirstFrame)
			{
				_previousFrame.DiscardContents();
				Graphics.Blit(_subFrame, _previousFrame);
				_isFirstFrame = false;
			}

			combinerMaterial.SetTexture("_SubFrame", _subFrame);
			combinerMaterial.SetTexture("_PrevFrame", _previousFrame);
			ApplyToMaterial(combinerMaterial);

			RenderTextureFormat format = _camera.allowHDR ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
			RenderTexture combined = RenderTexture.GetTemporary(_previousFrame.width, _previousFrame.height, 0, format, RenderTextureReadWrite.Linear);
			combined.filterMode = FilterMode.Bilinear;

			combined.DiscardContents();
			Graphics.Blit(null, combined, combinerMaterial);
			_previousFrame.DiscardContents();
			Graphics.Blit(combined, _previousFrame);

			RenderTexture.ReleaseTemporary(combined);

			previousProjection = projection;
			previousRotation = rotation;
			_dimensionsChangedSinceLastFrame = false;
			_renderCount++;
			_subFrameNumber = _frameNumbers[ _renderCount % (subPixelSize * subPixelSize)];
		}

		public void ApplyToMaterial(Material material, bool jitterProjection=false)
		{
			Matrix4x4 inverseProjection = projection.inverse;
			if( jitterProjection) { inverseProjection *= jitter; }

            material.SetMatrix("_PreviousProjection", previousProjection);
			material.SetMatrix( "_PreviousRotation", previousRotation);
			material.SetMatrix( "_Projection", projection);
			material.SetMatrix( "_InverseProjection", inverseProjection);
			material.SetMatrix( "_InverseRotation", inverseRotation);
			material.SetFloat( "_SubFrameNumber", subFrameNumber);
			material.SetFloat( "_SubPixelSize", subPixelSize);
			material.SetVector( "_SubFrameSize", new Vector2( _subFrameWidth, _subFrameHeight));
			material.SetVector( "_FrameSize", new Vector2( _frameWidth, _frameHeight));
		}

		private void UpdateFrameDimensions()
		{
			int newFrameWidth = _camera.pixelWidth / downsample;
			int newFrameHeight = _camera.pixelHeight / downsample;

			while( (newFrameWidth % _subPixelSize) != 0) { newFrameWidth++; }
			while( (newFrameHeight % _subPixelSize) != 0) { newFrameHeight++; }

			int newSubFrameWidth = newFrameWidth / _subPixelSize;
			int newSubFrameHeight = newFrameHeight / _subPixelSize;

			_dimensionsChangedSinceLastFrame = newFrameWidth != _frameWidth ||
											   newFrameHeight != _frameHeight ||
											   newSubFrameWidth != _subFrameWidth ||
											   newSubFrameHeight != _subFrameHeight;

			_frameWidth = newFrameWidth;
			_frameHeight = newFrameHeight;
			_subFrameWidth = newSubFrameWidth;
			_subFrameHeight = newSubFrameHeight;
		}

		private int[] CreateFrameNumbers( int subPixelSize)
		{
			int frameCount = subPixelSize * subPixelSize;
			int i=0;
			int[] frameNumbers = new int[ frameCount];

			for( i=0; i<frameCount; i++) 
			{ frameNumbers[i] = i; }
			
			while( i-- > 0) 
			{ 
				int k = frameNumbers[ i];
				int j = (int)(Random.value * 1000.0f) % frameCount;
				frameNumbers[i] = frameNumbers[j];
				frameNumbers[j] = k; 
			}

			return frameNumbers;
		}

		private Matrix4x4 CreateJitterMatrix()
		{
			int x = subFrameNumber % subPixelSize;
			int y = subFrameNumber / subPixelSize;
			
			Vector3 jitter = new Vector3( x * 2.0f / _frameWidth, 
			                   			  y * 2.0f / _frameHeight);

			return Matrix4x4.TRS( jitter, Quaternion.identity, Vector3.one);
		}

	}
}