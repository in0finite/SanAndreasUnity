using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace SanAndreasUnity.Utilities {
	
	public class FPSDisplay : MonoBehaviour {

		public static FPSDisplay Instance { get; private set; }

		private static readonly int s_fpsTextureWidth = 75;
		private static readonly int s_fpsTextureHeight = 25;
		private float m_fpsMaximum = 60.0f;
		private Texture2D m_fpsTexture = null;
		private Color32[] m_colors = null;
		private float[] m_fpsHistory = new float[s_fpsTextureWidth];
		private int m_fpsIndex = 0;

		public RawImage fpsImage;
		public Text fpsText;

		public bool updateFPS = true;



		void Awake () {

			if (null == Instance)
				Instance = this;

			m_fpsTexture = new Texture2D(s_fpsTextureWidth, s_fpsTextureHeight, TextureFormat.RGBA32, false, true);

			m_colors = new Color32[m_fpsTexture.width * m_fpsTexture.height];

			this.fpsImage.texture = this.m_fpsTexture;
		}
		
		void Update () {

			if (this.updateFPS)
			{
				UpdateTexture();
				UpdateText();
			}

		}

		void UpdateTexture()
		{

			float fps = 1.0f / Time.unscaledDeltaTime;

			UnityEngine.Profiling.Profiler.BeginSample("Reset texture pixels");
			int numPixels = m_fpsTexture.width * m_fpsTexture.height;
			Color backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.66f); // Half-transparent background for FPS graph
			for (int i = 0; i < numPixels; i++)
				m_colors[i] = backgroundColor;
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Set pixels");
			m_fpsTexture.SetPixels32(m_colors);
			UnityEngine.Profiling.Profiler.EndSample();

			// Append to history storage
			m_fpsHistory[m_fpsIndex] = fps;

			int f = m_fpsIndex;

			if (fps > m_fpsHistory.Average())
				m_fpsMaximum = fps;

			// Draw graph into texture
			UnityEngine.Profiling.Profiler.BeginSample("Set fps history pixels");
			for (int i = m_fpsTexture.width - 1; i >= 0; i--)
			{
				float graphVal = (m_fpsHistory[f] > m_fpsMaximum) ? m_fpsMaximum : m_fpsHistory[f]; //Clamps
				int height = (int)(graphVal * m_fpsTexture.height / (m_fpsMaximum + 0.1f)); //Returns the height of the desired point with a padding of 0.1f units

				float p = m_fpsHistory[f] / m_fpsMaximum,
				r = Mathf.Lerp(1, 1 - p, p),
				g = Mathf.Lerp(p * 2, p, p);

				m_fpsTexture.SetPixel(i, height, new Color(r, g, 0));
				f--;

				if (f < 0)
					f = m_fpsHistory.Length - 1;
			}
			UnityEngine.Profiling.Profiler.EndSample();

			// Next entry in rolling history buffer
			m_fpsIndex++;
			if (m_fpsIndex >= m_fpsHistory.Length)
				m_fpsIndex = 0;

			UnityEngine.Profiling.Profiler.BeginSample("Apply texture");
			m_fpsTexture.Apply(false, false);
			UnityEngine.Profiling.Profiler.EndSample();

		}

		void UpdateText()
		{
			float fps = 1.0f / Time.unscaledDeltaTime;
			string text = string.Format("{0:0.0} fps", fps);
			if (this.fpsText.text != text)
				this.fpsText.text = text;
		}

	}

}
