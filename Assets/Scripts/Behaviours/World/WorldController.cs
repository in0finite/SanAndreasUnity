using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public enum TimeState { Dawn = 0, Noon = 90, Dusk = 180, Midnight = 270 }

    // TODO: TimeFactor -> AngleFactor
    public class WorldController : MonoBehaviour
    {
        public static WorldController Instance;

        public const float dayCycleMins = 24,
                           relMinSecs = 1; // That means that one second in real life in one minute in game

        private static float tickCounter, dayCount;

        public AnimationCurve lightCurve;
        public Transform dirLight;

        public TimeState startTimeState;

        private new Light light;

        // Number of time ticks in a second
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
                return Instance.light.transform.eulerAngles.x.BetweenInclusive(180, 360);
            }
        }

        public static float LightAngle
        {
            get
            {
                return tickCounter * AngleFactor;
            }
        }

        public static float TickAngleFactor
        {
            get
            {
                return (1 / Time.fixedDeltaTime) / TimeFactor;
            }
        }

        public static float CompleteTickCycle
        {
            get
            {
                return dayCycleMins * 60 * TimeFactor * TickAngleFactor;
            }
        }

        public static float LightComponentAngle
        {
            get
            {
                return Instance.light.transform.eulerAngles.x;
            }
            set
            {
                Vector3 v = Instance.light.transform.eulerAngles;
                Instance.light.transform.eulerAngles = new Vector3(value, v.y, v.z);
            }
        }

        private void Awake()
        {
            light = dirLight.GetComponent<Light>();
            Instance = this;
        }

        // Use this for initialization
        private void Start()
        {
            SetTime(startTimeState);
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            if (dirLight != null)
            {
                float angle = LightAngle % 360;

                if (LightAngle > 0 && LightAngle % 360 == 0)
                {
                    ++dayCount;
                    Debug.Log("Day "+dayCount);
                }

                dirLight.rotation = Quaternion.Euler(angle, -130, 0);
                ++tickCounter;

                if(LightComponentAngle == 0)
                    F.SendMessageToObjectsOfType<MonoBehaviour>("OnDawnTime");
                else if (LightComponentAngle % 90 == 0)
                    F.SendMessageToObjectsOfType<MonoBehaviour>("OnNoonTime");
                else if (LightComponentAngle % 180 == 0)
                    F.SendMessageToObjectsOfType<MonoBehaviour>("OnDuskTime");
                else if (LightComponentAngle % 270 == 0)
                    F.SendMessageToObjectsOfType<MonoBehaviour>("OnMidnightTime");

                // Range: Dusk .. Dawn
                if (IsNight) light.intensity = lightCurve.Evaluate(Mathf.InverseLerp(180, 360, angle));
            }
        }

        // WIP: Sum ticks
        public static void SetTime(TimeState time, bool callback = true)
        {
            switch (time)
            {
                case TimeState.Dawn:
                    LightComponentAngle = 0;
                    if(callback) F.SendMessageToObjectsOfType<MonoBehaviour>("OnDawnTime");
                    break;

                case TimeState.Noon:
                    LightComponentAngle = 90;
                    if (callback) F.SendMessageToObjectsOfType<MonoBehaviour>("OnNoonTime");
                    break;

                case TimeState.Dusk:
                    LightComponentAngle = 180;
                    if (callback) F.SendMessageToObjectsOfType<MonoBehaviour>("OnDuskTime");
                    break;

                case TimeState.Midnight:
                    LightComponentAngle = 270;
                    if (callback) F.SendMessageToObjectsOfType<MonoBehaviour>("OnMidnightTime");
                    break;
            }

            AdaptNewAngle();

            Debug.LogFormat("Time set to {0}! ({1})", time.ToString(), tickCounter);
        }

        private static void AdaptNewAngle()
        {
            tickCounter = dayCount * CompleteTickCycle + LightComponentAngle * CompleteTickCycle / 360;
        }
    }
}