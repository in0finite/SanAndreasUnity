using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Animation;
using SanAndreasUnity.Importing.Weapons;

namespace SanAndreasUnity.Behaviours {
	
	public class WeaponHolder : MonoBehaviour {

		private	Player	m_player;
		public	Pedestrian	PlayerModel { get { return m_player.PlayerModel; } }
		public	Camera	Camera { get { return m_player.Camera; } }

		private	Weapon[]	weapons = new Weapon[(int)WeaponSlot.Count];

		private	int		currentWeaponSlot = -1;
		public	int		CurrentWeaponSlot { get { return this.currentWeaponSlot; } }
		public	bool	IsHoldingWeapon { get { return this.currentWeaponSlot > 0; } }

		public	bool	autoAddWeapon = false;

		public	bool	IsAimOn { get; set; }
		public	bool	IsAiming { get { return this.IsAimOn && this.IsHoldingWeapon && !m_player.IsInVehicle; } }

		public	Weapon	CurrentWeapon { get ; private set ; }
		private	Transform	CurrentWeaponTransform { get { return CurrentWeapon != null ? CurrentWeapon.transform : null; } }
        

		public Vector3 SpineOffset;

		public enum WeaponAttachType
		{
			None,
			RightHand,
			BothFingers
		}

		public	WeaponAttachType	weaponAttachType = WeaponAttachType.RightHand;

		[SerializeField]	[Range(0,1)]	private	float	m_aimWithRifleMaxAnimTime = 0.7f;
		[SerializeField]	[Range(0,1)]	private	float	m_aimWithArmMaxAnimTime = 1.0f;



        void Awake () {
			
			m_player = this.GetComponent<Player> ();

		}
		
		void Update () {

			if (!Loader.HasLoaded)
				return;


			// switch weapons - does not work
			if (GameManager.CanPlayerReadInput() && !m_player.IsInVehicle) {
				if (Input.mouseScrollDelta.y != 0) {
					
					if (currentWeaponSlot < 0)
						currentWeaponSlot = 0;

					for (int i = currentWeaponSlot + (int)Mathf.Sign (Input.mouseScrollDelta.y), count = 0;
						i != currentWeaponSlot && count < (int)WeaponSlot.Count;
						i += (int)Mathf.Sign (Input.mouseScrollDelta.y), count++) {
						if (i < 0)
							i = weapons.Length - 1;
						if (i >= weapons.Length)
							i = 0;

						if (weapons [i] != null) {
							SwitchWeapon (i);
							break;
						}
					}
				}
			}


			// add weapons to player if he doesn't have any
			if (autoAddWeapon && null == System.Array.Find (weapons, w => w != null)) {
				// player has no weapons

				this.SetWeaponAtSlot (355, WeaponSlot.Machine);
				this.SwitchWeapon (WeaponSlot.Machine);
			}


		}

		void LateUpdate()
		{
			// all things that manipulate skeleton must be placed in LateUpdate(), because otherwise Animator will
			// override them

			// order of these functions is important

			UpdateAnims ();

			RotatePlayerInDirectionOfAiming ();

			RotateSpine ();

			UpdateWeaponTransform ();

		}


		private void UpdateAnims ()
		{

			if (!m_player.shouldPlayAnims)
				return;

			if (this.IsAiming) {
				// player is aiming
				// play appropriate anim

			//	this.Play2Animations (new int[]{ 41, 51 }, new int[]{ 2 }, AnimGroup.MyWalkCycle,
			//		AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.GUN_STAND);

				if (CurrentWeapon.HasFlag (GunFlag.AIMWITHARM)) {
                    // aim with arm
                    // ie: pistol, tec9, sawnoff
                    
//					var state = PlayerModel.PlayAnim (AnimGroup.Colt45, AnimIndex.colt45_fire);
//					state.wrapMode = WrapMode.ClampForever;
//					if (state.normalizedTime > m_aimWithArmMaxAnimTime)
//						state.normalizedTime = m_aimWithArmMaxAnimTime;

                    var state = PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Idle);
					//state.RemoveMixingTransform (PlayerModel.Spine);

					// rotate right upper arm to match direction of player
					// we'll need a few adjustments, because arm's right vector is player's forward vector,
					// and arm's forward vector is player's down vector => arm's up is player's left
					Vector3 lookAtPos = m_player.transform.position - m_player.transform.up * 500;
					Vector3 dir = -m_player.transform.right;
					PlayerModel.RightUpperArm.LookAt( lookAtPos, dir);
					// also rotate right hand
					PlayerModel.RightHand.LookAt (lookAtPos, dir);

				} else {
					
				//	PlayerModel.PlayUpperLayerAnimations (AnimGroup.Rifle, AnimGroup.WalkCycle, AnimIndex.RIFLE_fire, AnimIndex.Idle);

					var state = PlayerModel.PlayAnim (AnimGroup.Rifle, AnimIndex.RIFLE_fire);
					state.wrapMode = WrapMode.ClampForever;
					if (state.normalizedTime > m_aimWithRifleMaxAnimTime)
						state.normalizedTime = m_aimWithRifleMaxAnimTime;
					
				}
			}

			if (!m_player.IsInVehicle && !this.IsAiming && this.IsHoldingWeapon) {
				// player is not aiming, but is holding a weapon
				// update current anim

				if (m_player.IsRunning) {

				//	Play2Animations (new int[] { 41, 51 }, new int[] { 2 }, AnimGroup.WalkCycle,
				//		AnimGroup.MyWalkCycle, AnimIndex.Run, AnimIndex.IdleArmed);

					if (CurrentWeapon.HasFlag (GunFlag.AIMWITHARM)) {
						PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Run);
					} else {
						PlayerModel.PlayAnim (AnimGroup.Gun, AnimIndex.run_armed);
					}

				} else if (m_player.IsWalking) {

				//	Play2Animations (new int[] { 41, 51 }, new int[] { 2 }, AnimGroup.WalkCycle,
				//		AnimGroup.MyWalkCycle, AnimIndex.Walk, AnimIndex.IdleArmed);

					if (CurrentWeapon.HasFlag (GunFlag.AIMWITHARM)) {
						PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Walk);
					} else {
						PlayerModel.PlayAnim (AnimGroup.Gun, AnimIndex.WALK_armed);
					}

				} else if (m_player.IsSprinting) {

					if (CurrentWeapon.HasFlag (GunFlag.AIMWITHARM)) {
						PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.sprint_civi);
					} else {
						PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.IdleArmed);
					}

				} else {
					// player is standing

				//	Play2Animations(new int[] { 41, 51 }, new int[] { 2 }, AnimGroup.MyWalkCycle,
				//		AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.IdleArmed);

					if (CurrentWeapon.HasFlag (GunFlag.AIMWITHARM)) {
						PlayerModel.PlayAnim (AnimGroup.WalkCycle, AnimIndex.Idle);
					} else {
						PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.IdleArmed);
					}

				}

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

			if (this.IsAiming)
			{
				PlayerModel.Spine.LookAt(Camera.transform.position + Camera.transform.forward * 500);

				Vector3 eulers = this.SpineOffset;
				if (this.CurrentWeapon.HasFlag (GunFlag.AIMWITHARM))
					eulers.y = 0;
				PlayerModel.Spine.Rotate (eulers);
			//	PlayerModel.ChangeSpineRotation (this.CurrentWeaponTransform.forward, Camera.transform.position + Camera.transform.forward * Camera.farClipPlane - this.CurrentWeaponTransform.position, SpineRotationSpeed, ref tempSpineLocalEulerAngles, ref targetRot, ref spineRotationLastFrame);
			}

		}

		public void RotatePlayerInDirectionOfAiming ()
		{

			if (!this.IsAiming)
				return;

//			Vector3 lookAtPos = Camera.transform.position + Camera.transform.forward * 500;
//			lookAtPos.y = m_player.transform.position.y;
//
//			m_player.transform.LookAt (lookAtPos, Vector3.up);

			Vector3 forward = Camera.transform.forward;
			forward.y = 0;
			forward.Normalize ();
			m_player.transform.forward = forward;

		}


        public void SwitchWeapon(WeaponSlot slot)
		{
			this.SwitchWeapon ((int)slot);
		}

		public void SwitchWeapon (int slotIndex)
		{
			if (CurrentWeapon != null) {
				// set parent to weapons container in order to hide it
			//	weapon.SetParent (Weapon.weaponsContainer.transform);

				CurrentWeapon.gameObject.SetActive (false);
			}

			if (slotIndex >= 0) {
				
				CurrentWeapon = weapons [slotIndex];

				// change parent to make it visible
			//	weapon.SetParent(this.transform);
				CurrentWeapon.gameObject.SetActive (true);

			} else {
				CurrentWeapon = null;
			}

			currentWeaponSlot = slotIndex;
		}

		public void SetWeaponAtSlot (Importing.Items.Definitions.WeaponDef weaponDef, WeaponSlot slot)
		{

			this.SetWeaponAtSlot (weaponDef.Id, slot);

		}

		public void SetWeaponAtSlot (int weaponId, WeaponSlot slot)
		{

			weapons [(int)slot] = Weapon.Load (weaponId);

		}

		public void RemoveAllWeapons() {

			this.SwitchWeapon (-1);

			for (int i = 0; i < this.weapons.Length; i++) {
				this.weapons [i] = null;
			}

		}

	}

}
