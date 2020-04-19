using UnityEngine;
using UnityEngine.Events;

namespace SanAndreasUnity.Behaviours
{
	public class DamageInfo
	{
		public float amount = 0f;
		public object data = null;
		public Vector3 hitPoint;
		public Vector3 hitNormal;
	}

	public class Damageable : MonoBehaviour
	{
		[SerializeField] private UnityEvent m_onDamage = new UnityEvent ();
		public UnityEvent OnDamage { get { return m_onDamage; } set { m_onDamage = value; } }

		public DamageInfo LastDamageInfo { get; private set; }

		public void Damage (DamageInfo info)
		{
			if (info.amount > 0.0f)
			{
				LastDamageInfo = info;
				m_onDamage.Invoke();
			}
		}
	}

}
