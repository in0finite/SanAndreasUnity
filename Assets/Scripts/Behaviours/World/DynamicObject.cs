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

        BreakableObject m_breakableObject;
        Damageable m_damageable;
        bool m_loaded = false;
        int m_model;

        #endregion

        #region Methods
        private void Start()
        {
            gameObject.AddComponent<Rigidbody>();
            m_breakableObject = gameObject.AddComponent<BreakableObject>();
            m_damageable = gameObject.AddComponent<Damageable>();
            m_damageable.OnDamage.AddListener(m_damageable.HandleDamageByDefault);
            m_damageable.OnDamage.AddListener(OnDamage);
            if(m_model != 0)
            {
                LoadModelProperties();
            }
        }

        void SelectModel(int model)
        {
            m_model = model;
        }

        void LoadModelProperties()
        {
            DynamicObjectProperties? properties = DynamicObjectsManager.Instance.GetModelProperties(m_model);
            if (properties.HasValue)
            {
                m_breakableObject.DynamicObjectProperties = properties.Value;
            }
            else // if non gta model get loaded
            {
                m_breakableObject.DynamicObjectProperties = new DynamicObjectProperties
                {
                    mass = 100,
                    breakImpulse = 10000,
                    canBeCrashedByPed = false,
                    canBeCrashedByVehicle = false,
                    canBeShooted = false,
                    health = 1000,
                };
            }
            m_damageable.Health = m_breakableObject.DynamicObjectProperties.health;
        }

        void OnDamage()
        {
            DamageInfo damageInfo = m_damageable.LastDamageInfo;

            if (m_breakableObject.DynamicObjectProperties.canBeShooted)
                m_damageable.Health -= damageInfo.amount;

            if(m_damageable.Health > 0.0f)
            {
                m_breakableObject.PlayWeaponDamageEffect();
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                if (rb.IsSleeping())
                    rb.WakeUp();
                rb.AddForceAtPosition(-damageInfo.hitNormal * 2, damageInfo.hitPoint - transform.position); // @TODO fix calculations
                //rb.AddForceAtPosition(damageInfo.hitPoint - transform.position , - damageInfo.hitNormal * 2);
            }
            else
            {
                m_breakableObject.Break();
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

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

            // set particles same texture as object it is attached to
            ParticleSystemRenderer renderer = m_breakableObject.WeaponDamageEffect.GetComponent<ParticleSystemRenderer>();
            renderer.material.mainTexture = transform.GetComponent<Renderer>().materials[0].mainTexture;
            renderer.material.mainTextureScale = new Vector2(.3f, .3f);
            renderer = m_breakableObject.BreakEffect.GetComponent<ParticleSystemRenderer>();
            renderer.material.mainTexture = transform.GetComponent<Renderer>().materials[0].mainTexture;
            renderer.material.mainTextureScale = new Vector2(.3f, .3f);
        }

        public static DynamicObject CreateDynamic(int objectId)
        {
            GameObject gameObject = new GameObject();
            DynamicObject dynamicObject = gameObject.AddComponent<DynamicObject>();
            gameObject.layer = LayerMask.NameToLayer("DynamicObject");
            dynamicObject.SelectModel(objectId);
            return dynamicObject;
        }
        #endregion
    }
}
