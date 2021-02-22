using UnityEngine;

namespace SanAndreasUnity.Behaviours
{

    public class CharacterModelChanger : MonoBehaviour
    {
        public KeyCode actionKey = KeyCode.P;


        private void Update()
        {
            if (Input.GetKeyDown(actionKey) && GameManager.CanPlayerReadInput())
            {
                if (Ped.Instance != null)
                {
                    Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer("/skin");
                }
            }
        }
    }

}
