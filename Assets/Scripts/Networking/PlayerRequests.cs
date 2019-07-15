using UnityEngine;
using Mirror;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;
using System.Linq;
using System.Collections.Generic;

namespace SanAndreasUnity.Net
{

    public class PlayerRequests : NetworkBehaviour
    {
        Player m_player;
        Ped m_ped => m_player.OwnedPed;
        public static PlayerRequests Local { get; private set; }

        float m_timeWhenSpawnedVehicle = 0f;
        float m_timeWhenMadePedRequest = 0f;
        float m_timeWhenMadeWeaponRequest = 0f;

        List<Vehicle> m_myVehicles = new List<Vehicle>();



        void Awake()
        {
            m_player = this.GetComponentOrThrow<Player>();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Local = this;
        }

        void OnDisable()
        {
            // destroy player's vehicles
            if (NetStatus.IsServer)
            {
                this.DestroyPlayersVehicles();
            }
        }


        public bool CanPlayerSpawnVehicle()
        {
            if (null == m_player.OwnedPed)
                return false;

            if (Time.time - m_timeWhenSpawnedVehicle < 3f)
                return false;

            m_myVehicles.RemoveDeadObjects();
            if (m_myVehicles.Count >= 3)
                return false;

            return true;
        }

        bool CanMakePedRequest()
        {
            bool bCan = (NetStatus.IsServer && this.isLocalPlayer) || (Time.time - m_timeWhenMadePedRequest > 2f);
            bCan &= (m_ped != null);

            m_timeWhenMadePedRequest = Time.time;
            
            return bCan;
        }

        public void RequestVehicleSpawn(int vehicleId) => this.CmdRequestVehicleSpawn(vehicleId);

        [Command]
        void CmdRequestVehicleSpawn(int vehicleId)
        {
            if (!this.CanPlayerSpawnVehicle())
                return;

            m_timeWhenSpawnedVehicle = Time.time;

            F.RunExceptionSafe( () => m_myVehicles.Add( Vehicle.CreateInFrontOf(vehicleId, m_ped.transform) ) );
        }

        public void RequestPedModelChange()
        {
            this.CmdRequestPedModelChange();
        }

        [Command]
        void CmdRequestPedModelChange()
        {
            if (!this.CanMakePedRequest())
                return;

            F.RunExceptionSafe( () => m_player.OwnedPed.PlayerModel.Load(Ped.RandomPedId) );
        }

        public void SpawnPedStalker() => this.CmdSpawnPedStalker();

        [Command]
        void CmdSpawnPedStalker()
        {
            if (!this.CanMakePedRequest())
                return;

            F.RunExceptionSafe( () => Ped.SpawnPedStalker(Ped.RandomPedId, m_ped.transform, m_ped) );
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

        public void RequestToDestroyMyVehicles() => this.CmdRequestToDestroyMyVehicles();
        
        [Command]
        void CmdRequestToDestroyMyVehicles()
        {
            F.RunExceptionSafe( () => {
                this.DestroyPlayersVehicles();
            });
        }

        void DestroyPlayersVehicles()
        {
            m_myVehicles.RemoveDeadObjects();
            foreach (var v in m_myVehicles)
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
                F.RunExceptionSafe( () => m_player.OwnedPed.Teleport(pos, rot) );
            }
        }


        #region weapons

        bool CanMakeWeaponRequest
        {
            get
            {
                bool bCan = (NetStatus.IsServer && this.isLocalPlayer) || Time.time - m_timeWhenMadeWeaponRequest > 2f;
                m_timeWhenMadeWeaponRequest = Time.time;
                return bCan;
            }
        }

        public void AddRandomWeapons() => this.CmdAddRandomWeapons();

        [Command]
        void CmdAddRandomWeapons()
        {
            if (!this.CanMakeWeaponRequest)
                return;
            if (m_ped != null)
                F.RunExceptionSafe( () => m_ped.WeaponHolder.AddRandomWeapons() );
        }

        public void RemoveAllWeapons() => this.CmdRemoveAllWeapons();

        [Command]
        void CmdRemoveAllWeapons()
        {
            if (!this.CanMakeWeaponRequest)
                return;
            if (m_ped != null)
                F.RunExceptionSafe( () => m_ped.WeaponHolder.RemoveAllWeapons() );
        }

        public void RemoveCurrentWeapon() => this.CmdRemoveCurrentWeapon();

        [Command]
        void CmdRemoveCurrentWeapon()
        {
            if (!this.CanMakeWeaponRequest)
                return;
            if (m_ped != null && m_ped.CurrentWeapon != null)
                F.RunExceptionSafe( () => Object.Destroy(m_ped.CurrentWeapon.gameObject) );
        }

        public void GiveAmmo() => this.CmdGiveAmmo();

        [Command]
        void CmdGiveAmmo()
        {
            if (!this.CanMakeWeaponRequest)
                return;
            
            if (m_ped != null)
            {
                F.RunExceptionSafe( () => {
                    foreach (var w in m_ped.WeaponHolder.AllWeapons)
                        WeaponHolder.AddRandomAmmoAmountToWeapon(w);
                } );
            }
        }

        public void GiveWeapon(int modelId) => this.CmdGiveWeapon(modelId);

        [Command]
        void CmdGiveWeapon(int modelId)
        {
            if (!this.CanMakeWeaponRequest)
                return;
            
            if (m_ped != null)
            {
                F.RunExceptionSafe( () => {
                    var w = m_ped.WeaponHolder.SetWeaponAtSlot(modelId, 0);
                    m_ped.WeaponHolder.SwitchWeapon(w.SlotIndex);
                    WeaponHolder.AddRandomAmmoAmountToWeapon(w);
                } );
            }
        }

        #endregion

    }

}
