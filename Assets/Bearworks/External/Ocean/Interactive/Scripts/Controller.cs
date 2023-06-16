using UnityEngine;
using System.Collections;

namespace NOcean
{
    [DisallowMultipleComponent]
	[RequireComponent(typeof(Rigidbody))]
	public class Controller : MonoBehaviour
    {
        public float engineForce = 2f;
        public float engineTorque = 2f;
        
        [HideInInspector]
        public Rigidbody rig = null;

        [HideInInspector]
        public WaypointProgressTracker traker = null;

        private int _guid; // GUID for the height system

        // Use this for initialization
        public virtual void Start()
	    {
            rig = GetComponent<Rigidbody>();

            traker = GetComponent<WaypointProgressTracker>();

            _guid = gameObject.GetInstanceID();
        }

		// Update is called once per frame
        public void FixedUpdate()
	    {
            float trakerDamp = 1.0f;
            float trakerPlus = 0.0f;
            if (traker != null)
            {

                Vector2 thisV2 = new Vector2(transform.forward.x, transform.forward.z);
                Vector3 deltaV3 = (traker.target.position - transform.position).normalized;
                Vector2 deltaV2 = new Vector2(deltaV3.x, deltaV3.z);
                trakerPlus = Vector2.Angle(thisV2, deltaV2);
                trakerPlus /= 180f;

                trakerDamp = 1 - trakerPlus;
                trakerDamp *= trakerDamp;

                Vector3 cross = Vector3.Cross(thisV2, deltaV2);

                if (cross.z > 0)
                    trakerPlus = -trakerPlus;
                
            }

            if (rig != null)
            { 
                if (rig.mass > 0.01f)
                {
                    rig.AddForce(engineForce * new Vector3(transform.forward.x, 0, transform.forward.z) * trakerDamp);
                    rig.AddTorque(engineTorque * trakerPlus * transform.up);
                }
	        }
        }
	}
}
