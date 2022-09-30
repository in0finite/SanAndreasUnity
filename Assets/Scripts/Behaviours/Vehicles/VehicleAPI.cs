using System;
using System.Collections.Generic;
using System.Linq;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public static class VehicleAPI
    {
        #region "Lights"

        public const float constDamageFactor = 2, frontLightIntensity = 1.5f;

        public static Dictionary<VehicleLight, Vector3> blinkerPos = new Dictionary<VehicleLight, Vector3>();

        internal static Light SetCarLight(Vehicle vehicle, Transform parent, VehicleLight light, Vector3? pos = null)
        {
            GameObject gameObject = null;
            Light lightRet = SetCarLight(vehicle, parent, light, pos == null ? (IsLeftLight(light) ? new Vector3(-parent.localPosition.x * 2, 0, 0) : Vector3.zero) : pos.Value, out gameObject);

            // Now set its blinker
            Transform blinker = vehicle.transform.FindChildRecursive(parent.name + "2");

            // Note: If pixelLightCount is equal to 2 the blinker will never show

            //There is a bug, if the blinker is set the vehicle can't steer
            //if (blinker != null) // || testing ... QualitySettings.pixelLightCount > 2 // Not needed
                VehicleBlinker.Init(gameObject.transform, light, vehicle); //testing ? lightObj.transform :

            //Debug.Log("Is Blinker Null?: "+(blinker == null));

            return lightRet;

        }

        internal static Light SetCarLight(Vehicle vehicle, Transform parent, VehicleLight light, Vector3 pos, out GameObject go)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");

            Transform lightObj = new GameObject(GetLightName(light)).transform;
            lightObj.parent = parent;

            Quaternion rot = IsFrontLight(light) ? Quaternion.identity : Quaternion.Euler(Vector3.right * 180);

            lightObj.localPosition = pos;
            lightObj.localRotation = rot;

            Light ret = lightObj.gameObject.AddComponent<Light>();
            SetLightProps(GetVehicleLightParent(light).Value, ref ret);

            go = lightObj.gameObject;
            return ret;
        }

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

        internal static bool IsFrontLight(VehicleLight light)
        {
            return light == VehicleLight.Front || light == VehicleLight.FrontLeft || light == VehicleLight.FrontRight;
        }

        internal static bool IsLeftLight(VehicleLight light)
        {
            //Debug.LogFormat("Light type: {0} ({1})", light, light == VehicleLight.FrontLeft || light == VehicleLight.RearLeft);
            return light == VehicleLight.FrontLeft || light == VehicleLight.RearLeft;
        }

        internal static string GetLightName(VehicleLight light)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");
            string lightName = light.ToString();

            return string.Format("{0}Light", (IsFrontLight(light) ? lightName.Substring(5) : lightName.Substring(4)).ToLower());
        }

        internal static VehicleLight? GetVehicleLightParent(VehicleLight light)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) return null;
            string lightName = light.ToString();

            return (VehicleLight)System.Enum.Parse(typeof(VehicleLight), IsFrontLight(light) ? lightName.Substring(0, 5) : lightName.Substring(0, 4));
        }

        internal static bool IsValidIndividualLight(VehicleLight light)
        {
            return !(light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear);
        }

        internal static void SetLightProps(VehicleLight vehicleLight, ref Light light, bool isBlinker = false)
        {
            if (light == null) return;
            if (!isBlinker)
                switch (vehicleLight)
                {
                    case VehicleLight.Front:
                    case VehicleLight.FrontLeft:
                    case VehicleLight.FrontRight:
                        light.type = LightType.Spot;
                        light.range = 60;
                        light.spotAngle = 90;
                        light.intensity = frontLightIntensity;
                        break;

                    case VehicleLight.Rear:
                    case VehicleLight.RearLeft:
                    case VehicleLight.RearRight:
                        light.type = LightType.Spot;
                        light.range = 20;
                        light.spotAngle = 50;
                        light.intensity = 1;
                        light.color = Color.red;
                        break;
                }
            else
            {
                light.type = LightType.Spot;
                light.range = 10;
                light.spotAngle = 140;
                light.intensity = .8f;
                light.color = new Color(1, .5f, 0);
            }
        }

        internal static IEnumerable<GameObject> GetLightObjects(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<Light>().Select(x => x.gameObject);
        }

        internal static VehicleLight ParseFromBit(int bit)
        {
            return (VehicleLight)((int)Mathf.Pow(2, bit));
        }

        #endregion "Lights"
    }
}