using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace SanAndreasUnity.Utilities {
	
	public class FPSDisplay : MonoBehaviour {

		private static int s_fpsTextureWidth = 75;
		private static int s_fpsTextureHeight = 25;
		private static float s_fpsMaximum = 60.0f;
		private float m_fpsDeltaTime = 0.0f;
		private Texture2D m_fpsTexture = null;
		private Color[] m_colors = null;
		private float[] m_fpsHistory = new float[s_fpsTextureWidth];
		private int m_fpsIndex = 0;

		private static bool s_showFPS = true;

		public RawImage fpsImage;
		public Text fpsText;



		void Awake () {

			m_fpsTexture = new Texture2D(s_fpsTextureWidth, s_fpsTextureHeight, TextureFormat.RGBA32, false, true);

			m_colors = new Color[m_fpsTexture.width * m_fpsTexture.height];

			this.fpsImage.texture = this.m_fpsTexture;
		}
		
		void Update () {

			m_fpsDeltaTime += (Time.unscaledDeltaTime - m_fpsDeltaTime) * 0.1f;

			if (Input.GetKeyDown(KeyCode.F10))
				s_showFPS = !s_showFPS;

			if (s_showFPS)
			{
				UpdateTexture(1.0f / m_fpsDeltaTime);

				UpdateText();
			}
			
		}

		void UpdateTexture(float fps)
		{

			UnityEngine.Profiling.Profiler.BeginSample("Reset texture pixels");
			int numPixels = m_fpsTexture.width * m_fpsTexture.height;
			Color backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.66f); // Half-transparent background for FPS graph
			for (int i = 0; i < numPixels; i++)
				m_colors[i] = backgroundColor;
			UnityEngine.Profiling.Profiler.EndSample();

			UnityEngine.Profiling.Profiler.BeginSample("Set pixels");
			m_fpsTexture.SetPixels(m_colors);
			UnityEngine.Profiling.Profiler.EndSample();

			// Append to history storage
			m_fpsHistory[m_fpsIndex] = fps;

			int f = m_fpsIndex;

			if (fps > m_fpsHistory.Average())
				s_fpsMaximum = fps;

			// Draw graph into texture
			UnityEngine.Profiling.Profiler.BeginSample("Set fps history pixels");
			for (int i = m_fpsTexture.width - 1; i >= 0; i--)
			{
				float graphVal = (m_fpsHistory[f] > s_fpsMaximum) ? s_fpsMaximum : m_fpsHistory[f]; //Clamps
				int height = (int)(graphVal * m_fpsTexture.height / (s_fpsMaximum + 0.1f)); //Returns the height of the desired point with a padding of 0.1f units

				float p = m_fpsHistory[f] / s_fpsMaximum,
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
			float fps = 1.0f / m_fpsDeltaTime;
			string text = string.Format("{0:0.0} fps", fps);
			if (this.fpsText.text != text)
				this.fpsText.text = text;
		}

	}

}
