using UnityEngine;
using SanAndreasUnity.UI;
using SanAndreasUnity.Behaviours.Vehicles;
using UGameCore.Utilities;

namespace SanAndreasUnity.Behaviours
{

    public class Vehicle2Minimap : MonoBehaviour
    {
        
        Vehicle m_vehicle;


        void Start()
        {
            // use Start(), because vehicle script is dynamically added to game object
            m_vehicle = this.GetComponentOrThrow<Vehicle>();
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
            if (null == m_vehicle)
                return;
            
            //MapWindow.Instance.DrawItemOnMap( MiniMap.Instance.VehicleTexture, m_vehicle.transform.position, 12 );

        }

    }

}
