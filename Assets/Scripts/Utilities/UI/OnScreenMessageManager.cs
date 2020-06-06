using UnityEngine;
using System.Collections.Generic;

namespace SanAndreasUnity.Utilities
{

	public class OnScreenMessageManager : MonoBehaviour
	{

		public GameObject messagePrefab;

		public RectTransform messagesContainer;

		private readonly List<OnScreenMessage> m_onScreenMessages = new List<OnScreenMessage>();
		public IReadOnlyList<OnScreenMessage> Messages => m_onScreenMessages;

		public int messagePoolSize = 10;

		public static OnScreenMessageManager Instance { get; private set; }



		void Awake()
		{
			Instance = this;
		}

		void Update()
		{

			bool hasDeadMessages = false;

			for (int i = 0; i < m_onScreenMessages.Count; i++)
			{
				var msg = m_onScreenMessages[i];

				if (null == msg)
				{
					hasDeadMessages = true;
					continue;
				}

				if (msg.velocity != Vector2.zero)
					msg.ScreenPos += msg.velocity * Time.deltaTime;

				msg.timeLeft -= Time.deltaTime;
				if (msg.timeLeft <= 0)
				{
					Object.Destroy(msg.gameObject);
				}
			}

			if (hasDeadMessages)
			{
				// only remove dead objects if there are any, because this method allocates memory
				m_onScreenMessages.RemoveDeadObjects();
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