using System.Collections.Generic;
using UnityEngine;
using Mirror;
using SanAndreasUnity.Behaviours;

namespace SanAndreasUnity.Net
{

    public class Player : NetworkBehaviour
    {

        static List<Player> s_allPlayers = new List<Player>();
        public static Player[] AllPlayers { get { return s_allPlayers.ToArray(); } }

        /// <summary>Local player.</summary>
        public static Player Local { get; private set; }

        [SyncVar(hook=nameof(OnOwnedGameObjectChanged))] GameObject m_ownedGameObject;
        Ped m_ownedPed;
        //public GameObject OwnedGameObject { get { return m_ownedGameObject; } internal set { m_ownedGameObject = value; } }
        public Ped OwnedPed { get { return m_ownedPed; } internal set { m_ownedPed = value; m_ownedGameObject = value != null ? value.gameObject : null; } }


        void OnEnable()
        {
            s_allPlayers.Add(this);
        }

        void OnDisable()
        {
            s_allPlayers.Remove(this);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            
            if (this.isServer)
                return;
            
            m_ownedPed = m_ownedGameObject != null ? m_ownedGameObject.GetComponent<Ped>() : null;
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            Local = this;
        }

        void Start()
        {
            
        }

        void OnOwnedGameObjectChanged(GameObject newGo)
        {
            if (this.isServer)
                return;

            m_ownedGameObject = newGo;

            m_ownedPed = m_ownedGameObject != null ? m_ownedGameObject.GetComponent<Ped>() : null;
        }

    }

}
