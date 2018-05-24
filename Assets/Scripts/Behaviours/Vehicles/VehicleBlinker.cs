using SanAndreasUnity.Behaviours.Vehicles;
using System;
using UnityEngine;

public class VehicleBlinker : Vehicle
{
    private bool setAppart;

    [HideInInspector]
    public VehicleLight lightType;

    [HideInInspector]
    public Transform parent;

    //private VehicleLight? lightSide;

    private float blinkerCounter = 0, defaultIntesity;

    private Light blinkerLight;

    private bool blinkerSwitch, success;

    private bool IsLeftSide
    {
        get
        {
            return IsLeftLight(lightType);
        }
    }

    // Use this for initialization
    private void Start()
    {
        //lightSide = GetVehicleLightSide(lightType);

        if (!IsValidIndividualLight(lightType)) throw new Exception("Light sides need to have a valid value, revise your code.");
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

        SetLightProps(lightType, ref blinkerLight, true);

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
        return IsLeftSide && (blinkerMode == VehicleBlinkerMode.Left || blinkerMode == VehicleBlinkerMode.Emergency);
    }
}