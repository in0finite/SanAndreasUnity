using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SanAndreasUnity.Behaviours;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Stats
{
    public class MiscStats : MonoBehaviour
    {
        
        void Start()
        {
            Utilities.Stats.RegisterStat(new Utilities.Stats.Entry(){category = "MISC", onGUI = OnStatGUI});
        }

        void OnStatGUI()
        {

            GUILayout.Label("Num peds: " + Ped.NumPeds);
            GUILayout.Label("Num vehicles: " + Vehicle.NumVehicles);
            GUILayout.Label("Num ped state changes received: " + Ped.NumStateChangesReceived);

        }

    }
}
