using UnityEngine;
using System.Collections.Generic;

namespace SanAndreasUnity.Utilities
{

	public class OnScreenMessageManager : MonoBehaviour
	{

		public GameObject messagePrefab;

		public RectTransform messagesContainer;

		private List<OnScreenMessage> m_onScreenMessages = new List<OnScreenMessage>();
		public IReadOnlyList<OnScreenMessage> Messages => m_onScreenMessages;

		public int messagePoolSize = 10;

		public static OnScreenMessageManager Instance { get; private set; }



		void Awake()
		{
			Instance = this;
		}

		void Update()
		{

			m_onScreenMessages.RemoveDeadObjects();

			foreach (var msg in m_onScreenMessages)
			{
				msg.timeLeft -= Time.deltaTime;
				msg.ScreenPos += msg.velocity * Time.deltaTime;
				if (msg.timeLeft <= 0)
				{
					Object.Destroy(msg.gameObject);
				}
			}

		}

		public OnScreenMessage CreateMessage()
		{
			messagePrefab.GetComponentOrThrow<OnScreenMessage>();

			GameObject messageGo = Object.Instantiate(messagePrefab);
			messageGo.transform.SetParent(messagesContainer, false);

			OnScreenMessage message = messageGo.GetComponent<OnScreenMessage>();

			m_onScreenMessages.Add(message);

			return message;
		}

	}

}