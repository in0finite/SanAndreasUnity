using UnityEngine;
using UGameCore.Utilities;
using System.Linq;

namespace SanAndreasUnity.Behaviours
{

    public class OutOfRangeDestroyer : MonoBehaviour
    {
        public float timeUntilDestroyed = 5;
        public float range = 250;

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
                
                // check if we are in range of any focus point
                Vector3 thisPosition = this.transform.position;
                bool isInRange = focusPoints.Any(point => point.Distance(thisPosition) < this.range);
                if (isInRange) {
                    this.timeSinceOutOfRange = 0;
                } else {
                    this.timeSinceOutOfRange += 1.0f;
                }
                
                if (this.timeSinceOutOfRange >= this.timeUntilDestroyed) {
                    // timeout expired
                    Destroy(this.gameObject);
                    break;
                }

            }
        }

    }
}
