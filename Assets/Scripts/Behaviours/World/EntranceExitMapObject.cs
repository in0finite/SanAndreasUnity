using UnityEngine;
using SanAndreasUnity.Importing.Items.Placements;
using SanAndreasUnity.Utilities;
using System.Collections.Generic;

namespace SanAndreasUnity.Behaviours.World
{

    public class EntranceExitMapObject : MapObject
    {
        static List<EntranceExitMapObject> s_allObjects = new List<EntranceExitMapObject>();
        public static IEnumerable<EntranceExitMapObject> AllObjects => s_allObjects;

        Coroutine m_animateArrowCoroutine;

        public float minArrowPos = -1f;
        public float maxArrowPos = 1f;
        public Transform arrowTransform;
        public float arrowMoveSpeed = 0.3f;



        public static EntranceExitMapObject Create(EntranceExit info)
        {
            var obj = Create<EntranceExitMapObject>(Cell.Instance.enexPrefab);
            obj.Initialize(info);
            return obj;
        }

        public EntranceExit Info { get; private set; }

        void Initialize(EntranceExit info)
        {
            Info = info;

            name = string.Format("ENEX ({0})", info.Name);

            float height = 2f;

            Initialize(info.EntrancePos + Vector3.up * height * 0.5f, Quaternion.identity);

            gameObject.SetActive(false);
            gameObject.isStatic = true;

            // collider
            var collider = gameObject.GetComponent<BoxCollider>();
            collider.size = new Vector3(info.Size.x, height, info.Size.y);
            collider.isTrigger = true;

            // need rigid body for detecting collisions
            var rb = gameObject.GetComponent<Rigidbody>();
            rb.mass = 0f;
            rb.isKinematic = true;

            this.SetDrawDistance(100f);

        }

        void OnEnable()
        {
            s_allObjects.Add(this);
            m_animateArrowCoroutine = this.StartCoroutine(this.AnimateArrow());
        }

        void OnDisable()
        {
            s_allObjects.Remove(this);
            
            if (m_animateArrowCoroutine != null)
                this.StopCoroutine(m_animateArrowCoroutine);
            m_animateArrowCoroutine = null;
        }


        void OnDrawGizmosSelected()
        {
            Utilities.F.HandlesDrawText(this.transform.position, this.name, Color.yellow);
        }

        protected override void OnLoad()
        {
            //Debug.LogFormat("OnLoad() - {0}", this.gameObject.name);
        }

        protected override void OnShow()
        {
            //Debug.LogFormat("OnShow() - {0}", this.gameObject.name);
            this.gameObject.SetActive(true);
        }

        void OnTriggerEnter(Collider collider)
        {
            //Debug.LogFormat("OnTriggerEnter() - with {0}", collider.gameObject.name);

            var ped = collider.gameObject.GetComponent<Ped>();
            if (ped != null)
                ped.OnStartCollidingWithEnex(this);
            
        }

        void OnTriggerExit(Collider collider)
        {
            //Debug.LogFormat("OnTriggerExit() - with {0}", collider.gameObject.name);

            var ped = collider.gameObject.GetComponent<Ped>();
            if (ped != null)
                ped.OnStopCollidingWithEnex(this);
            
        }

        System.Collections.IEnumerator AnimateArrow()
        {

            // place arrow at center
            float center = (this.minArrowPos + this.maxArrowPos) * 0.5f;
            this.arrowTransform.localPosition = this.arrowTransform.localPosition.WithY(center);

            yield return null;

            // move arrow up/down

            // set initial sign (direction of movement)
            float sign = Mathf.Sign(Random.Range(-1f, 1f));

            while (true)
            {
                float y = this.arrowTransform.localPosition.y;
                y += sign * this.arrowMoveSpeed * Time.deltaTime;
                if (y >= this.maxArrowPos || y <= this.minArrowPos)
                {
                    // clamp
                    y = Mathf.Clamp(y, this.minArrowPos, this.maxArrowPos);
                    // flip direction
                    sign = - sign;
                }
                this.arrowTransform.localPosition = this.arrowTransform.localPosition.WithY(y);
                yield return null;
            }

        }

    }

}
