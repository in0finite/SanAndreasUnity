using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Animation;

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

		private	bool	m_isAiming = false;
		public	bool	IsAiming { get { return this.m_isAiming; } set { 
				if (!this.IsHoldingWeapon || m_player.IsInVehicle)
					return;
				if (m_isAiming == value)
					return;
				m_isAiming = value;
				// start anim
			//	var state = PlayerModel.PlayAnim (AnimGroup.Rifle, AnimIndex.RIFLE_fire);
			//	state.speed = 0.0f;
			//	state.wrapMode = WrapMode.ClampForever;
			//	state.time = state.length;
			} }

		public	Transform	CurrentWeaponTransform { get ; private set ; }

		public Vector3 UpwardAxis;

		private Vector3 tempSpineLocalEulerAngles;
		private Quaternion targetRot;
		private Quaternion spineRotationLastFrame;

		public Vector3 SpineOffset;



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


			if (this.IsAiming && this.IsHoldingWeapon && !m_player.IsInVehicle) {
				// player is aiming
				// play appropriate anim

			//	this.Play2Animations (new int[]{ 41, 51 }, new int[]{ 2 }, AnimGroup.MyWalkCycle,
			//		AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.GUN_STAND);

				var state = PlayerModel.PlayAnim (AnimGroup.Rifle, AnimIndex.RIFLE_fire);
				state.wrapMode = WrapMode.ClampForever;
				if (state.normalizedTime > 0.7f)
					state.normalizedTime = 0.7f;
			}

			if (!m_player.IsInVehicle && !this.IsAiming && this.IsHoldingWeapon) {
				// player is not aiming
				// update current anim

				if (m_player.IsRunning) {

				//	Play2Animations (new int[] { 41, 51 }, new int[] { 2 }, AnimGroup.WalkCycle,
				//		AnimGroup.MyWalkCycle, AnimIndex.Run, AnimIndex.IdleArmed);

					PlayerModel.PlayAnim (AnimGroup.Gun, AnimIndex.run_armed);

				} else if (m_player.IsWalking) {

				//	Play2Animations (new int[] { 41, 51 }, new int[] { 2 }, AnimGroup.WalkCycle,
				//		AnimGroup.MyWalkCycle, AnimIndex.Walk, AnimIndex.IdleArmed);

					PlayerModel.PlayAnim (AnimGroup.Gun, AnimIndex.WALK_armed);

				} else {
					// player is standing

				//	Play2Animations(new int[] { 41, 51 }, new int[] { 2 }, AnimGroup.MyWalkCycle,
				//		AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.IdleArmed);

					PlayerModel.PlayAnim (AnimGroup.MyWalkCycle, AnimIndex.IdleArmed);
				}

			}


			// spine
			if (this.IsAiming)
			{
				PlayerModel.Spine.LookAt(Camera.transform.position + Camera.transform.position + Camera.transform.forward * Camera.farClipPlane);
				PlayerModel.Spine.Rotate(SpineOffset);
				this.UpwardAxis = PlayerModel.Spine.up;
			//	PlayerModel.ChangeSpineRotation (this.CurrentWeaponTransform.forward, Camera.transform.position + Camera.transform.forward * Camera.farClipPlane - this.CurrentWeaponTransform.position, SpineRotationSpeed, ref tempSpineLocalEulerAngles, ref targetRot, ref spineRotationLastFrame);
			}

			// update transform of weapon
			if (this.CurrentWeaponTransform != null && PlayerModel.RightFinger != null && PlayerModel.LeftFinger != null)
			{
				this.CurrentWeaponTransform.position = PlayerModel.RightFinger.position;
				this.CurrentWeaponTransform.rotation = PlayerModel.RightFinger.rotation;
			}


			// reset aim state - it should be done by controller
		//	m_isAiming = false;

		}

        private void LateUpdate()
        {
        }

        public void SwitchWeapon(WeaponSlot slot)
		{
			this.SwitchWeapon ((int)slot);
		}

		public void SwitchWeapon (int slotIndex)
		{
			if (CurrentWeaponTransform != null) {
				// set parent to weapons container in order to hide it
			//	weapon.SetParent (Weapon.weaponsContainer.transform);

				CurrentWeaponTransform.gameObject.SetActive (false);
			}

			if (slotIndex >= 0) {
				
				CurrentWeaponTransform = weapons [slotIndex].gameObject.transform;

				// change parent to make it visible
			//	weapon.SetParent(this.transform);
				CurrentWeaponTransform.gameObject.SetActive (true);

			} else {
				CurrentWeaponTransform = null;
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
