using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SanAndreasUnity.Behaviours.World
{
    class DynamicObject : StaticGeometry
    {
        #region Variables

        Vector3 m_startPosition;
        Quaternion m_startRotation;
        float m_lastMoved;
        bool m_respawned = true;
        bool m_savePositionAndRotationEstablished = false;
        BreakableObject m_breakableObject;
        Damageable m_damageable;

        #endregion

        #region Methods
        private void Start()
        {
            m_breakableObject = gameObject.AddComponent<BreakableObject>();
            m_damageable = gameObject.AddComponent<Damageable>();
            m_damageable.OnDamage.AddListener(OnDamage);
        }

        void OnDamage()
        {
            if (m_breakableObject.IsBroken)
                return;

            DamageInfo damageInfo = m_damageable.LastDamageInfo;
            //m_breakableObject.Health -= damageInfo.amount;
            if(m_breakableObject.Health > 0.0f)
            {
                m_breakableObject.PlayWeaponDamageEffect();
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                if (rb.IsSleeping())
                    rb.WakeUp();
                rb.AddForceAtPosition(transform.position - damageInfo.hitPoint, -damageInfo.hitNormal * 10); // @TODO fix calculations
            }
            else
            {

                m_breakableObject.Break();
            }
        }

        protected override void OnLoad()
        {
            m_breakableObject.RespawnPosition = transform.position;
            m_breakableObject.RespawnRotation = transform.rotation.eulerAngles;

            m_breakableObject.BreakEffect = ParticleSystemManager.Instance.GetByNane("Debris");
            m_breakableObject.BreakEffect.transform.SetParent(transform, false);
            m_breakableObject.BreakEffect.transform.localPosition = Vector3.zero;
            m_breakableObject.BreakEffect.transform.localRotation = Quaternion.identity;
            m_breakableObject.BreakEffect.name += " breakEffect";
            m_breakableObject.WeaponDamageEffect = ParticleSystemManager.Instance.GetByNane("Debris");
            m_breakableObject.WeaponDamageEffect.transform.SetParent(transform, false);
            m_breakableObject.WeaponDamageEffect.transform.localPosition = Vector3.zero;
            m_breakableObject.WeaponDamageEffect.transform.localRotation = Quaternion.identity;
            m_breakableObject.WeaponDamageEffect.name += " weaponDamageEffect";
            m_breakableObject.CanBeCrashed = true;
            InvokeRepeating("CheckForRespawn", 1f, 1f);

            base.OnLoad();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            ParticleSystemRenderer renderer = m_breakableObject.WeaponDamageEffect.GetComponent<ParticleSystemRenderer>();
            renderer.material.mainTexture = transform.GetComponent<Renderer>().materials[0].mainTexture;
            renderer.material.mainTextureScale = new Vector2(.3f, .3f);
        }

        void OnCollisionStay(Collision collisionInfo)
        {
            m_lastMoved = Time.realtimeSinceStartup;
            m_respawned = false;
        }

        void Respawn()
        {
            transform.SetPositionAndRotation(m_startPosition, m_startRotation);
            m_respawned = true;
            GetComponent<Rigidbody>().Sleep();
        }
        void CheckForRespawn()
        {
            if (m_respawned)
                return;

            // if fall below map
            if(transform.position.y < -100.0f)
            {
                Respawn();
            }

            if (m_lastMoved + 5.0f < Time.realtimeSinceStartup)
            {
                if (m_savePositionAndRotationEstablished)
                {
                    Respawn();
                }
                else
                {
                    m_startPosition = transform.position;
                    m_startRotation = transform.rotation;
                    m_breakableObject.RespawnPosition = m_startPosition;
                    m_breakableObject.RespawnRotation = m_startRotation.eulerAngles;
                    m_savePositionAndRotationEstablished = true;
                }
            }
        }

        public static DynamicObject CreateDynamic()
        {
            GameObject gameObject = new GameObject();
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.Sleep();
            rb.mass = 10.0f;
            DynamicObject dynamicObject = gameObject.AddComponent<DynamicObject>();
            gameObject.layer = LayerMask.NameToLayer("DynamicObject");
            return dynamicObject;
        }
        #endregion
    }
}
