using SanAndreasUnity.Behaviours.Vehicles;
using System;
using UnityEngine;

public class VehicleBlinker : MonoBehaviour
{
    #region "Fields"

    #region "Init private fields"

    private VehicleLight lightType;
    private Transform parent;
    private Vehicle vehicle;

    #endregion "Init private fields"

    #region "Ordinary private fields"

    private bool setAppart;
    private float blinkerCounter = 0, defaultIntesity;
    private Light blinkerLight;
    private bool blinkerSwitch, success;

    #endregion "Ordinary private fields"

    #endregion "Fields"

    public static VehicleBlinker Init(GameObject go, Transform par, VehicleLight light, Vehicle vh)
    {
        VehicleBlinker vehicleBlinker = go.AddComponent<VehicleBlinker>();

        vehicleBlinker.parent = par;
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
        //lightSide = GetVehicleLightSide(lightType);

        if (!VehicleAPI.IsValidIndividualLight(lightType)) throw new Exception("Light sides need to have a valid value, revise your code.");
        success = true;

        setAppart = gameObject.GetComponent<Light>() != null;

        GameObject obj = gameObject;

        if (setAppart)
        {
            obj = new GameObject("Blinker");
            obj.transform.parent = parent;
            obj.transform.position = parent.position + Vector3.right * (IsLeftSide ? -1 : 1) * .2f;
        }

        blinkerLight = obj.AddComponent<Light>();

        VehicleAPI.SetLightProps(lightType, ref blinkerLight, true);

        defaultIntesity = blinkerLight.intensity;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!success) return;
        if (ShouldBePowered(lightType))
        {
            if ((int)blinkerCounter % 2 == 0)
                blinkerSwitch = !blinkerSwitch;

            blinkerLight.intensity = blinkerSwitch ? defaultIntesity : 0;

            blinkerCounter += Time.deltaTime;

            if (blinkerCounter > 1000)
                blinkerCounter = 0; // Reset not overflow
        }
    }

    private bool ShouldBePowered(VehicleLight side)
    {
        //if (!side.HasValue) throw new Exception("Light sides need to have a value, revise your code.");
        return IsLeftSide && (vehicle.blinkerMode == VehicleBlinkerMode.Left || vehicle.blinkerMode == VehicleBlinkerMode.Emergency);
    }
}