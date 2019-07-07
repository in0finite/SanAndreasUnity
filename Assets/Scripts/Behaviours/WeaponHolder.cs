using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Weapons;
using System.Linq;
using SanAndreasUnity.Utilities;
using SanAndreasUnity.Net;

namespace SanAndreasUnity.Behaviours {

	[DefaultExecutionOrder(-70)]
	public class WeaponHolder : MonoBehaviour {

		private	Ped	m_ped;
		public	PedModel	PlayerModel { get { return m_ped.PlayerModel; } }
		public	Camera	Camera { get { return m_ped.Camera; } }

		private	Weapon[]	weapons = new Weapon[(int)WeaponSlot.Count];
		public	Weapon[]	AllWeapons { get { return this.weapons.Where (w => w != null).ToArray (); } }

		private	int		currentWeaponSlot = 0;
		public	int		CurrentWeaponSlot { get { return this.currentWeaponSlot; } }
		public	bool	IsHoldingWeapon { get { return this.currentWeaponSlot > 0; } }

		public	bool	autoAddWeapon = false;


		#region Aiming

	//	public	bool	IsAimOn { get; set; }
	//	private	bool	m_isAiming = false;
		public	bool	IsAiming {
			get { return m_ped.CurrentState != null && m_ped.CurrentState is Peds.States.IAimState; }
//			private set {
//				if (value == m_isAiming)
//					return;
//				if (value) {
//					m_timeWhenStartedAiming = Time.time;
//					m_frameWhenStartedAiming = Time.frameCount;
//				}
//				m_isAiming = value;
//			}
		}
	//	private	float	m_timeWhenStartedAiming = 0f;
	//	public	float	TimeSinceStartedAiming { get { return Time.time - m_timeWhenStartedAiming; } }
	//	private	int		m_frameWhenStartedAiming = 0;
	//	public	int		NumFramesSinceStartedAiming { get { return Time.frameCount - m_frameWhenStartedAiming; } }

		public	Vector3	AimDirection {
			get {
				if (m_ped.IsControlledByLocalPlayer)
				{
					if (this.IsAiming && this.Camera != null)
						return this.Camera.transform.forward;
					else
						return this.transform.forward;
				}
				else
				{
					return m_ped.m_net_aimDir;
				}
			}
			set {
				m_ped.m_net_aimDir = value;
			}
		}

		//[SerializeField]	[Range(0,1)]	private	float	m_aimWithRifleMinAnimTime = 0.0f;
		//public	float	AimWithRifleMinAnimTime { get { return m_aimWithRifleMinAnimTime; } set { m_aimWithRifleMinAnimTime = value; } }

		[SerializeField]	[Range(0,4)]	private	float	m_aimWithRifleMaxAnimTime = 0.7f;
		public	float	AimWithRifleMaxAnimTime { get { return m_aimWithRifleMaxAnimTime; } set { m_aimWithRifleMaxAnimTime = value; } }

		//	[SerializeField]	[Range(0,1)]	private	float	m_aimWithArmMaxAnimTime = 1.0f;

		public	Vector3	cameraAimOffset = new Vector3 (0.7f, 0.2f, -1);

		#endregion

		#region Firing

		public	bool	IsFiring {
			get { return m_ped.CurrentState != null && m_ped.CurrentState is Peds.States.IFireState; }
		}
	//	public	bool	IsFireOn { get; set; }
	//	public	float	TimeWhenStartedFiring { get; private set; }
	//	public	float	TimeSinceStartedFiring { get { return Time.time - this.TimeWhenStartedFiring; } }

		#endregion


		public	Weapon	CurrentWeapon { get => this.weapons[this.currentWeaponSlot]; }
		private	Transform	CurrentWeaponTransform { get { return CurrentWeapon != null ? CurrentWeapon.transform : null; } }
        
		private	int		m_frameWhenSwitchedWeapon = 0;
		public	int		NumFramesSinceSwitchedWeapon { get { return Time.frameCount - m_frameWhenSwitchedWeapon; } }


		public Vector3 SpineOffset;

		public enum WeaponAttachType
		{
			None,
			RightHand,
			BothFingers
		}

		public	WeaponAttachType	weaponAttachType = WeaponAttachType.RightHand;


		public	bool	rotatePlayerInDirectionOfAiming = true;



        void Awake () {
			
			m_ped = this.GetComponent<Ped> ();

		}

		void OnDisable()
		{
			// destroy all weapons
			if (NetStatus.IsServer)
			{
				// we won't call RemoveAllWeapons() because it causes complications - it calls SwitchWeapon(), which
				// then calls StopFiring(), which then switches ped state, etc

				// no special reason for using AllWeapons
				foreach(var w in this.AllWeapons)
				{
					DestroyWeapon(w);
				}
			}
		}

		void Start ()
		{
			PlayerModel.onLateUpdate += this.UpdateWeaponTransform;

			if (NetStatus.IsServer)
			{
				if (this.autoAddWeapon)
				{
					this.AddRandomWeapons();
					this.SwitchWeapon(WeaponSlot.Pistol);
				}
			}
		}

		void Update () {

			if (!Loader.HasLoaded)
				return;


			//this.UpdateWeaponTransform ();

			if (CurrentWeapon != null) {
				CurrentWeapon.EnableOrDisableGunFlash ();
				CurrentWeapon.UpdateGunFlashRotation ();
			}

			// reload weapon ammo clip
			if (NetStatus.IsServer)
			{
				if (CurrentWeapon != null) {
					if (CurrentWeapon.AmmoClipSize > 0 && CurrentWeapon.AmmoInClip <= 0) {
						int amountToRefill = Mathf.Min (CurrentWeapon.AmmoClipSize, CurrentWeapon.AmmoOutsideOfClip);
						CurrentWeapon.AmmoInClip = amountToRefill;
						CurrentWeapon.AmmoOutsideOfClip -= amountToRefill;
					}
				}
			}

		}

		void LateUpdate_jdghrjgjr()
		{
			// all things that manipulate skeleton must be placed in LateUpdate(), because otherwise Animator will
			// override them

			// order of these functions is important


			UpdateAnims ();

		//	RotatePlayerInDirectionOfAiming ();

			// this should be done AFTER the player has rotated in direction of aiming
			RotateSpine ();

			// this should be done after all other skeleton changes
			// idk why this boolean has to be checked - there are some race conditions with animations
			// - if we don't check it, weapons will start shaking
		//	if (this.IsAiming)
			UpdateWeaponTransform ();

		}


		private void UpdateAnims ()
		{
			
		}

		private void RotateSpine ()
		{

		}

		public void UpdateWeaponTransform ()
		{

			// update transform of weapon
			if (CurrentWeaponTransform != null) {

				if (this.weaponAttachType == WeaponAttachType.BothFingers) {
					if (PlayerModel.RightFinger != null && PlayerModel.LeftFinger != null) {

						CurrentWeaponTransform.transform.position = PlayerModel.RightFinger.transform.position;

						Vector3 dir = (PlayerModel.LeftFinger.transform.position - PlayerModel.RightFinger.transform.position).normalized;
						Quaternion q = Quaternion.LookRotation (dir, transform.up);
						Vector3 upNow = q * Vector3.up;
						dir = Quaternion.AngleAxis (-90, upNow) * dir;
						CurrentWeaponTransform.transform.rotation = Quaternion.LookRotation (dir, transform.up);
					}
				} else if (this.weaponAttachType == WeaponAttachType.RightHand) {
					if (PlayerModel.RightHand != null) {

						Vector3 weaponPos = PlayerModel.RightHand.position;
						Transform rotationTr = PlayerModel.RightHand;

						// add aim offset
						//	var aimOffset = CurrentWeapon.GunAimingOffset;
						//	if (aimOffset != null)
						//		weaponPos += rotationTr.forward * aimOffset.aimZ + rotationTr.right * aimOffset.aimX;

						CurrentWeaponTransform.transform.position = weaponPos;
						CurrentWeaponTransform.transform.rotation = rotationTr.rotation;
					}
				}

			}

		}


		public void SwitchWeapon( bool next )
		{
			NetStatus.ThrowIfNotOnServer();

			if (currentWeaponSlot < 0)
				currentWeaponSlot = 0;

			int delta = next ? 1 : -1;

			for (int i = currentWeaponSlot + delta, count = 0;
				i != currentWeaponSlot && count < weapons.Length;
				i += delta, count++) {

				if (i < 0)
					i = weapons.Length - 1;
				if (i >= weapons.Length)
					i = 0;

				if ( (int)WeaponSlot.Hand == i || weapons [i] != null ) {
					// this is a hand slot or there is a weapon in this slot
					// switch to it
					SwitchWeapon (i);
					break;
				}
			}

		}

		public void SwitchWeapon (int slotIndex)
		{
			
			if (slotIndex < 0)
				slotIndex = 0;

			if (slotIndex == this.currentWeaponSlot)
				return;

			if (this.CurrentWeapon != null) {
				// hide the weapon
				HideWeapon( this.CurrentWeapon );
			}

			if (slotIndex >= 0) {
				
				//CurrentWeapon = weapons [slotIndex];
				var w = this.weapons [slotIndex];

				// show the weapon
				if (w != null)
					UnHideWeapon (w);

			} else {
				//CurrentWeapon = null;
			}

			this.currentWeaponSlot = slotIndex;

			m_frameWhenSwitchedWeapon = Time.frameCount;

			if (NetStatus.IsServer)
				m_ped.StopFiring ();

			this.UpdateWeaponTransform ();

		}

		private static void HideWeapon (Weapon weapon)
		{
			//weapon.gameObject.SetActive (false);

			foreach (var r in weapon.GetComponentsInChildren<Renderer>()) {
				r.enabled = false;
			}
		}

		private static void UnHideWeapon (Weapon weapon)
		{
			//weapon.gameObject.SetActive (true);

			foreach (var r in weapon.GetComponentsInChildren<Renderer>()) {
				r.enabled = true;
			}
		}

		public void AddWeapon(Weapon weapon)
		{

			int slotIndex = weapon.SlotIndex;

			if (this.weapons[slotIndex] == weapon)	// trying to add the weapon that already exists
				return;

			// destroy current weapon at this slot
			if (NetStatus.IsServer)
			{
				if (this.weapons [slotIndex] != null) {
					DestroyWeapon (this.weapons [slotIndex]);
				}
			}
			
			this.weapons [slotIndex] = weapon;

			weapon.PedOwner = m_ped;

			if (slotIndex == this.currentWeaponSlot) {
				// update current weapon variable
				//CurrentWeapon = weapons [slotIndex];

				// update it's transform
				this.UpdateWeaponTransform ();
			} else {
				// hide the newly added weapon
				HideWeapon (weapon);
			}

		}

		public void SetWeaponAtSlot (Importing.Items.Definitions.WeaponDef weaponDef, int slot)
		{
			this.SetWeaponAtSlot (weaponDef.Id, slot);
		}

		public void SetWeaponAtSlot (int weaponId, int slotIndex)
		{
			NetStatus.ThrowIfNotOnServer();

			Weapon.Create (weaponId, m_ped);
		}

		public Weapon GetWeaponAtSlot (int slotIndex)
		{
			return this.weapons [slotIndex];
		}

		public void RemoveAllWeapons() {

			if (!NetStatus.IsServer)
				return;

			this.SwitchWeapon (-1);

			for (int i = 0; i < this.weapons.Length; i++) {
				if (this.weapons [i] != null)
					DestroyWeapon (this.weapons [i]);
				this.weapons [i] = null;
			}

		}

		private static void DestroyWeapon (Weapon w)
		{
			Destroy (w.gameObject);
			w.PedOwner = null;
		}


		public void AddRandomWeapons ()
		{
			if (!NetStatus.IsServer)
				return;

			int[] slots = new int[] { WeaponSlot.Pistol, WeaponSlot.Shotgun, WeaponSlot.Submachine,
				WeaponSlot.Machine, WeaponSlot.Rifle, WeaponSlot.Heavy
			};

			var groups = WeaponData.LoadedWeaponsData.Where( wd => slots.Contains( wd.weaponslot ) )
				.DistinctBy( wd => wd.weaponType )
				.GroupBy( wd => wd.weaponslot );

			foreach (var grp in groups) {

				int count = grp.Count ();
				if (count < 1)
					continue;

				int index = Random.Range (0, count);
				WeaponData chosenWeaponData = grp.ElementAt (index);

				this.SetWeaponAtSlot (chosenWeaponData.modelId1, grp.Key);

				// add some ammo
				Weapon weapon = this.GetWeaponAtSlot( grp.Key );
				AddRandomAmmoAmountToWeapon (weapon);
			}

		}

		public static void AddRandomAmmoAmountToWeapon (Weapon weapon)
		{
			if (!NetStatus.IsServer)
				return;
			
			weapon.AmmoInClip = weapon.AmmoClipSize;
			weapon.AmmoOutsideOfClip += weapon.AmmoClipSize * Random.Range( 0, 11 );
			weapon.AmmoOutsideOfClip += Random.Range (50, 200);
		}


		void OnDrawGizmosSelected ()
		{
			// draw gizmos for current weapon

			if (CurrentWeapon != null) {
				CurrentWeapon.OnDrawGizmosSelected ();
			}

			// draw line from camera
			F.GizmosDrawLineFromCamera ();

		}

	}

}
