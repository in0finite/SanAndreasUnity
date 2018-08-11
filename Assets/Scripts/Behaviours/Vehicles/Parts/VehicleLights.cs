using SanAndreasUnity.Behaviours.World;
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

            lights.lightType = light;
            lights.vehicle = vehicle;

            lights.SetLightProps();

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

        public void SetLightProps()
        {
            if (lightComponent == null) return;

            switch (lightType)
            { //Must review: In some cases car lights are powered by no reason
                case VehicleLight.Front:
                case VehicleLight.FrontLeft:
                case VehicleLight.FrontRight:
                    lightComponent.type = LightType.Spot;
                    lightComponent.range = 60;
                    lightComponent.spotAngle = 90;
                    lightComponent.intensity = WorldController.IsNight ? frontLightIntensity : 0;
                    break;

                case VehicleLight.Rear:
                case VehicleLight.RearLeft:
                case VehicleLight.RearRight:
                    lightComponent.type = LightType.Spot;
                    lightComponent.range = 20;
                    lightComponent.spotAngle = 50;
                    lightComponent.intensity = WorldController.IsNight ? rearLightIntensity : 0;
                    lightComponent.color = Color.red;
                    break;
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
                if (isRear)
                    return _isPowered;

                return true;
            }
            set
            {
                if (!isRear)
                {
                    _isPowered = value;
                    SetLight(_isPowered ? frontLightIntensity : 0);
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
            {
                Debug.Log("Toggling lights");
                isPowered = !isPowered;
            }
        }

        public void OnDuskTime()
        {
            IsNightToggled = true;
        }

        public void OnDawnTime()
        {
            IsNightToggled = false;
        }

        public override void OnVehicleCollisionEnter(Collision collision)
        {
            if (isOk && (collision.contacts[0].point - transform.position).sqrMagnitude < lightContactDistance)
            {
                Debug.LogFormat("{0} has broken!!", lightType);
                isOk = false;
            }
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

            if (lightComponent != null)
            {
                if (brightness > 0 && !lightComponent.enabled)
                {
                    lightComponent.enabled = true;
                    lightComponent.intensity = brightness;
                }
                else
                {
                    lightComponent.enabled = false;
                }
            }

            vehicle.SetLight((int)Mathf.Log((int)lightType, 2), brightness);
        }
    }
}