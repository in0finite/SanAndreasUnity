using SanAndreasUnity.Behaviours.Vehicles;
using System;
using UnityEngine;

public class VehicleBlinker : MonoBehaviour
{
    #region "Fields"

    #region "Public Fields"

    public float repeatInterval = 1;

    #endregion "Public Fields"

    #region "Init private fields"

    private VehicleLight lightType;
    private Transform parent;
    private Vehicle vehicle;

    #endregion "Init private fields"

    #region "Ordinary private fields"

    private bool blinkerSwitch;

    #endregion "Ordinary private fields"

    #endregion "Fields"

    public static VehicleBlinker Init(Transform blinker, VehicleLight light, Vehicle vh)
    {
        VehicleBlinker vehicleBlinker = blinker.gameObject.AddComponent<VehicleBlinker>();

        vehicleBlinker.parent = blinker;
        vehicleBlinker.lightType = light;
        vehicleBlinker.vehicle = vh;

        return vehicleBlinker;
    }

    private bool IsLeftSide
    {
        get
        {
            return VehicleAPI.IsLeftLight(lightType);
        }
    }

    // Use this for initialization
    private void Start()
    {
        if (!VehicleAPI.IsValidIndividualLight(lightType)) throw new Exception("Light sides need to have a valid value, revise your code.");

        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);

        //If you uncomment this wheels won't steer
        //Destroy(obj.GetComponent<CapsuleCollider>());

        obj.name = string.Format("Blinker ({0})", lightType.ToString());
        obj.transform.parent = parent;
        obj.transform.position = parent.position + Vector3.right * (IsLeftSide ? -1 : 1) * .2f;
        obj.transform.localRotation = Quaternion.Euler(new Vector3(0, 30 * (IsLeftSide ? -1 : 1), 0));
        obj.transform.localScale = Vector3.one * .2f;

        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

        renderer.material = Resources.Load<Material>("Materials/Blinker");

        ToggleBlinker(false);

        InvokeRepeating("Cycle", 0, repeatInterval);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void Cycle()
    {
        if ((vehicle.HasDriver && ShouldBePowered(lightType)) || blinkerSwitch)
        {
            ToggleBlinker(blinkerSwitch);
            blinkerSwitch = !blinkerSwitch;
        }
    }

    private bool ShouldBePowered(VehicleLight side)
    {
        //if (!side.HasValue) throw new Exception("Light sides need to have a value, revise your code.");
        //Debug.LogFormat("Blinker Mode: {0}; Steering: {1}", vehicle.blinkerMode, vehicle.Steering);
        return IsLeftSide && (vehicle.blinkerMode == VehicleBlinkerMode.Left || vehicle.blinkerMode == VehicleBlinkerMode.Emergency);
    }

    private void ToggleBlinker(bool active)
    {
        //blinkerLight.intensity = active ? defaultIntensity : 0;
        //blinkerLight.enabled = active;
    }
}