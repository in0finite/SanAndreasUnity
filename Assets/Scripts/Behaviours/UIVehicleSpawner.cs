using UnityEngine;

namespace SanAndreasUnity.Behaviours
{

    public class UIVehicleSpawner : MonoBehaviour
    {
        public KeyCode spawnKey = KeyCode.V;


        private void Update()
        {
            if (Input.GetKeyDown(spawnKey) && GameManager.CanPlayerReadInput())
            {
                if (Ped.Instance != null)
                {
                    Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer("/veh");
                }
            }
        }

    }

}
