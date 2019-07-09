using UnityEngine;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.UI
{

    public class NetworkEvents2MessageBox : MonoBehaviour
    {
        bool m_clientWasConnected = false;
        public string titleToDisplayWhenClientDisconnects = "";
        public string messageToDisplayWhenClientDisconnects = "";


        void Start()
        {
            NetManager.Instance.onClientStatusChanged += this.OnClientStatusChanged;
        }

        void OnDisable()
        {
            NetManager.Instance.onClientStatusChanged -= this.OnClientStatusChanged;
        }

        void OnClientStatusChanged()
        {
            if (NetStatus.IsClientConnected())
            {
                m_clientWasConnected = true;
            }
            else
            {
                if (m_clientWasConnected && !NetStatus.IsServer)
                {
                    ShowMsg();
                }
            }
        }

        void ShowMsg()
        {
            MessageBox.Show(this.titleToDisplayWhenClientDisconnects, this.messageToDisplayWhenClientDisconnects, false);
        }

    }

}
