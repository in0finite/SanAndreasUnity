﻿using UnityEngine;
using Mirror;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Weapons
{

    public class NetworkedWeapon : NetworkBehaviour
    {

        [SyncVar] int m_net_modelId;
        [SyncVar] int m_net_ammoInClip;
        [SyncVar] int m_net_ammoOutsideOfClip;
        [SyncVar] GameObject m_net_pedOwnerGameObject;

        public int ModelId { get { return m_net_modelId; } set { m_net_modelId = value; } }
        public int AmmoInClip { get { return m_net_ammoInClip; } set { m_net_ammoInClip = value; } }
        public int AmmoOutsideOfClip { get { return m_net_ammoOutsideOfClip; } set { m_net_ammoOutsideOfClip = value; } }
        public Ped PedOwner { get { return m_net_pedOwnerGameObject != null ? m_net_pedOwnerGameObject.GetComponent<Ped>() : null; } set { m_net_pedOwnerGameObject = value != null ? value.gameObject : null; } }



        public override void OnStartClient()
        {
            base.OnStartClient();

            if (NetUtils.IsServer)
                return;

            // create weapon
            F.RunExceptionSafe( () => Weapon.OnWeaponCreatedByServer(this) );
        }
    }

}
