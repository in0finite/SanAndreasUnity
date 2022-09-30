using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    [DisallowMultipleComponent]
    [AddComponentMenu("RVP/Damage/Detachable Part", 1)]

    //Class for parts that can detach
    public class DetachablePart : MonoBehaviour
    {
        private Transform tr;
        private Rigidbody rb;
        private Rigidbody parentBody;
        private Transform initialParent;
        private Vector3 initialLocalPos;
        private Quaternion initialLocalRot;

        [System.NonSerialized]
        public HingeJoint hinge;

        [System.NonSerialized]
        public bool detached;

        [System.NonSerialized]
        public Vector3 initialPos;

        public float mass = 0.1f;
        public float drag;
        public float angularDrag = 0.05f;
        public float looseForce = -1;
        public float breakForce = 25;

        [Tooltip("A hinge joint is randomly chosen from the list to use")]
        public PartJoint[] joints;

        private Vector3 initialAnchor;

        [System.NonSerialized]
        public Vector3 displacedAnchor;

        private void Start()
        {
            tr = transform;

            if (tr.parent)
            {
                initialParent = tr.parent;
                initialLocalPos = tr.localPosition;
                initialLocalRot = tr.localRotation;
            }

            parentBody = (Rigidbody)tr.GetTopmostParentComponent<Rigidbody>();
            initialPos = tr.localPosition;
        }

        private void Update()
        {
            if (hinge)
            {
                //Destory hinge if displaced too far from original position
                if ((initialAnchor - displacedAnchor).sqrMagnitude > 0.1f)
                {
                    Destroy(hinge);
                }
            }
        }

        public void Detach(bool makeJoint)
        {
            if (!detached)
            {
                detached = true;
                tr.parent = null;
                rb = gameObject.AddComponent<Rigidbody>();
                rb.mass = mass;
                rb.drag = drag;
                rb.angularDrag = angularDrag;

                if (parentBody)
                {
                    parentBody.mass -= mass;
                    rb.velocity = parentBody.GetPointVelocity(tr.position);
                    rb.angularVelocity = parentBody.angularVelocity;

                    //Pick a random hinge joint to use
                    if (makeJoint && joints.Length > 0)
                    {
                        PartJoint chosenJoint = joints[Random.Range(0, joints.Length)];
                        initialAnchor = chosenJoint.hingeAnchor;
                        displacedAnchor = initialAnchor;

                        hinge = gameObject.AddComponent<HingeJoint>();
                        hinge.autoConfigureConnectedAnchor = false;
                        hinge.connectedBody = parentBody;
                        hinge.anchor = chosenJoint.hingeAnchor;
                        hinge.axis = chosenJoint.hingeAxis;
                        hinge.connectedAnchor = initialPos + chosenJoint.hingeAnchor;
                        hinge.enableCollision = false;
                        hinge.useLimits = chosenJoint.useLimits;

                        JointLimits limits = new JointLimits();
                        limits.min = chosenJoint.minLimit;
                        limits.max = chosenJoint.maxLimit;
                        limits.bounciness = chosenJoint.bounciness;
                        hinge.limits = limits;
                        hinge.useSpring = chosenJoint.useSpring;

                        JointSpring spring = new JointSpring();
                        spring.targetPosition = chosenJoint.springTargetPosition;
                        spring.spring = chosenJoint.springForce;
                        spring.damper = chosenJoint.springDamper;
                        hinge.spring = spring;
                        hinge.breakForce = breakForce;
                        hinge.breakTorque = breakForce;
                    }
                }
            }
        }

        public void Reattach()
        {
            if (detached)
            {
                detached = false;
                tr.parent = initialParent;
                tr.localPosition = initialLocalPos;
                tr.localRotation = initialLocalRot;

                if (parentBody)
                {
                    parentBody.mass += mass;
                }

                if (hinge)
                {
                    Destroy(hinge);
                }

                if (rb)
                {
                    Destroy(rb);
                }
            }
        }

        //Draw joint gizmos
        private void OnDrawGizmosSelected()
        {
            if (!tr)
            {
                tr = transform;
            }

            if (looseForce >= 0 && joints.Length > 0)
            {
                Gizmos.color = Color.red;
                foreach (PartJoint curJoint in joints)
                {
                    Gizmos.DrawRay(tr.TransformPoint(curJoint.hingeAnchor), tr.TransformDirection(curJoint.hingeAxis).normalized * 0.2f);
                    Gizmos.DrawWireSphere(tr.TransformPoint(curJoint.hingeAnchor), 0.02f);
                }
            }
        }
    }

    //Class for storing hinge joint information in the joints list
    [System.Serializable]
    public class PartJoint
    {
        public Vector3 hingeAnchor;
        public Vector3 hingeAxis = Vector3.right;
        public bool useLimits;
        public float minLimit;
        public float maxLimit;
        public float bounciness;
        public bool useSpring;
        public float springTargetPosition;
        public float springForce;
        public float springDamper;
    }
}