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

		[SerializeField] private Vector3 m_respawnPosition;
		public Vector3 RespawnPosition { get { return m_respawnPosition; } set { m_respawnPosition = value; } }

		[SerializeField] private Vector3 m_respawnRotation;
		public Vector3 RespawnRotation { get { return m_respawnRotation; } set { m_respawnRotation = value; } }
		
		[SerializeField] private bool m_isBroken;
		public bool IsBroken { get { return m_isBroken; } set { m_isBroken = value; } }

		public bool m_respawned = true;

		private void Awake()
		{
			
		}

		public void Respawn()
		{
			Quaternion quaternion = Quaternion.identity;
			quaternion.eulerAngles = RespawnRotation;
			transform.SetPositionAndRotation(RespawnPosition, quaternion);
			gameObject.GetComponent<MeshRenderer>().enabled = true;
			gameObject.GetComponent<Rigidbody>().isKinematic = false;
			gameObject.GetComponent<Rigidbody>().detectCollisions = true;
			gameObject.GetComponent<Damageable>().ResetHealth();
			transform.Find("Collision").gameObject.SetActive(true);
			IsBroken = false;
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
			if (IsBroken)
				return;

			IsBroken = true;
			if (BreakEffect)
			{
 				BreakEffect.Emit(Random.Range(10,20));
			}

			if (DynamicObjectsManager.Instance.RespawnTime > 0)
			{
   				gameObject.GetComponent<MeshRenderer>().enabled = false;
				transform.Find("Collision").gameObject.SetActive(false);
				gameObject.GetComponent<Rigidbody>().isKinematic = true;
				gameObject.GetComponent<Rigidbody>().detectCollisions = false;
				gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
				gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				Invoke("Respawn", DynamicObjectsManager.Instance.RespawnTime);
			}
			else
				Respawn();
		}

		void OnCollisionEnter(Collision collision)
		{
			if (!DynamicObjectProperties.canBeCrashedByPed)
				return;

			Quaternion quaternion = Quaternion.identity;
			quaternion.eulerAngles = RespawnRotation;

			if (collision.impulse.sqrMagnitude > DynamicObjectProperties.breakImpulse)
			{
           		Break();
			}
		}
	}

}
