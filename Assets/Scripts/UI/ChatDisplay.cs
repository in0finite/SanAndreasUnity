using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.UI
{

    public class ChatDisplay : MonoBehaviour
    {

        Queue<Chat.ChatMessage> m_chatMessages = new Queue<Chat.ChatMessage>();

        public int maxNumChatMessages = 5;



        void Start()
        {
            Chat.ChatManager.onChatMessage += OnChatMsg;
            Behaviours.UIManager.onGUI += OnGUICustom;
        }

        void OnChatMsg(Chat.ChatMessage chatMsg)
		{
			if (m_chatMessages.Count >= this.maxNumChatMessages)
				m_chatMessages.Dequeue();
			
			m_chatMessages.Enqueue(chatMsg);
		}

        void OnGUICustom()
        {

            if (! Behaviours.GameManager.IsInStartupScene)
                DrawChat();

        }

        void DrawChat()
		{
			if (m_chatMessages.Count < 1)
				return;

			float width = Screen.width * 0.25f;
			float height = Screen.height * 0.33f;
			Rect rect = GUIUtils.GetCornerRect(ScreenCorner.BottomLeft, new Vector2(width, height), Vector2.one * 50);

			GUILayout.BeginArea(rect);

			foreach (var chatMsg in m_chatMessages)
			{
				GUILayout.Label("<color=blue>" + chatMsg.sender + "</color> : " + chatMsg.msg);
			}

			GUILayout.EndArea();

		}

    }

}
