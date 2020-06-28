using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SanAndreasUnity.Utilities
{

    public class ExplosionForce : MonoBehaviour
    {
        public float explosionForce = 4;
        public float upwardsModifier = 1f;
        public float radius = 10f;
        public float explosionMultiplier = 1f;


        private IEnumerator Start()
        {
            // wait one frame because some explosions instantiate debris which should then
            // be pushed by physics force
            yield return null;

            float multiplier = this.explosionMultiplier;

            float r = radius * multiplier;
            var cols = Physics.OverlapSphere(this.transform.position, r);

            var rigidbodies = new Dictionary<Rigidbody, List<Collider>>();
            foreach (var col in cols)
            {
                if (col.attachedRigidbody != null)
                {
                    if (rigidbodies.ContainsKey(col.attachedRigidbody))
                    {
                        rigidbodies[col.attachedRigidbody].Add(col);
                    }
                    else
                    {
                        rigidbodies.Add(col.attachedRigidbody, new List<Collider>() { col });
                    }
                }
            }

            foreach (var pair in rigidbodies)
            {
                Rigidbody rb = pair.Key;
                var colliders = pair.Value;

                // apply higher force on objects with higher mass
                float massFactor = Mathf.Pow(rb.mass, 0.95f);

                foreach (var collider in colliders)
                {
                    Vector3 closestPointOnCollider = collider.ClosestPointOrBoundsCenter(this.transform.position);

                    Vector3 diff = closestPointOnCollider - this.transform.position;
                    float distance = diff.magnitude;
                    float distanceFactor = Mathf.Sqrt(1.0f - Mathf.Clamp01(distance / r));

                    rb.AddForceAtPosition((diff.normalized * explosionForce + Vector3.up * upwardsModifier) * multiplier * distanceFactor * massFactor / colliders.Count, closestPointOnCollider, ForceMode.Impulse);

                }

            }

        }
    }

}
