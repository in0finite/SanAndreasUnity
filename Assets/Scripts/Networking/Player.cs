using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace SanAndreasUnity.Net
{

    public class Player : NetworkBehaviour
    {

        static List<Player> s_allPlayers = new List<Player>();
        public static Player[] AllPlayers { get { return s_allPlayers.ToArray(); } }


        void OnEnable()
        {
            s_allPlayers.Add(this);
        }

        void OnDisable()
        {
            s_allPlayers.Remove(this);
        }

        void Start()
        {
            
        }

    }

}
