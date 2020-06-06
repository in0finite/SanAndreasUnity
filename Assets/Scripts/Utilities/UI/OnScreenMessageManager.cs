using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SanAndreasUnity.Utilities
{

	public class OnScreenMessageManager : MonoBehaviour
	{

		public GameObject messagePrefab;

		public RectTransform messagesContainer;

		private readonly List<OnScreenMessage> m_onScreenMessages = new List<OnScreenMessage>();
		public IReadOnlyList<OnScreenMessage> Messages => m_onScreenMessages;

		public int messagePoolSize = 10;

		private readonly Queue<OnScreenMessage> m_pooledOnScreenMessages = new Queue<OnScreenMessage>();
		public int NumPooledMessages => m_pooledOnScreenMessages.Count;

		public static OnScreenMessageManager Instance { get; private set; }



		void Awake()
		{
			Instance = this;
		}

		void Update()
		{

			bool hasMessagesToRemove = false;

			for (int i = 0; i < m_onScreenMessages.Count; i++)
			{
				var msg = m_onScreenMessages[i];

				if (null == msg || !msg.gameObject.activeSelf)
				{
					hasMessagesToRemove = true;
					continue;
				}

				if (msg.velocity != Vector2.zero)
					msg.ScreenPos += msg.velocity * Time.deltaTime;

				msg.timeLeft -= Time.deltaTime;
				if (msg.timeLeft <= 0)
				{
					// try to pool this message
					if (m_pooledOnScreenMessages.Count >= this.messagePoolSize)
					{
						// can't pool => destroy it
						Object.Destroy(msg.gameObject);
					}
					else
					{
						hasMessagesToRemove = true;
						msg.gameObject.SetActive(false);
						m_pooledOnScreenMessages.Enqueue(msg);
					}
				}
			}

			if (hasMessagesToRemove)
			{
				// only remove dead objects if there are any, because this method allocates memory
				m_onScreenMessages.RemoveAll(msg => null == msg || !msg.gameObject.activeSelf);
			}

		}

		public OnScreenMessage CreateMessage()
		{
			var originalMessage = messagePrefab.GetComponentOrThrow<OnScreenMessage>();

			if (m_pooledOnScreenMessages.Count > 0)
			{
				var originalTextComponent = originalMessage.GetComponentInChildren<Text>();
				OnScreenMessage pooledMessage = m_pooledOnScreenMessages.Dequeue();
				pooledMessage.gameObject.SetActive(true);
				// reset parameters
				pooledMessage.ScreenPos = originalMessage.ScreenPos;
				pooledMessage.SizeOnScreen = originalMessage.SizeOnScreen;
				pooledMessage.Text = originalTextComponent.text;
				pooledMessage.TextColor = originalTextComponent.color;
				pooledMessage.velocity = originalMessage.velocity;
				pooledMessage.timeLeft = originalMessage.timeLeft;

				m_onScreenMessages.Add(pooledMessage);
				return pooledMessage;
			}

			GameObject messageGo = Object.Instantiate(messagePrefab);
			messageGo.transform.SetParent(messagesContainer, false);

			OnScreenMessage message = messageGo.GetComponent<OnScreenMessage>();

			m_onScreenMessages.Add(message);

			return message;
		}

	}

}