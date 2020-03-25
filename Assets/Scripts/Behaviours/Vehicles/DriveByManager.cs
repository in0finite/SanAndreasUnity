using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{

    public class DriveByManager : MonoBehaviour
    {
        public static DriveByManager Instance { get; private set; }

        public float cameraHeightOffset = 1f;
        public float cameraBackwardOffset = 1f;



        void Awake()
        {
            Instance = this;
        }

    }

}
