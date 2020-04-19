using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SanAndreasUnity.Behaviours
{
	public class BreakableObject : MonoBehaviour
	{
		[Tooltip("Does strong enough collision impulse can cause object to break?")]
		[SerializeField] private bool m_canBeCrashed = false;
		public bool CanBeCrashed { get { return m_canBeCrashed; } set { m_canBeCrashed = value; } }

		[Tooltip("Do shooting cause object to move?")]
		[SerializeField] private bool m_affectShooting = false;
		public bool AffectShooting { get { return m_affectShooting; } set { m_affectShooting = value; } }

		[Tooltip("Minimum square impulse to break that object.")]
		[SerializeField] private float m_impulse = 1000.0f;
		public float Impulse { get { return m_impulse; } set { m_impulse = value; } }
		
		[SerializeField] private float m_health = 200.0f;
		public float Health { get { return m_health; } set { m_health = value; } }

		[SerializeField] private float m_maxHealth = 200.0f;
		public float MaxHealth { get { return m_maxHealth; } set { m_maxHealth = value; } }

		[Tooltip("Effect created after object got break.")]
		[SerializeField] private ParticleSystem m_breakEffect = null;
		public ParticleSystem BreakEffect { get { return m_breakEffect; } set { m_breakEffect = value; } }
		
		[Tooltip("Effect created after object get shoot by weapon.")]
		[SerializeField] private ParticleSystem m_weaponDamageEffect = null;
		public ParticleSystem WeaponDamageEffect { get { return m_weaponDamageEffect; } set { m_weaponDamageEffect = value; } }
		
		[Tooltip("After how many seconds, object will respawn.")]
		[SerializeField] private float m_respawnTime = 5.0f;
		public float RespawnTime { get { return m_respawnTime; } set { m_respawnTime = value; } }
		[SerializeField] private Vector3 m_respawnPosition;
		public Vector3 RespawnPosition { get { return m_respawnPosition; } set { m_respawnPosition = value; } }

		[SerializeField] private Vector3 m_respawnRotation;
		public Vector3 RespawnRotation { get { return m_respawnRotation; } set { m_respawnRotation = value; } }
		
		[SerializeField] private bool m_isBroken;
		public bool IsBroken { get { return m_isBroken; } set { m_isBroken = value; } }

		private void Awake()
		{
			
		}

		void Respawn()
		{
			Quaternion quaternion = Quaternion.identity;
			quaternion.eulerAngles = RespawnRotation;
			transform.SetPositionAndRotation(RespawnPosition, quaternion);
			gameObject.GetComponent<MeshRenderer>().enabled = true;
			gameObject.GetComponent<Rigidbody>().isKinematic = true;
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

			if (m_respawnTime > 0)
			{
   				gameObject.GetComponent<MeshRenderer>().enabled = false;
				transform.Find("Collision").gameObject.SetActive(false);
				gameObject.GetComponent<Rigidbody>().isKinematic = false;
				gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
				gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
				Invoke("Respawn", m_respawnTime);
			}
			else
				Respawn();
		}

		void OnCollisionEnter(Collision collision)
		{
			if (!CanBeCrashed)
				return;

			Quaternion quaternion = Quaternion.identity;
			quaternion.eulerAngles = RespawnRotation;

			if (collision.impulse.sqrMagnitude > Impulse)
			{
           		Break();
			}
		}
	}

}
