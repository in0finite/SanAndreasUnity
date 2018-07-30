using SanAndreasUnity.Behaviours.World;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public enum VehicleLight
    {
        FrontLeft = 1,
        FrontRight = 2,

        RearLeft = 4,
        RearRight = 8,

        Front = FrontLeft | FrontRight,
        Rear = RearLeft | RearRight,

        All = Front | Rear
    }

    [RequireComponent(typeof(Light))]
    public class VehicleLights : VehicleBehaviour
    {
        public const float frontLightIntensity = 1.5f, rearLightIntensity = .7f, lightContactDistance = 5;
        private bool _isNightToggled;
        public VehicleLight lightType;

        private bool IsNightToggled
        {
            get
            {
                return _isNightToggled;
            }
            set
            {
                _isNightToggled = value;
                if (canPower) SetLight(_isNightToggled ? (isRear ? rearLightIntensity : frontLightIntensity) : 0);
            }
        }

        [Obsolete]
        public void ToggleMightLights(bool isFront)
        {
            if (isFront)
                vehicle.SetMultipleLights(VehicleLight.Front, _isNightToggled ? frontLightIntensity : 0);
            else
                vehicle.SetMultipleLights(VehicleLight.Rear, _isNightToggled ? rearLightIntensity : 0);
        }

        public static VehicleLights Init(Transform parent, Vehicle vehicle, VehicleLight light, Vector3? pos = null)
        {
            GameObject gameObject = null;
            VehicleLights lightRet = Init(parent, vehicle, light, pos == null ? (IsLeftLight(light) ? new Vector3(-parent.localPosition.x * 2, 0, 0) : Vector3.zero) : pos.Value, out gameObject);

            Transform blinker = vehicle.transform.FindChildRecursive(parent.name + "2");

            //There is a bug, if the blinker is set the vehicle can't steer
            VehicleBlinker.Init(gameObject.transform, lightRet, vehicle);

            return lightRet;
        }

        public static VehicleLights Init(Transform parent, Vehicle vehicle, VehicleLight light, Vector3 pos, out GameObject go)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");

            VehicleLights lights = parent.gameObject.AddComponent<VehicleLights>();

            Transform lightObj = new GameObject(GetLightName(light)).transform;
            lightObj.parent = parent;

            Quaternion rot = IsFrontLight(light) ? Quaternion.identity : Quaternion.Euler(Vector3.right * 180);

            lightObj.localPosition = pos;
            lightObj.localRotation = rot;

            lights.lightComponent = lightObj.gameObject.AddComponent<Light>();

            SetLightProps(GetVehicleLightParent(light).Value, ref lights.lightComponent);

            //Debug.LogFormat("Added {0} light!", light);

            lights.lightType = light;
            lights.vehicle = vehicle;

            vehicle.m_lightDict.Add(light, lights);

            go = lightObj.gameObject;
            return lights;
        }

        public static bool IsFrontLight(VehicleLight light)
        {
            return light == VehicleLight.Front || light == VehicleLight.FrontLeft || light == VehicleLight.FrontRight;
        }

        public static bool IsLeftLight(VehicleLight light)
        {
            return light == VehicleLight.FrontLeft || light == VehicleLight.RearLeft;
        }

        public static string GetLightName(VehicleLight light)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) throw new System.Exception("Light must be right or left, can't be general!");
            string lightName = light.ToString();

            return string.Format("{0}Light", (IsFrontLight(light) ? lightName.Substring(5) : lightName.Substring(4)).ToLower());
        }

        public static VehicleLight? GetVehicleLightParent(VehicleLight light)
        {
            if (light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear) return null;
            string lightName = light.ToString();

            return (VehicleLight)System.Enum.Parse(typeof(VehicleLight), IsFrontLight(light) ? lightName.Substring(0, 5) : lightName.Substring(0, 4));
        }

        public static bool IsValidIndividualLight(VehicleLight light)
        {
            return !(light == VehicleLight.All || light == VehicleLight.Front || light == VehicleLight.Rear);
        }

        public static void SetLightProps(VehicleLight vehicleLight, ref Light light, bool isBlinker = false)
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

        public static IEnumerable<GameObject> GetLightObjects(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<Light>().Select(x => x.gameObject);
        }

        public static VehicleLight ParseFromBit(int bit)
        {
            return (VehicleLight)((int)Mathf.Pow(2, bit));
        }

        private bool _isOk = true, _isPowered;
        public Light lightComponent;
        private Vehicle vehicle;

        public bool isOk
        {
            get
            {
                return _isOk;
            }
            set
            {
                _isOk = value;
                if (!_isOk)
                    SetLight(0);
            }
        }

        public bool canPower
        {
            get
            {
                return isOk && isPowered;
            }
        }

        public bool isRear
        {
            get
            {
                return !IsFrontLight(lightType);
            }
        }

        public bool isLeft
        {
            get
            {
                return IsLeftLight(lightType);
            }
        }

        public bool isPowered
        {
            get
            {
                if(isRear)
                    return _isPowered;
                return true;
            }
            set
            {
                if (!isRear)
                {
                    _isPowered = value;
                    vehicle.SetMultipleLights(VehicleLight.Front, frontLightIntensity);
                }
            }
        }

        // Use this for initialization
        private void Start()
        {
            lightComponent = GetComponent<Light>();
            _isPowered = true;
            IsNightToggled = WorldController.IsNight;
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L) && !isRear)
                isPowered = !isPowered;
        }

        public override void OnVehicleCollisionEnter(Collision collision)
        {
            if (isOk && (collision.contacts[0].point - transform.position).sqrMagnitude < lightContactDistance)
                isOk = false;
        }

        public override void OnVehicleCollisionExit(Collision collision)
        {
        }

        public override void OnVehicleCollisionStay(Collision collision)
        {
        }

        public override void OnVehicleTriggerEnter(Collider other)
        {
        }

        public override void OnVehicleTriggerExit(Collider other)
        {
        }

        public override void OnVehicleTriggerStay(Collider other)
        {
        }

        public void SetLight(float brightness)
        {
            brightness = Mathf.Clamp01(brightness);

            bool mustRearPower = _isNightToggled && !IsFrontLight(lightType);

            if (brightness > 0 || mustRearPower)
            {
                if (lightComponent != null && !lightComponent.enabled)
                {
                    lightComponent.enabled = true;
                    lightComponent.intensity = mustRearPower ? rearLightIntensity : brightness;
                }
            }
            else
            {
                if (lightComponent != null) lightComponent.enabled = false;
            }

            //Debug.LogFormat("[{0}] CanPower? {1} ({2})", vehicle.name, lightType, brightness);
            vehicle.SetLight((int)Mathf.Log((int)lightType, 2), mustRearPower ? rearLightIntensity : brightness);
        }
    }
}