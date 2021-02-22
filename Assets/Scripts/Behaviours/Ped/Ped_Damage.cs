using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours
{
	
	public partial class Ped {

		public Damageable Damageable { get; private set; }

		public float Health { get { return this.Damageable.Health; } set { this.Damageable.Health = value; } }
		[SerializeField] private float m_maxHealth = 100f;
		public float MaxHealth { get { return m_maxHealth; } set { m_maxHealth = value; } }

		public Bar HealthBar { get; private set; }

		/// <summary>
		/// Damage info that killed the ped.
		/// </summary>
		public DamageInfo KillingDamageInfo { get; set; }

		private bool m_alreadyKilled = false;

		public class DamageResult
		{
			public float DamageAmount { get; }

			public DamageResult()
			{
			}

			public DamageResult(float damageAmount)
			{
				DamageAmount = damageAmount;
			}
		}

		public static event System.Action<Ped, DamageInfo, DamageResult> onDamaged = delegate {};



		void AwakeForDamage ()
		{
			this.Damageable = this.GetComponentOrThrow<Damageable> ();

		}

		void StartForDamage ()
		{
			this.CreateHealthBar ();

		}

		void CreateHealthBar ()
		{
			this.HealthBar = Object.Instantiate (GameManager.Instance.barPrefab, this.transform).GetComponentOrThrow<Bar> ();
			//	this.HealthBar.SetBorderWidth (0.1f);
			this.HealthBar.BackgroundColor = PedManager.Instance.healthBackgroundColor;
			this.HealthBar.FillColor = PedManager.Instance.healthColor;
			this.HealthBar.BorderColor = Color.black;

			this.UpdateHealthBar ();
		}

		void UpdateDamageStuff ()
		{
			this.UpdateHealthBar ();
		}

		void UpdateHealthBar ()
		{
			bool shouldBeVisible = PedManager.Instance.displayHealthBarAbovePeds && !this.IsControlledByLocalPlayer;
			this.HealthBar.gameObject.SetActive (shouldBeVisible);

			if (shouldBeVisible)
			{
				this.HealthBar.BarSize = new Vector3 (PedManager.Instance.healthBarWorldWidth, PedManager.Instance.healthBarWorldHeight, 1.0f);
				this.HealthBar.SetFillPerc (this.Health / this.MaxHealth);
				this.HealthBar.transform.position = this.GetPosForHealthBar ();
				this.HealthBar.MaxHeightOnScreen = PedManager.Instance.healthBarMaxScreenHeight;
			}

		}

		public void DrawHealthBar ()
		{
			
			Vector3 pos = this.GetPosForHealthBar ();

			Rect rect = GUIUtils.GetRectForBarAsBillboard (pos, PedManager.Instance.healthBarWorldWidth, 
				PedManager.Instance.healthBarWorldHeight, Camera.main);

			// limit height
			rect.height = Mathf.Min (rect.height, PedManager.Instance.healthBarMaxScreenHeight);

			float borderWidth = Mathf.Min( 2f, rect.height / 4f );
			GUIUtils.DrawBar( rect, this.Health / this.MaxHealth, PedManager.Instance.healthColor, PedManager.Instance.healthBackgroundColor, borderWidth );

		}

		private Vector3 GetPosForHealthBar ()
		{
			if (null == this.PlayerModel.Head)
				return this.transform.position;

			Vector3 pos = this.PlayerModel.Head.position;
			pos += this.transform.up * PedManager.Instance.healthBarVerticalOffset;

			return pos;
		}

		public void OnDamaged()
		{
			if (!NetStatus.IsServer)
				return;

			if (this.Health <= 0)
				return;

			var damageInfo = this.Damageable.LastDamageInfo;

			var damageResult = this.CurrentState.OnDamaged(damageInfo);

			F.InvokeEventExceptionSafe(onDamaged, this, damageInfo, damageResult);
		}

		public void SendDamagedEventToClients(DamageInfo damageInfo, float damageAmount)
		{
			Ped attackingPed = damageInfo.attacker as Ped;

			PedSync.SendDamagedEvent(this.gameObject, attackingPed != null ? attackingPed.gameObject : null, damageAmount);
		}

		public void OnReceivedDamageEventFromServer(float damageAmount, Ped attackingPed)
		{
			if (attackingPed != null && attackingPed.IsControlledByLocalPlayer && attackingPed != this)
			{
				this.DisplayInflictedDamageMessage(damageAmount);
			}
		}

		public void DisplayInflictedDamageMessage(float damageAmount)
		{
			var msg = OnScreenMessageManager.Instance.CreateMessage();

			msg.velocity = Random.insideUnitCircle.normalized * PedManager.Instance.inflictedDamageMessageVelocityInScreenPerc;
			msg.TextColor = PedManager.Instance.inflictedDamageMessageColor;
			msg.timeLeft = PedManager.Instance.inflictedDamageMessageLifetime;
			msg.Text = Mathf.RoundToInt(damageAmount).ToString();
		}

		public void Kill()
		{
			F.RunExceptionSafe(this.KillInternal);
		}

		void KillInternal()
		{
			if (m_alreadyKilled)
				return;

			m_alreadyKilled = true;

			this.CurrentState.KillPed();
		}

	}

}
