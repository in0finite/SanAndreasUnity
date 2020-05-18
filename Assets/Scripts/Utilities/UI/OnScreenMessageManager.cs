using UnityEngine;
using System.Collections.Generic;

namespace SanAndreasUnity.Utilities
{

	public class OnScreenMessageManager : MonoBehaviour
	{

		private List<OnScreenMessage> m_onScreenMessages = new List<OnScreenMessage>();
		public IReadOnlyList<OnScreenMessage> Messages => m_onScreenMessages;

		[SerializeField] private bool m_drawMessages = true;
		public static bool DrawMessages { get { return Instance.m_drawMessages; } set { Instance.m_drawMessages = value; } }

		public static OnScreenMessageManager Instance { get; private set; }



		void Awake()
		{
			Instance = this;
		}

		void Update()
		{

			foreach (var msg in m_onScreenMessages)
			{
				msg.timeLeft -= Time.deltaTime;
				msg.screenPos += msg.velocity * Time.deltaTime;
			}

			m_onScreenMessages.RemoveAll(msg => msg.timeLeft <= 0);

		}

		void OnGUI()
		{
			// draw messages

			if (!m_drawMessages)
				return;

			var originalColor = GUI.color;
			var originalBackgroundColor = GUI.backgroundColor;

			Vector2 screenSize = new Vector2(Screen.width, Screen.height);

			foreach (var msg in m_onScreenMessages)
			{
				GUI.color = msg.color;
				GUI.backgroundColor = msg.backgroundColor;

				Vector2 size = Utilities.GUIUtils.CalcScreenSizeForContent(new GUIContent(msg.text), GUI.skin.label);

				GUI.Label(new Rect(Vector2.Scale(msg.screenPos, screenSize), size), msg.text);
			}

			GUI.color = originalColor;
			GUI.backgroundColor = originalBackgroundColor;

		}


		public void AddMessage(OnScreenMessage msg)
		{
			m_onScreenMessages.Add(msg);
		}

		public void RemoveMessage(OnScreenMessage msg)
		{
			m_onScreenMessages.Remove(msg);
		}

	}

}