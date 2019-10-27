using UnityEngine;
using SanAndreasUnity.Importing.Items.Placements;

namespace SanAndreasUnity.Behaviours.World
{

    public class EntranceExitMapObject : MapObject
    {
        
        public static EntranceExitMapObject Create(EntranceExit info)
        {
            var obj = Object.Instantiate(Cell.Instance.enexPrefab).GetComponent<EntranceExitMapObject>();
            obj.Initialize(info);
            return obj;
        }

        public EntranceExit Info { get; private set; }

        void Initialize(EntranceExit info)
        {
            Info = info;

            name = string.Format("ENEX ({0})", info.Name);

            Initialize(info.EntrancePos, Quaternion.identity);

            gameObject.SetActive(false);
            gameObject.isStatic = true;

            // collider
            var collider = gameObject.GetComponent<BoxCollider>();
            collider.size = new Vector3(info.Size.x, 2f, info.Size.y);
            collider.isTrigger = true;

            // need rigid body for detecting collisions
            var rb = gameObject.GetComponent<Rigidbody>();
            rb.mass = 0f;
            rb.isKinematic = true;

        }

        void OnDrawGizmosSelected()
        {
            Utilities.F.HandlesDrawText(this.transform.position, this.name, Color.yellow);
        }

        protected override float OnRefreshLoadOrder(Vector3 from)
        {
            
            float dist = Vector3.Distance(from, this.transform.position);
            if (dist > 100f)
            {
                this.gameObject.SetActive(false);
                return float.PositiveInfinity;
            }

            if (this.HasLoaded)
                return float.PositiveInfinity;

            return dist * dist;
        }

        protected override void OnLoad()
        {
            Debug.LogFormat("OnLoad() - {0}", this.gameObject.name);
        }

        protected override void OnShow()
        {
            Debug.LogFormat("OnShow() - {0}", this.gameObject.name);
            this.gameObject.SetActive(true);
        }

        void OnTriggerEnter(Collider collider)
        {
            Debug.LogFormat("OnTriggerEnter() - with {0}", collider.gameObject.name);
        }

    }

}
