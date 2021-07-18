using UnityEngine;

namespace SanAndreasUnity.Utilities
{
    public class FaceTowardsCamera : MonoBehaviour
    {
        public Transform[] transformsToFace;

        void Update()
        {
            var cam = Camera.current;
            if (cam != null)
            {
                Quaternion quaternion = Quaternion.LookRotation(-cam.transform.forward, cam.transform.up);
                for (int i = 0; i < this.transformsToFace.Length; i++)
                {
                    this.transformsToFace[i].rotation = quaternion;
                }
            }
        }
    }
}
