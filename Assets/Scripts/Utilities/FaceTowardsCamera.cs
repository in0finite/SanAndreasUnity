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
                Vector3 f = -cam.transform.forward;
                for (int i = 0; i < this.transformsToFace.Length; i++)
                {
                    this.transformsToFace[i].forward = f;
                }
            }
        }
    }
}
