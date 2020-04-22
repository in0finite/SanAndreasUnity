using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SanAndreasUnity.Behaviours
{
	public class BreakableObject : MonoBehaviour
	{
		[SerializeField] private DynamicObjectProperties m_dynamicObjectProperties;
		public DynamicObjectProperties DynamicObjectProperties { get { return m_dynamicObjectProperties; } set { m_dynamicObjectProperties = value; } }

		[Tooltip("Effect created after object got break.")]
		[SerializeField] private ParticleSystem m_breakEffect = null;
		public ParticleSystem BreakEffect { get { return m_breakEffect; } set { m_breakEffect = value; } }
		
		[Tooltip("Effect created after object get shoot by weapon.")]
		[SerializeField] private ParticleSystem m_weaponDamageEffect = null;
		public ParticleSystem WeaponDamageEffect { get { return m_weaponDamageEffect; } set { m_weaponDamageEffect = value; } }

		private void Awake()
		{
			
		}

		public void PlayWeaponDamageEffect()
		{
			if(WeaponDamageEffect)
			{
				WeaponDamageEffect.Emit(Random.Range(7, 12));
			}
		}

		public void Break()
		{
			if (BreakEffect)
			{
 				BreakEffect.Emit(Random.Range(10,20));
			}

   			gameObject.GetComponent<MeshRenderer>().enabled = false;
			transform.Find("Collision").gameObject.SetActive(false);
			gameObject.GetComponent<Rigidbody>().isKinematic = true;
			gameObject.GetComponent<Rigidbody>().detectCollisions = false;
			gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
			gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			Destroy(this, 1);
		}

		void OnCollisionEnter(Collision collision)
		{
			if (!DynamicObjectProperties.canBeCrashedByPed)
				return;

			if (collision.impulse.sqrMagnitude > DynamicObjectProperties.breakImpulse)
			{
           		Break();
			}
		}
	}

}
