using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public const float dayCycleMins = 24,
                       relMinSecs = 1; // That means that one second in real life in one minute in game

    private static float dayTimeCounter;

    public Transform dirLight;

    public static float AngleFactor
    {
        get
        {
            return (1f / Time.deltaTime) * (dayCycleMins * 60) / 360;
        }
    }

    public static float TimeFactor
    {
        get
        {
            return 360 / ((1f / Time.deltaTime) * (dayCycleMins * 60));
        }
    }

    // Use this for initialization
    private void Start()
    {
        dayTimeCounter = 50 * AngleFactor;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        //360 = 24 minutos
        //x = Time.deltaTime

        if (dirLight != null)
        {
            dirLight.rotation = Quaternion.Euler((dayTimeCounter * TimeFactor) % 360, -130, 0);
            dayTimeCounter += TimeFactor;
        }
    }
}