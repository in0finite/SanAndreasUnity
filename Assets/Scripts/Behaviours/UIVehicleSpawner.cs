using UGameCore.Utilities;
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
                    if (NetUtils.IsServer)
                        Vehicles.Vehicle.CreateRandomInFrontOf(Ped.Instance.transform);
                    else
                        Chat.ChatManager.SendChatMessageToAllPlayersAsLocalPlayer("/veh");
                }
            }
        }

    }

}
