using SanAndreasUnity.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LightData = SpriteLights.LightData;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public static class VehicleAPI
    {
        #region "Lights"

        internal static Light SetCarLight(Vehicle vehicle, Transform parent, VehicleLight light, Vector3? pos = null, bool setBlinker = true)
        {
            GameObject gameObject = null;
            return SetCarLight(vehicle, parent, light, pos == null ? (IsLeftLight(light) ? Vector3.zero : new Vector3(-parent.localPosition.x * 2, 0, 0)) : pos.Value, out gameObject, setBlinker);
        }

        internal static Light SetCarLight(Vehicle vehicle, Transform parent, VehicleLight light, Vector3 pos, out GameObject go, bool setBlinker = true)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");

            Transform lightObj = new GameObject(GetLightName(light)).transform;
            lightObj.parent = parent;

            Quaternion rot = IsFrontLight(light) ? Quaternion.identity : Quaternion.Euler(Vector3.right * 180);

            lightObj.localPosition = pos;
            lightObj.localRotation = rot;

            // Rear light props
            Light ret = lightObj.gameObject.AddComponent<Light>();
            SetLightProps(GetVehicleLightParent(light).Value, ref ret);

            // Now set its blinker
            if (setBlinker)
                VehicleBlinker.Init(lightObj.gameObject, lightObj.transform, light, vehicle);

            go = lightObj.gameObject;
            return ret;
        }

        internal static bool IsFrontLight(VehicleLight light)
        {
            return light == VehicleLight.Front || light == VehicleLight.FrontLeft || light == VehicleLight.FrontRight;
        }

        internal static bool IsLeftLight(VehicleLight light)
        {
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
                        light.intensity = 2;
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

        internal static void SetLightSources(GameObject gameObject, Material mat)
        {
            List<LightData> datas = new List<LightData>();
            var objs = GetLightObjects(gameObject);

            //Map object with an index
            //Debug.LogFormat("Objs: {0}", objs.Count());

            foreach (var go in objs)
            {
                LightData lightData = new LightData();

                lightData.position = go.transform.position;
                lightData.brightness = 1;
                lightData.size = 1;

                datas.Add(lightData);
            }

            // WIP: Move this to blinker object, where we need to generate the spritelight

            //var obj = SpriteLights.CreateLights(gameObject.name.ToLower() + "-LD", datas.ToArray(), mat);

            //Debug.LogFormat("Obj Count: {0}", obj.Count());

            /*gameObject.transform.MakeChild(obj, (p, o) =>
            {
                // Check the index and them set where is has to be generated
                o.transform.position = m_frontLeftLight.transform.position;
            });*/
        }

        internal static IEnumerable<GameObject> GetLightObjects(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<Light>().Select(x => x.gameObject);
        }

        #endregion "Lights"
    }
}