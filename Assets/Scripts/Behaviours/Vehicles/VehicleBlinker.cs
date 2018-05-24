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

    private float defaultIntensity;

    private Light blinkerLight;

    private bool blinkerSwitch,
                 setAppart;

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

        setAppart = gameObject.GetComponent<Light>() != null;

        GameObject obj = gameObject;

        if (setAppart)
        {
            obj = new GameObject("Blinker");
            obj.transform.parent = parent;
            obj.transform.position = parent.position + Vector3.right * (IsLeftSide ? -1 : 1) * .2f;
            obj.transform.localRotation = Quaternion.Euler(new Vector3(0, 30 * (IsLeftSide ? -1 : 1), 0));
        }

        blinkerLight = obj.AddComponent<Light>();

        VehicleAPI.SetLightProps(lightType, ref blinkerLight, true);

        defaultIntensity = blinkerLight.intensity;

        ToggleBlinker(false);

        InvokeRepeating("Cycle", 0, repeatInterval);
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void Cycle()
    {
        bool lastSwitch = blinkerSwitch;
        if (ShouldBePowered(lightType))
        {
            if (lastSwitch != blinkerSwitch)
                ToggleBlinker(blinkerSwitch);

            blinkerSwitch = !blinkerSwitch;
        }
    }

    private bool ShouldBePowered(VehicleLight side)
    {
        //if (!side.HasValue) throw new Exception("Light sides need to have a value, revise your code.");
        return IsLeftSide && (vehicle.blinkerMode == VehicleBlinkerMode.Left || vehicle.blinkerMode == VehicleBlinkerMode.Emergency);
    }

    private void ToggleBlinker(bool active)
    {
        blinkerLight.intensity = active ? defaultIntensity : 0;
        blinkerLight.enabled = active;
    }
}