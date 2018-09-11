using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Weapons;
using System.Linq;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours {
	
	public class WeaponHolder : MonoBehaviour {

		private	Ped	m_ped;
		public	PedModel	PlayerModel { get { return m_ped.PlayerModel; } }

		private	Weapon[]	weapons = new Weapon[(int)WeaponSlot.Count];
		public	Weapon[]	AllWeapons { get { return this.weapons.Where (w => w != null).ToArray (); } }

		private	int		currentWeaponSlot = -1;
		public	int		CurrentWeaponSlot { get { return this.currentWeaponSlot; } }
		public	bool	IsHoldingWeapon { get { return this.currentWeaponSlot > 0; } }

		public	bool	autoAddWeapon = false;

		public	bool	IsAimOn { get; set; }
		private	bool	m_isAiming = false;
		public	bool	IsAiming {
			get { return m_isAiming; }
			private set {
				if (value == m_isAiming)
					return;
				if (value) {
					m_timeWhenStartedAiming = Time.time;
					m_frameWhenStartedAiming = Time.frameCount;
				}
				m_isAiming = value;
			}
		}
		private	float	m_timeWhenStartedAiming = 0f;
		public	float	TimeSinceStartedAiming { get { return Time.time - m_timeWhenStartedAiming; } }
		private	int		m_frameWhenStartedAiming = 0;
		public	int		NumFramesSinceStartedAiming { get { return Time.frameCount - m_frameWhenStartedAiming; } }

		public	Vector3	AimDirection {
			get {
				if (this.IsAiming && Camera.main != null)
					return Camera.main.transform.forward;
				else
					return this.transform.forward;
			}
		}

		private	bool	m_isFiring = false;
		public	bool	IsFiring {
			get { return m_isFiring; }
			set {
				if (value == m_isFiring)
					return;
				if (value)
					this.TimeWhenStartedFiring = Time.time;
				m_isFiring = value;
			}
		}
		public	bool	IsFireOn { get; set; }
		public	float	TimeWhenStartedFiring { get; private set; }
		public	float	TimeSinceStartedFiring { get { return Time.time - this.TimeWhenStartedFiring; } }

		public	Weapon	CurrentWeapon { get ; private set ; }
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


		//[SerializeField]	[Range(0,1)]	private	float	m_aimWithRifleMinAnimTime = 0.0f;
		//public	float	AimWithRifleMinAnimTime { get { return m_aimWithRifleMinAnimTime; } set { m_aimWithRifleMinAnimTime = value; } }

		[SerializeField]	[Range(0,4)]	private	float	m_aimWithRifleMaxAnimTime = 0.7f;
		public	float	AimWithRifleMaxAnimTime { get { return m_aimWithRifleMaxAnimTime; } set { m_aimWithRifleMaxAnimTime = value; } }
		
	//	[SerializeField]	[Range(0,1)]	private	float	m_aimWithArmMaxAnimTime = 1.0f;

		public	Vector3	cameraAimOffset = new Vector3 (0.7f, 0.2f, -1);

		public	bool	rotatePlayerInDirectionOfAiming = true;



        void Awake () {
			
			m_ped = this.GetComponent<Ped> ();

		}

		void Start ()
		{
			PlayerModel.onLateUpdate += this.UpdateWeaponTransform;
		}

		void OnLoaderFinished ()
		{
			if (this.autoAddWeapon)
			{
				this.AddRandomWeapons ();

				if (!this.IsHoldingWeapon)
				{
					this.SwitchWeapon (WeaponSlot.Pistol);
				}
			}
		}

		void Update () {

			if (!Loader.HasLoaded)
				return;


			// switch weapons
			if (GameManager.CanPlayerReadInput()) {

				if (Input.GetKeyDown (KeyCode.Q))
					this.SwitchWeapon (false);
				else if (Input.GetKeyDown (KeyCode.E))
					this.SwitchWeapon (true);
				
			}

			//this.UpdateWeaponTransform ();

			if (CurrentWeapon != null) {
				CurrentWeapon.EnableOrDisableGunFlash ();
				CurrentWeapon.UpdateGunFlashRotation ();
			}

			// update aiming state

			if (this.IsAiming) {
				// check if we should exit aiming state
				if (!this.IsHoldingWeapon || m_ped.IsInVehicle || !this.IsAimOn) {
					if (!this.IsFiring) {
						this.IsAiming = false;
					}
				}
			} else {
				// check if we should enter aiming state
				if (this.IsHoldingWeapon && this.IsAimOn && !m_ped.IsInVehicle) {
					this.IsAiming = true;
				}
			}

			// update firing state

			if (!this.IsAiming)
				this.IsFiring = false;

			// reload weapon ammo clip
			if (CurrentWeapon != null) {
				if (CurrentWeapon.AmmoClipSize > 0 && CurrentWeapon.AmmoInClip <= 0) {
					int amountToRefill = Mathf.Min (CurrentWeapon.AmmoClipSize, CurrentWeapon.AmmoOutsideOfClip);
					CurrentWeapon.AmmoInClip = amountToRefill;
					CurrentWeapon.AmmoOutsideOfClip -= amountToRefill;
				}
			}

		}

		void LateUpdate()
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

			if (!m_ped.shouldPlayAnims)
				return;

			if (this.IsAiming) {
				// player is aiming
				// play appropriate anim
				CurrentWeapon.UpdateAnimWhileAiming ();
			}

			if (!m_ped.IsInVehicle && !this.IsAiming && this.IsHoldingWeapon) {
				// player is not aiming, but is holding a weapon
				// update current anim

				CurrentWeapon.UpdateAnimWhileHolding ();
			}

		}

		private void UpdateWeaponTransform ()
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

		private void RotateSpine ()
		{
			if (!this.IsAiming)
				return;

			if (this.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
				return;
			
			PlayerModel.Spine.LookAt(Camera.main.transform.position + Camera.main.transform.forward * 500);

			Vector3 eulers = this.SpineOffset;
			if (this.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
				eulers.y = 0;
			PlayerModel.Spine.Rotate (eulers);
		//	PlayerModel.ChangeSpineRotation (this.CurrentWeaponTransform.forward, Camera.transform.position + Camera.transform.forward * Camera.farClipPlane - this.CurrentWeaponTransform.position, SpineRotationSpeed, ref tempSpineLocalEulerAngles, ref targetRot, ref spineRotationLastFrame);

		}

		public void RotatePlayerInDirectionOfAiming ()
		{

			if (!this.rotatePlayerInDirectionOfAiming)
				return;

			if (!this.IsAiming)
				return;

			if (this.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
				return;

//			Vector3 lookAtPos = Camera.transform.position + Camera.transform.forward * 500;
//			lookAtPos.y = m_player.transform.position.y;
//
//			m_player.transform.LookAt (lookAtPos, Vector3.up);

			Vector3 forward = Camera.main.transform.forward;
			forward.y = 0;
			forward.Normalize ();
		//	m_player.transform.forward = forward;
			m_ped.Heading = forward;

		}


		public void SwitchWeapon( bool next )
		{

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
			if (slotIndex == currentWeaponSlot)
				return;

			if (CurrentWeapon != null) {
				// hide the weapon
				HideWeapon( CurrentWeapon );
			}

			if (slotIndex >= 0) {
				
				CurrentWeapon = weapons [slotIndex];

				// show the weapon
				if (CurrentWeapon != null)
					UnHideWeapon (CurrentWeapon);

			} else {
				CurrentWeapon = null;
			}

			currentWeaponSlot = slotIndex;

			m_frameWhenSwitchedWeapon = Time.frameCount;

			this.IsFiring = false;

			this.UpdateWeaponTransform ();

		}

		private static void HideWeapon (Weapon weapon)
		{
			weapon.gameObject.SetActive (false);
		}

		private static void UnHideWeapon (Weapon weapon)
		{
			weapon.gameObject.SetActive (true);
		}

		public void SetWeaponAtSlot (Importing.Items.Definitions.WeaponDef weaponDef, int slot)
		{
			this.SetWeaponAtSlot (weaponDef.Id, slot);
		}

		public void SetWeaponAtSlot (int weaponId, int slotIndex)
		{
			
			// destroy current weapon at this slot
			if (weapons [slotIndex] != null) {
				DestroyWeapon (weapons [slotIndex]);
			}

			weapons [slotIndex] = Weapon.Load (weaponId);

			weapons [slotIndex].PedOwner = m_ped;

			if (slotIndex == currentWeaponSlot) {
				// update current weapon variable
				CurrentWeapon = weapons [slotIndex];

				// update it's transform
				this.UpdateWeaponTransform ();
			} else {
				// hide the newly created weapon
				HideWeapon (weapons[slotIndex]);
			}

		}

		public Weapon GetWeaponAtSlot (int slotIndex)
		{
			return this.weapons [slotIndex];
		}

		public void RemoveAllWeapons() {

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
