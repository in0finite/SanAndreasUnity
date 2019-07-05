using System.Collections.Generic;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;
using System.Linq;

namespace SanAndreasUnity.Net
{

    public class PlayerRequests : NetworkBehaviour
    {
        Player m_player;
        public static PlayerRequests Local { get; private set; }

        float m_timeWhenSpawnedVehicle = 0f;
        float m_timeWhenChangedPedModel = 0f;



        void Awake()
        {
            m_player = this.GetComponentOrThrow<Player>();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Local = this;
        }


        public bool CanPlayerSpawnVehicle()
        {
            if (null == m_player.OwnedPed)
                return false;

            if (Vehicle.NumVehicles > Ped.NumPeds * 2)
                return false;

            if (Time.time - m_timeWhenSpawnedVehicle < 3f)
                return false;

            return true;
        }

        public bool CanChangePedModel()
        {
            if (null == m_player.OwnedPed)
                return false;
            if (Time.time - m_timeWhenChangedPedModel < 3f)
                return false;
            return true;
        }

        public void RequestVehicleSpawn()
        {
            this.CmdRequestVehicleSpawn();
        }

        [Command]
        void CmdRequestVehicleSpawn()
        {
            if (!this.CanPlayerSpawnVehicle())
                return;

            m_timeWhenSpawnedVehicle = Time.time;
            F.RunExceptionSafe( () => FindObjectOfType<UIVehicleSpawner> ().SpawnVehicle(m_player.OwnedPed) );
        }

        public void RequestPedModelChange()
        {
            this.CmdRequestPedModelChange();
        }

        [Command]
        void CmdRequestPedModelChange()
        {
            if (!this.CanChangePedModel())
                return;

            m_timeWhenChangedPedModel = Time.time;
            F.RunExceptionSafe( () => m_player.OwnedPed.PlayerModel.Load(Ped.RandomPedId) );
        }

        public void RequestSuicide()
        {
            this.CmdRequestSuicide();
        }

        [Command]
        void CmdRequestSuicide()
        {
            if (m_player.OwnedPed != null)
                Destroy(m_player.OwnedPed.gameObject);
        }

        public void RequestToDestroyAllVehicles()
        {
            this.CmdRequestToDestroyAllVehicles();
        }

        [Command]
        void CmdRequestToDestroyAllVehicles()
        {
            foreach (var v in Vehicle.AllVehicles.ToArray())
            {
                Destroy(v.gameObject);
            }
        }

        public void RequestTeleport(Vector3 pos, Quaternion rot)
        {
            this.CmdRequestTeleport(pos, rot);
        }

        [Command]
        void CmdRequestTeleport(Vector3 pos, Quaternion rot)
        {
            if (m_player.OwnedPed != null)
            {
                m_player.OwnedPed.Teleport(pos, rot);
            }
        }

    }

}
