using SanAndreasUnity.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace SanAndreasUnity.UI
{
    public class ChatDisplay : MonoBehaviour
    {
        public static ChatDisplay Instance = null;

        public TMP_InputField inputField;
        public TMP_Text text;

        CustomInput customInput;

        static UnityEngine.EventSystems.EventSystem eventSystemComponent;

        Queue<Chat.ChatMessage> m_chatMessages = new Queue<Chat.ChatMessage>();
        public int maxNumChatMessages = 5;

        bool currentlyFading = false;

        public float timeToHideChat = 5f;

        void Start()
        {
            GameObject eventSystem = GameObject.Find("EventSystem");
            eventSystemComponent = eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>();

            Chat.ChatManager.onChatMessage += OnChatMsg;
            inputField.onSubmit.AddListener((s) => SendChatMessage(s));

            customInput = CustomInput.Instance;

            Instance = this;
        }

        static bool lastFrameIsOpened = false;

        public static bool IsOpened()
        {
            if (Instance != null)
            {
                bool condition = eventSystemComponent.currentSelectedGameObject == Instance.inputField.gameObject;
                bool output = condition || lastFrameIsOpened;
                lastFrameIsOpened = condition;
                return output;
            }
            else
            {
                return false;
            }
        }

        void Update()
        {
            if ( customInput != null)
            {
                if (customInput.GetKeyDown(KeyCode.T))
                {
                    inputField.Select();
                    UnFade();
                }
            }
        }

        string GetDisplayTextForChatMessage(Chat.ChatMessage chatMessage)
        {
            return "<color=blue>" + chatMessage.sender + "</color> : " + chatMessage.msg;
        }

        void OnChatMsg(Chat.ChatMessage chatMsg)
        {
            // text.text = text.text + GetDisplayTextForChatMessage(chatMsg) + "\n";
            AddMessage(chatMsg);
            inputField.text = "";
        }

        void AddMessage(Chat.ChatMessage chatMsg)
        {
            if (m_chatMessages.Count >= this.maxNumChatMessages)
                m_chatMessages.Dequeue();

            m_chatMessages.Enqueue(chatMsg);

            this.UpdateUI();
        }

        void UpdateUI()
        {
            Chat.ChatMessage[] chatMessages = m_chatMessages.ToArray();

            string textStr = "";

            for ( int i = 0; i < chatMessages.Length; i++)
            {
                textStr += GetDisplayTextForChatMessage(chatMessages[i]) + "\n";
            }

            text.text = textStr;

            UnFade();
            SetFadeTimeout();
        }

        IEnumerator FadeChat(bool fadeAway)
        {
            // fade from opaque to transparent
            if (fadeAway)
            {
                // loop over 1 second backwards
                for (float i = 1; i >= 0; i -= Time.deltaTime)
                {
                    if (!currentlyFading)
                        yield break;

                    // set color with i as alpha
                    text.color = new Color(1, 1, 1, i);
                    yield return null;
                }
            }
            // fade from transparent to opaque
            else
            {
                // loop over 1 second
                for (float i = 0; i <= 1; i += Time.deltaTime)
                {
                    // set color with i as alpha
                    if (!currentlyFading)
                        yield break;

                    // set color with i as alpha
                    text.color = new Color(1, 1, 1, i);
                    yield return null;
                }
            }
        }

        void StartFade()
        {
            if ( currentlyFading == false)
            {
                currentlyFading = true;
                StartCoroutine(FadeChat(true));
            }
        }

        void UnFade()
        {
            currentlyFading = false;
            text.color = new Color(1, 1, 1, 1);
        }

        void SetFadeTimeout()
        {
            if (!this.IsInvoking(nameof(StartFade)))
                this.Invoke(nameof(StartFade), this.timeToHideChat);
        }

        void SendChatMessage(string msg)
        {
            eventSystemComponent.SetSelectedGameObject(null);
            SetFadeTimeout();

            inputField.text = "";

            if (!this.IsInvoking(nameof(StartFade)))
                this.Invoke(nameof(StartFade), 5f);

            if (string.IsNullOrWhiteSpace(msg))
                return;

            Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(msg);
        }

        // Fading with coroutines
        // https://forum.unity.com/threads/simple-ui-animation-fade-in-fade-out-c.439825/
    }
}

/*
using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using UnityEngine.UI;
using System.Linq;

namespace SanAndreasUnity.UI
{

    public class ChatDisplay : MonoBehaviour
    {

        Queue<Chat.ChatMessage> m_chatMessages = new Queue<Chat.ChatMessage>();

        public int maxNumChatMessages = 5;
        public float timeToRemoveMessage = 3f;



        void Start()
        {
            Chat.ChatManager.onChatMessage += OnChatMsg;
        }

        void OnChatMsg(Chat.ChatMessage chatMsg)
		{
			if (m_chatMessages.Count >= this.maxNumChatMessages)
				m_chatMessages.Dequeue();
			
			m_chatMessages.Enqueue(chatMsg);

            if (!this.IsInvoking(nameof(RemoveMessage)))
                this.Invoke(nameof(RemoveMessage), this.timeToRemoveMessage);

            this.UpdateUI();
		}

        void RemoveMessage()
        {
            if (m_chatMessages.Count > 0)
                m_chatMessages.Dequeue();
            
            // invoke again if there are more messages
            if (m_chatMessages.Count > 0)
                this.Invoke(nameof(RemoveMessage), this.timeToRemoveMessage);

            this.UpdateUI();
        }

        void UpdateUI()
        {
            Text[] texts = this.gameObject.GetFirstLevelChildrenComponents<Text>().ToArray();
            Chat.ChatMessage[] chatMessages = m_chatMessages.ToArray();

            for (int i = 0; i < texts.Length; i++)
            {
                if (i < chatMessages.Length)
                    texts[i].text = GetDisplayTextForChatMessage(chatMessages[i]);
                else
                    texts[i].text = "";
            }

        }

        string GetDisplayTextForChatMessage(Chat.ChatMessage chatMessage)
        {
            return "<color=blue>" + chatMessage.sender + "</color> : " + chatMessage.msg;
        }

    }

}
*/
