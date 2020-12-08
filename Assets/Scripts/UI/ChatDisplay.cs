using SanAndreasUnity.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace SanAndreasUnity.UI
{
    public class ChatDisplay : MonoBehaviour
    {
        public static ChatDisplay Instance { get; private set; }

        public TMP_InputField inputField;
        public TMP_Text text;

        CustomInput customInput;

        static UnityEngine.EventSystems.EventSystem eventSystemComponent;

        Queue<Chat.ChatMessage> m_chatMessages = new Queue<Chat.ChatMessage>();
        public int maxNumChatMessages = 5;

        bool currentlyFading = false;

        public float timeToHideChat = 5f;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            eventSystemComponent = UnityEngine.EventSystems.EventSystem.current;

            Chat.ChatManager.onChatMessage += OnChatMsg;
            inputField.onSubmit.AddListener((s) => SendChatMessage(s));

            customInput = CustomInput.Instance;
        }

        static bool lastFrameIsOpened = false;

        public static bool IsOpened
        {
            get {
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
        }

        void Update()
        {
            if ( customInput != null)
            {
                if (customInput.GetKeyDown(KeyCode.T))
                {
                    inputField.gameObject.SetActive(true);
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

        IEnumerator FadeChat()
        {
            // fade from opaque to transparent
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

        void StartFade()
        {
            if ( currentlyFading == false)
            {
                currentlyFading = true;
                StartCoroutine(FadeChat());
            }
        }

        void SetFadeTimeout()
        {
            if (!this.IsInvoking(nameof(StartFade)))
                this.Invoke(nameof(StartFade), this.timeToHideChat);
        }

        void UnFade()
        {
            currentlyFading = false;
            text.color = new Color(1, 1, 1, 1);
        }

        void SendChatMessage(string msg)
        {
            eventSystemComponent.SetSelectedGameObject(null);
            SetFadeTimeout();

            inputField.text = "";

            inputField.gameObject.SetActive(false);

            if (!this.IsInvoking(nameof(StartFade)))
                this.Invoke(nameof(StartFade), 5f);

            if (string.IsNullOrWhiteSpace(msg))
                return;

            Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(msg);
        }
    }
}