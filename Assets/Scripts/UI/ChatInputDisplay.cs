using UnityEngine;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.UI
{

    public class ChatInputDisplay : MonoBehaviour
    {
        string m_chatText = "";

        public ScreenCorner screenCorner = ScreenCorner.BottomRight;
        public Vector2 padding = new Vector2(40, 40);
        public float textInputWidth = 200;



        void Start()
        {
            PauseMenu.onGUI += this.OnPauseMenuGUI;
        }

        void OnPauseMenuGUI()
        {

			string buttonText = "Send";
			Vector2 buttonSize = GUIUtils.CalcScreenSizeForText(buttonText, GUI.skin.button);
			Rect rect = GUIUtils.GetCornerRect(this.screenCorner, buttonSize, this.padding);
			if (GUI.Button(rect, buttonText))
			{
				Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer(m_chatText);
				m_chatText = "";
			}

			rect.xMin -= this.textInputWidth;
			rect.xMax -= buttonSize.x + 15;
			m_chatText = GUI.TextField(rect, m_chatText, 100);

        }

    }

}
