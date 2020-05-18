using UnityEngine;

namespace SanAndreasUnity.Utilities
{

    public class OnScreenMessage
    {
		public string text = "";
		public Color color = Color.black;
		public Color backgroundColor = new Color(0, 0, 0, 1);   // transparent
		/// in percentage of screen dimensions
		public Vector2 screenPos = Vector2.one / 2f;
		/// in percentage of screen dimensions
		public Vector2 sizeOnScreen = new Vector2(80 / 1280f, 30 / 720f);
		/// in percentage of screen dimensions
		public Vector2 velocity = Vector2.zero;
		/// how much time it is displayed
		public float timeLeft = 2;
	}

}
