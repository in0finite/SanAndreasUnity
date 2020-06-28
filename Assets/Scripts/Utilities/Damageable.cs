using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SanAndreasUnity.Utilities
{

	public class DamageInfo
	{
		public float amount = 0f;
		public Transform raycastHitTransform = null;
		public object attacker = null;
		public object data = null;
	}

	public class Damageable : MonoBehaviour
	{

		[SerializeField] private float m_health = 0f;
		public float Health { get { return m_health; } set { m_health = value; } }

		[SerializeField] private UnityEvent m_onDamage = new UnityEvent ();
		public UnityEvent OnDamageEvent => m_onDamage;

		public DamageInfo LastDamageInfo { get; private set; }



		public void Damage (DamageInfo info)
		{
			this.LastDamageInfo = info;

			F.RunExceptionSafe(() => m_onDamage.Invoke());
		}

		public void HandleDamageByDefault ()
		{
			DamageInfo info = this.LastDamageInfo;

			this.Health -= info.amount;

			if (this.Health <= 0f) {
				Destroy (this.gameObject);
			}
		}

		public static void InflictDamageToObjectsInArea(
			Vector3 center, float radius, float damageAmount, AnimationCurve damageOverDistanceCurve)
		{
			Collider[] overlappingColliders = Physics.OverlapSphere(center, radius);

			var damagables = new Dictionary<Damageable, List<Collider>>();

			foreach (var collider in overlappingColliders)
			{
				var damagable = collider.GetComponentInParent<Damageable>();
				if (damagable != null)
				{
					if (damagables.ContainsKey(damagable))
					{
						damagables[damagable].Add(collider);
					}
					else
					{
						damagables.Add(damagable, new List<Collider>() { collider });
					}
				}
			}

			foreach (var pair in damagables)
			{
				Damageable damageable = pair.Key;
				List<Collider> colliders = pair.Value;

				// find closest point from all colliders

				float closestPointDistance = float.MaxValue;
				Collider closestPointCollider = null;

				foreach (var collider in pair.Value)
				{
					Vector3 closestPointOnCollider = collider.ClosestPointOrBoundsCenter(center);
					float distanceToPointOnCollider = Vector3.Distance(center, closestPointOnCollider);

					if (distanceToPointOnCollider < closestPointDistance)
					{
						closestPointDistance = distanceToPointOnCollider;
						closestPointCollider = collider;
					}

				}

				// apply damage based on closest distance

				float distance = closestPointDistance;
				float distanceFactor = damageOverDistanceCurve.Evaluate(Mathf.Clamp01(distance / radius));
				float damageAmountBasedOnDistance = damageAmount * distanceFactor;

				F.RunExceptionSafe(() => damageable.Damage(new DamageInfo() { amount = damageAmountBasedOnDistance }));
			}

		}

	}

}
