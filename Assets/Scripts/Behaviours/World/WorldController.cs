using SanAndreasUnity.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public enum TimeState { Dawn, Noon, Dusk, Midnight }

    public class WorldController : MonoBehaviour
    {
        public const float dayCycleMins = 24,
                           relMinSecs = 1; // That means that one second in real life in one minute in game

        private static float dayTimeCounter;

        public AnimationCurve lightCurve;
        public Transform dirLight;
        private Light light;

        private static Rect windowRect = new Rect(10, 350, 250, 200);
        private const int windowID = 3;

        public static float TimeFactor
        {
            get
            {
                return (1f / Time.deltaTime) * (dayCycleMins * 60) / 360;
            }
        }

        public static float AngleFactor
        {
            get
            {
                return 360 / ((1f / Time.deltaTime) * (dayCycleMins * 60));
            }
        }

        private void Awake()
        {
            light = dirLight.GetComponent<Light>();
        }

        // Use this for initialization
        private void Start()
        {
            dayTimeCounter = 50 * TimeFactor;
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            //360 = 24 minutos
            //x = Time.deltaTime

            if (dirLight != null)
            {
                float angle = (dayTimeCounter * AngleFactor) % 360;

                dirLight.rotation = Quaternion.Euler(angle, -130, 0);
                dayTimeCounter += AngleFactor;

                // Range: Dusk .. Dawn
                if (angle.BetweenInclusive(180, 360)) light.intensity = lightCurve.Evaluate(Mathf.InverseLerp(180, 360, angle));
            }
        }

        private void OnGUI()
        {
            if (!PlayerController._showMenu)
                return;

            windowRect = GUILayout.Window(windowID, windowRect, timeWindow, "Set Time");
        }

        private void timeWindow(int windowID)
        {
            GUILayout.Label("Set Time:");

            foreach (var en in Enum.GetValues(typeof(TimeState)))
            {
                TimeState e = (TimeState)en;
                if (GUILayout.Button(e.ToString()))
                    SetTime(e);
            }

            GUI.DragWindow();
        }

        public static void SetTime(TimeState time)
        {
            switch (time)
            {
                case TimeState.Dawn:
                    dayTimeCounter = GetNearestWholeMultiple(dayTimeCounter, TimeFactor);
                    break;

                case TimeState.Noon:
                    dayTimeCounter = GetNearestWholeMultiple(dayTimeCounter, 90 * TimeFactor);
                    break;

                case TimeState.Dusk:
                    dayTimeCounter = GetNearestWholeMultiple(dayTimeCounter, 180 * TimeFactor);
                    break;

                case TimeState.Midnight:
                    dayTimeCounter = GetNearestWholeMultiple(dayTimeCounter, 270 * TimeFactor);
                    break;
            }

            Debug.Log(string.Format("Time set to {0}! ({1})", time.ToString(), dayTimeCounter));
        }

        private static float GetNearestWholeMultiple(float input, float X)
        {
            var output = Mathf.Round(input / X);
            if (output == 0 && input > 0) output += 1;
            output *= X;

            return output;
        }
    }
}