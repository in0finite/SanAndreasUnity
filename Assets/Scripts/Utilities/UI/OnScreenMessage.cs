using UnityEngine;
using UnityEngine.UI;

namespace SanAndreasUnity.Utilities
{

    public class OnScreenMessage : MonoBehaviour
    {
		public Text TextComponent { get; private set; }

		public string Text { get => this.TextComponent.text; set => this.TextComponent.text = value; }

		public Color TextColor { get => this.TextComponent.color; set => this.TextComponent.color = value; }

		//public Color backgroundColor = new Color(0, 0, 0, 1);   // transparent

		/// in percentage of screen dimensions
		public Vector2 ScreenPos
		{
			get => Vector2.Scale(new Vector2(this.transform.localPosition.x, this.transform.localPosition.y), new Vector2(1.0f / Screen.width, 1.0f / Screen.height));
			set
			{
				(this.transform as RectTransform).localPosition = new Vector3(value.x * Screen.width, value.y * Screen.height, 0f);
			}
		}

		/// in percentage of screen dimensions
		public Vector2 SizeOnScreen
		{
			get => Vector2.Scale((this.transform as RectTransform).sizeDelta, new Vector2(1.0f / Screen.width, 1.0f / Screen.height));
			set
			{
				(this.transform as RectTransform).sizeDelta = new Vector2(value.x * Screen.width, value.y * Screen.height);
			}
		}

		/// in percentage of screen dimensions
		public Vector2 velocity = Vector2.zero;

		/// how much time it is displayed
		public float timeLeft = 2;



		void Awake()
		{
			this.TextComponent = this.GetComponentInChildren<Text>();
		}
	}

}
