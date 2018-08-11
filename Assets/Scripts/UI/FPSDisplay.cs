using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SanAndreasUnity.Utilities {
	
	public class FPSDisplay : MonoBehaviour {

        public Gradient gradient;

		private static int fpsTextureWidth = 75;
		private static int fpsTextureHeight = 25;
		private static float fpsMaximum = 60.0f;
		private float fpsDeltaTime = 0.0f;
		private Texture2D fpsTexture = null;
		private float[] fpsHistory = new float[fpsTextureWidth];
		private int fpsIndex = 0;

		private static bool _showFPS = true;

        private const float updateEvery = .3f;
        private float fpsRead, msRead, fpsTimer;

		void Awake () {

			fpsTexture = new Texture2D(fpsTextureWidth, fpsTextureHeight, TextureFormat.RGBA32, false, true);

		}
		
		void Update () {

			// FPS counting
			fpsDeltaTime += (Time.deltaTime - fpsDeltaTime) * 0.1f;

			if (Input.GetKeyDown(KeyCode.F10))
				_showFPS = !_showFPS;
			
		}

		void OnGUI() {



			if (_showFPS)
			{
				float msec = fpsDeltaTime * 1000.0f,
                      fps = 1.0f / fpsDeltaTime;

                if(Time.time - fpsTimer > updateEvery)
                {
                    fpsRead = fps;
                    msRead = msec;
                    fpsTimer = Time.time;
                }

				// Show FPS counter
				GUILayout.BeginArea(GUIUtils.GetCornerRect(ScreenCorner.BottomRight, 100, 25, new Vector2(15 + fpsTexture.width, 10)));
				GUILayout.Label(string.Format("{0:0.}fps ({1:0.0}ms)", fpsRead, msRead), new GUIStyle("label") { alignment = TextAnchor.MiddleLeft });
				GUILayout.EndArea();

				if (fpsTexture == null) return;

				// Show FPS history
				Color[] colors = new Color[fpsTexture.width * fpsTexture.height];

				for (int i = 0; i < (fpsTexture.width * fpsTexture.height); i++)
					colors[i] = new Color(0.0f, 0.0f, 0.0f, 0.66f); // Half-transparent background for FPS graph

				fpsTexture.SetPixels(colors);

				// Append to history storage
				fpsHistory[fpsIndex] = fps;

				int f = fpsIndex;

				if (fps > fpsHistory.Average())
					fpsMaximum = fps;

				// Draw graph into texture
				for (int i = fpsTexture.width - 1; i >= 0; i--)
				{
					float graphVal = (fpsHistory[f] > fpsMaximum) ? fpsMaximum : fpsHistory[f]; //Clamps
					int height = (int)(graphVal * fpsTexture.height / (fpsMaximum + 0.1f)); //Returns the height of the desired point with a padding of 0.1f units

                    float p = fpsHistory[f] / fpsMaximum;

					fpsTexture.SetPixel(i, height, gradient.Evaluate(p));
					f--;

					if (f < 0)
						f = fpsHistory.Length - 1;
				}

				// Next entry in rolling history buffer
				fpsIndex++;
				if (fpsIndex >= fpsHistory.Length)
					fpsIndex = 0;

				// Draw texture on GUI
				fpsTexture.Apply(false, false);
				GUI.DrawTexture(GUIUtils.GetCornerRect(ScreenCorner.BottomRight, fpsTexture.width, fpsTexture.height, new Vector2(5, fpsTexture.height - 15)), fpsTexture);
			}

		}

	}

}
