using UnityEngine;
using UGameCore.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours.Peds.States
{

	/// <summary>
	/// Base class for all states that are scripts.
	/// </summary>
	public abstract class BaseScriptState : MonoBehaviour, IPedState
	{

		public object ParameterForEnteringState { get; set; }

		protected Ped m_ped;
		protected PedModel m_model { get { return m_ped.PlayerModel; } }
	//	protected StateMachine m_stateMachine;
		protected new Transform transform { get { return m_ped.transform; } }
		public bool IsActiveState { get { return m_ped.CurrentState == this; } }
		protected bool m_isServer { get { return Net.NetStatus.IsServer; } }
		protected bool m_isClientOnly => Net.NetStatus.IsClientOnly;
		protected bool m_shouldSendButtonEvents { get { return !m_isServer && m_ped.IsControlledByLocalPlayer; } }

		public double LastTimeWhenActivated { get; set; } = 0;
		public double TimeSinceActivated => Time.timeAsDouble - this.LastTimeWhenActivated;
		public double LastTimeWhenDeactivated { get; set; } = 0;
		public double TimeSinceDeactivated => Time.timeAsDouble - this.LastTimeWhenDeactivated;



		protected virtual void Awake ()
		{
			m_ped = this.GetComponentInParent<Ped> ();
		}

		protected virtual void OnEnable ()
		{
			
		}

		protected virtual void OnDisable ()
		{
			
		}

		protected virtual void Start ()
		{

		}

		public virtual void OnBecameActive ()
		{
			
		}

		public virtual void OnBecameInactive ()
		{
			
		}

		public virtual bool RepresentsState (System.Type type)
		{
			var myType = this.GetType ();
			return myType.Equals (type) || myType.IsSubclassOf (type);
		}

		public bool RepresentsState<T> () where T : IState
		{
			return this.RepresentsState (typeof(T));
		}

		public virtual void UpdateState() {

			this.ConstrainPosition();
			this.ConstrainRotation();
			
		}

		public virtual void PostUpdateState()
		{
			
		}

		public virtual void LateUpdateState()
		{

			if (m_ped.Camera)
				this.UpdateCamera ();
			
			if (m_ped.shouldPlayAnims)
				this.UpdateAnims ();
			
		}

		public virtual void FixedUpdateState()
		{

			this.UpdateHeading();
			this.UpdateRotation();
			this.UpdateMovement();

		}

		protected virtual void ConstrainPosition()
		{
			if (m_isServer)
				m_ped.ConstrainPosition();
		}

		protected virtual void ConstrainRotation ()
		{
			if (m_isServer)
				m_ped.ConstrainRotation();
		}

		protected virtual void UpdateHeading()
		{
			if (m_isServer)
				m_ped.UpdateHeading ();
		}

		protected virtual void UpdateRotation()
		{
			if (m_isServer)
				m_ped.UpdateRotation ();
		}

		protected virtual void UpdateMovement()
		{
			if (m_isServer)
				m_ped.UpdateMovement ();
		}

		public virtual void UpdateCamera()
		{
			this.RotateCamera();
			this.UpdateCameraZoom();
			this.CheckCameraCollision ();
		}

		public virtual void RotateCamera()
		{
			BaseScriptState.RotateCamera(m_ped, m_ped.MouseMoveInput, m_ped.CameraClampValue.y);
		}

		public static void RotateCamera(Ped ped, Vector2 mouseDelta, float xAxisClampValue)
		{
			Camera cam = ped.Camera;

			if (mouseDelta.sqrMagnitude < float.Epsilon)
				return;

		//	cam.transform.Rotate( new Vector3(-mouseDelta.y, mouseDelta.x, 0f), Space.World );
			var eulers = cam.transform.eulerAngles;
		//	eulers.z = 0f;
			eulers.x += - mouseDelta.y;
			eulers.y += mouseDelta.x;
			// adjust x
			if (eulers.x > 180f)
				eulers.x -= 360f;
			// clamp
			if (xAxisClampValue > 0)
				eulers.x = Mathf.Clamp(eulers.x, -xAxisClampValue, xAxisClampValue);

			cam.transform.rotation = Quaternion.AngleAxis(eulers.y, Vector3.up)
				* Quaternion.AngleAxis(eulers.x, Vector3.right);
			
		}

		public virtual Vector3 GetCameraFocusPos()
		{
			return m_ped.transform.position + Vector3.up * 0.5f;
		}

		public virtual float GetCameraDistance()
		{
			return m_ped.CameraDistance;
		}

		public virtual void UpdateCameraZoom()
		{
			m_ped.CameraDistance = Mathf.Clamp(m_ped.CameraDistance - m_ped.MouseScrollInput.y, PedManager.Instance.minCameraDistanceFromPed, 
				PedManager.Instance.maxCameraDistanceFromPed);
		}

		public virtual void CheckCameraCollision()
		{
			BaseScriptState.CheckCameraCollision (m_ped, this.GetCameraFocusPos (), -m_ped.Camera.transform.forward, 
				this.GetCameraDistance ());
		}

		public static void CheckCameraCollision(Ped ped, Vector3 castFrom, Vector3 castDir, float cameraDistance)
		{
			
			// cast a ray from ped to camera to see if it hits anything
			// if so, then move the camera to hit point

			Camera cam = ped.Camera;

			float distance = cameraDistance;
			var castRay = new Ray(castFrom, castDir);
			RaycastHit hitInfo;
			int ignoredLayerMask = PedManager.Instance.cameraRaycastIgnoredLayerMask;

			if (Physics.SphereCast(castRay, 0.25f, out hitInfo, distance, ~ ignoredLayerMask))
			{
				distance = hitInfo.distance;
			}

			cam.transform.position = castRay.GetPoint(distance);

		}

		protected virtual void UpdateAnims()
		{
			
		}


		public virtual void OnFireButtonPressed()
		{
			if (m_shouldSendButtonEvents)
				PedSync.Local.OnFireButtonPressed();
		}

		public virtual void OnAimButtonPressed()
		{
			if (m_shouldSendButtonEvents)
				PedSync.Local.OnAimButtonPressed();
		}

		public virtual void OnSubmitPressed()
		{
			if (m_shouldSendButtonEvents)
				PedSync.Local.OnSubmitButtonPressed();
		}

		public virtual void OnJumpPressed()
		{

		}

		public virtual void OnCrouchButtonPressed()
		{
			if (m_shouldSendButtonEvents)
				PedSync.Local.OnCrouchButtonPressed();
		}

		public virtual void OnNextWeaponButtonPressed()
		{
			if (m_isServer)
				m_ped.WeaponHolder.SwitchWeapon (true);
			else if (m_shouldSendButtonEvents)
				PedSync.Local.OnNextWeaponButtonPressed();
		}

		public virtual void OnPreviousWeaponButtonPressed()
		{
			if (m_isServer)
				m_ped.WeaponHolder.SwitchWeapon (false);
			else if (m_shouldSendButtonEvents)
				PedSync.Local.OnPreviousWeaponButtonPressed();
		}

		public virtual void OnButtonPressed(string buttonName)
		{
			if (m_isServer)
				this.OnButtonPressedOnServer(buttonName);
			else if (m_shouldSendButtonEvents)
				PedSync.Local.OnButtonPressed(buttonName);
		}

		protected virtual void OnButtonPressedOnServer(string buttonName)
		{
			
		}

		public virtual void OnFlyButtonPressed()
		{

		}

		public virtual void OnFlyThroughButtonPressed()
		{

		}

		public virtual void OnSurrenderButtonPressed()
        {

        }


		public virtual void OnSwitchedStateByServer(byte[] data)
		{
			m_ped.SwitchState(this.GetType());
		}

		public virtual byte[] GetAdditionalNetworkData()
		{
			return null;
		}

		public virtual void OnChangedWeaponByServer(int newSlot)
		{
			m_ped.WeaponHolder.SwitchWeapon(newSlot);
		}

		public virtual void OnWeaponFiredFromServer(Weapon weapon, Vector3 firePos)
		{
			// if (m_ped.IsControlledByLocalPlayer)
			// 	return;

			// update gun flash
			if (weapon.GunFlash != null)
				weapon.GunFlash.gameObject.SetActive (true);
			weapon.UpdateGunFlashRotation ();

            weapon.PlayFireSound();
		}

		public virtual Ped.DamageResult OnDamaged(DamageInfo damageInfo)
		{
			float amount = damageInfo.raycastHitTransform != null
				? m_model.GetAmountOfDamageForBone(damageInfo.raycastHitTransform, damageInfo.amount)
				: damageInfo.amount;

			amount *= PedManager.Instance.pedDamageMultiplier;

			m_ped.Health -= amount;

			if (m_ped.Health <= 0)
			{
				m_ped.KillingDamageInfo = damageInfo;
				m_ped.Kill();
			}

			// notify clients
			m_ped.SendDamagedEventToClients(damageInfo, amount);

			return new Ped.DamageResult(amount);
		}

		public virtual void KillPed()
		{
			if (m_model != null)
			{
				m_model.DetachRagdoll(m_ped.KillingDamageInfo);
			}

			Object.Destroy(m_ped.gameObject);
		}

	}

}
