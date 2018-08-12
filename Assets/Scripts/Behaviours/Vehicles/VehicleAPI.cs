using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public static class VehicleAPI
    {
        #region "Lights"

        public const float constDamageFactor = 2;
        public static Dictionary<VehicleLight, Vector3> blinkerPos = new Dictionary<VehicleLight, Vector3>();

        internal static void LoopBlinker(VehicleBlinkerMode light, Action<Vector3> act)
        {
            try
            {
                switch (light)
                {
                    case VehicleBlinkerMode.Left:
                        act(blinkerPos[VehicleLight.FrontLeft]);
                        act(blinkerPos[VehicleLight.RearLeft]);
                        break;

                    case VehicleBlinkerMode.Right:
                        act(blinkerPos[VehicleLight.FrontRight]);
                        act(blinkerPos[VehicleLight.RearRight]);
                        break;
                }
            } catch { }
        }

        #endregion "Lights"
    }
}