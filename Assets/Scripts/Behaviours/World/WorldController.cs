using SanAndreasUnity.Utilities;
using System;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public enum TimeState { Dawn, Noon, Dusk, Midnight }

    // TODO: TimeFactor -> AngleFactor
    public class WorldController : MonoBehaviour
    {
        public const float dayCycleMins = 24,
                           relMinSecs = 1; // That means that one second in real life in one minute in game

        private static float dayTimeCounter, dayCount;

        public AnimationCurve lightCurve;
        public Transform dirLight;

        public TimeState startTimeState;

        private Light light;

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

        public static bool IsNight
        {
            get
            {
                return ((dayTimeCounter * AngleFactor) % 360).BetweenInclusive(180, 360);
            }
        }

        private void Awake()
        {
            light = dirLight.GetComponent<Light>();
        }

        // Use this for initialization
        private void Start()
        {
            SetTime(startTimeState);
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            //360 = 24 minutos
            //x = Time.deltaTime

            if (dirLight != null)
            {
                float prod = dayTimeCounter * AngleFactor, 
                      angle = prod % 360;

                if (prod > 0 && prod % 360 == 0)
                {
                    ++dayCount;
                    Debug.Log("Day "+dayCount);
                }

                dirLight.rotation = Quaternion.Euler(angle, -130, 0);
                dayTimeCounter += AngleFactor;

                // Range: Dusk .. Dawn
                if (IsNight) light.intensity = lightCurve.Evaluate(Mathf.InverseLerp(180, 360, angle));
            }
        }

        // Must review
        public static void SetTime(TimeState time)
        {
            switch (time)
            {
                case TimeState.Dawn:
                    dayTimeCounter = dayCount > 0 ? GetRoundedTime(TimeFactor) : 0;
                    break;

                case TimeState.Noon:
                    dayTimeCounter = dayCount > 0 ? GetRoundedTime(90 * TimeFactor) : TimeFactor * 90;
                    break;

                case TimeState.Dusk:
                    dayTimeCounter = dayCount > 0 ? GetRoundedTime(180 * TimeFactor) : TimeFactor * 180;
                    break;

                case TimeState.Midnight:
                    dayTimeCounter = dayCount > 0 ? GetRoundedTime(270 * TimeFactor) : TimeFactor * 270;
                    break;
            }

            Debug.LogFormat("Time set to {0}! ({1})", time.ToString(), dayTimeCounter);
        }

        private static float GetRoundedTime(float X)
        {
            float completeDay = 360 * TimeFactor;
            //Debug.LogWarning("Days: "+dayCount);

            return completeDay * dayCount + X;
        }
    }
}