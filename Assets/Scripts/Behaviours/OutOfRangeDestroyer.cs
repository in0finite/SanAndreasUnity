using UnityEngine;

namespace SanAndreasUnity
{

    public class OutOfRangeDestroyer : MonoBehaviour
    {
        public float timeUntilDestroyed = 5;
        public float range = 250;
        public Transform targetObject = null;

        private float timeSinceOutOfRange = 0;

        private void Start()
        {
            if (targetObject == null)
            {
                if (Camera.main != null)
                    targetObject = Camera.main.transform;
            }
        }

        private void Update()
        {
            
            if (targetObject == null)
            {
                if (Camera.main != null)
                    targetObject = Camera.main.transform;
            }

            if (targetObject != null)
            {
                // only increase time if target object exists
                timeSinceOutOfRange += Time.deltaTime;

                float distanceSq = (transform.position - targetObject.position).sqrMagnitude;
                if (distanceSq <= range * range)
                {
                    timeSinceOutOfRange = 0;
                }
            }

            if (timeSinceOutOfRange >= timeUntilDestroyed)
            {
                Destroy(gameObject);
            }
        }
    }
}
