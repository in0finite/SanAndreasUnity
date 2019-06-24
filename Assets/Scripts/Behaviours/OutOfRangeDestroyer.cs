using UnityEngine;
using SanAndreasUnity.Utilities;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{

    public class OutOfRangeDestroyer : MonoBehaviour
    {
        public float timeUntilDestroyed = 5;
        public float range = 250;
        public Transform targetObject = null;

        private float timeSinceOutOfRange = 0;

        void Start()
        {
            if (NetUtils.IsServer)
                this.StartCoroutine(this.DestroyCoroutine());
        }

        System.Collections.IEnumerator DestroyCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);

                // obtain focus points
                var focusPoints = Net.Player.AllPlayersEnumerable.Where(p => p.OwnedPed != null).Select(p => p.OwnedPed.transform);
                if (Camera.main != null)
                    focusPoints = focusPoints.Append(Camera.main.transform);
                
                if (focusPoints.Count() < 1) {
                    // no focus points
                    // don't do anything

                } else {
                    // check if we are in range of any focus point
                    Vector3 thisPosition = this.transform.position;
                    bool isInRange = focusPoints.Any(point => point.Distance(thisPosition) < this.range);
                    if (isInRange) {
                        this.timeSinceOutOfRange = 0;
                    } else {
                        this.timeSinceOutOfRange += 1.0f;
                    }
                }

                if (this.timeSinceOutOfRange >= this.timeUntilDestroyed) {
                    // timeout expired
                    Destroy(this.gameObject);
                    break;
                }

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
