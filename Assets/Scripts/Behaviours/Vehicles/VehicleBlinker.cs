using SanAndreasUnity.Behaviours.Vehicles;
using System;
using UnityEngine;

public class VehicleBlinker : MonoBehaviour
{
    #region "Fields"

    #region "Public Fields"

    public float repeatInterval = .5f;

    #endregion "Public Fields"

    #region "Init private fields"

    private VehicleLight lightType;
    private Transform parent;
    private Vehicle vehicle;

    #endregion "Init private fields"

    #region "Ordinary private fields"

    private bool _blinkerSwitch;
    private MeshRenderer blinkerRenderer;
    private float defaultIntensity;

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

    private bool ShouldBePowered
    {
        get
        {
            return (IsLeftSide && vehicle.blinkerMode == VehicleBlinkerMode.Left || !IsLeftSide && vehicle.blinkerMode == VehicleBlinkerMode.Right) || vehicle.blinkerMode == VehicleBlinkerMode.Emergency;
        }
    }

    private bool blinkerSwitch
    {
        get
        {
            return _blinkerSwitch;
        }

        set
        {
            _blinkerSwitch = value;
            ToggleBlinker(_blinkerSwitch);
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
        //obj.transform.localRotation = Quaternion.Euler(new Vector3(0, 30 * (IsLeftSide ? -1 : 1), 0));
        obj.transform.localScale = Vector3.one * .2f;

        blinkerRenderer = obj.GetComponent<MeshRenderer>();

        blinkerRenderer.material = Resources.Load<Material>("Materials/Blinker");
        defaultIntensity = blinkerRenderer.material.GetFloat("_MKGlowPower");

        blinkerSwitch = false;

        InvokeRepeating("Cycle", 0, repeatInterval);
    }

    // Update is called once per frame
    private void Update()
    {
        // Must review
        if (vehicle.HasDriverSeat && !ShouldBePowered && blinkerSwitch)
        {
            //Debug.Log("Turning off blinkers!");
            blinkerSwitch = false;
        }
    }

    private void Cycle()
    {
        if (!(vehicle.HasDriverSeat && ShouldBePowered))
            return;

        blinkerSwitch = !blinkerSwitch;
    }

    private void ToggleBlinker(bool active)
    {
        blinkerRenderer.material.SetFloat("_MKGlowPower", active ? defaultIntensity : 0);
    }
}