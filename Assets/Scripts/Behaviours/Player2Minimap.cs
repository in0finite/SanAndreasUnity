using UnityEngine;
using SanAndreasUnity.UI;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{

    public class Player2Minimap : MonoBehaviour
    {
        Net.Player m_player;


        void Awake()
        {
            m_player = this.GetComponentOrThrow<Net.Player>();
        }

        void OnEnable()
        {
            UI.MapWindow.Instance.onDrawMapItems += OnMinimapGUI;
        }

        void OnDisable()
        {
            UI.MapWindow.Instance.onDrawMapItems -= OnMinimapGUI;
        }

        void OnMinimapGUI()
        {
            if (m_player == Net.Player.Local)   // don't draw anything for local player - it's done by map window
                return;

            var ped = m_player.OwnedPed;
            if (null == ped)
                return;
            
            MapWindow.Instance.DrawItemOnMapRotated( MiniMap.Instance.PlayerBlip, ped.transform.position, ped.transform.forward, 
                (int) MapWindow.Instance.PlayerPointerSize );

        }

    }
}
