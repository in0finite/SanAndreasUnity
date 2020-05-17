using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SanAndreasUnity.Utilities
{

	public class DamageInfo
	{
		public float amount = 0f;
		public Transform raycastHitTransform = null;
		public object data = null;
	}

	public class Damageable : MonoBehaviour
	{

		[SerializeField] private float m_health = 0f;
		public float Health { get { return m_health; } set { m_health = value; } }

		[SerializeField] private UnityEvent m_onDamage = new UnityEvent ();

		public DamageInfo LastDamageInfo { get; private set; }



		public void Damage (DamageInfo info)
		{
			this.LastDamageInfo = info;
			m_onDamage.Invoke ();
		}

		public void HandleDamageByDefault ()
		{
			DamageInfo info = this.LastDamageInfo;

			this.Health -= info.amount;

			if (this.Health <= 0f) {
				Destroy (this.gameObject);
			}
		}

	}

}
