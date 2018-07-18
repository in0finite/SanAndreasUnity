using System.Collections.Generic;
using UnityEngine;
using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours {
	
	public class WeaponHolder : MonoBehaviour {

		private	Player	m_player;
		public	Pedestrian	PlayerModel { get { return m_player.PlayerModel; } }

		public	Weapon[]	weapons = new Weapon[(int)WeaponSlot.Count];

		public	int		currentWeaponSlot = -1;
		public	bool	IsHoldingWeapon { get { return this.currentWeaponSlot > 0; } }

		public	bool	autoAddWeapon = false;

		private	bool	m_isAiming = false;
		public bool IsAiming { get { return this.m_isAiming; } set { m_isAiming = value; } }



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

				weapons [(int)WeaponSlot.Machine] = Weapon.Load (355);
				SwitchWeapon ((int)WeaponSlot.Machine);
			}


			if (this.IsAiming && !m_player.IsInVehicle) {

				//	this.Play2Animations (new int[]{ 41, 51 }, new int[]{ 2 }, AnimGroup.MyWalkCycle,
				//		AnimGroup.MyWalkCycle, AnimIndex.IdleArmed, AnimIndex.GUN_STAND);

				PlayerModel.PlayAnim(AnimGroup.MyWalkCycle, AnimIndex.GUN_STAND, PlayMode.StopAll);

			}

			// update current anim
			if (!m_player.IsInVehicle && !this.IsAiming) {



			}


			// reset aim state - it should be done by controller
		//	m_isAiming = false;

		}

		private void SwitchWeapon(int slotIndex)
		{
			if (PlayerModel.weapon != null)
			{
				// set parent to weapons container in order to hide it
				//	PlayerModel.weapon.SetParent (Weapon.weaponsContainer.transform);

				PlayerModel.weapon.gameObject.SetActive(false);
			}

			PlayerModel.weapon = weapons[slotIndex].gameObject.transform;
			// change parent to make it visible
			//	PlayerModel.weapon.SetParent(this.transform);
			PlayerModel.weapon.gameObject.SetActive(true);

			currentWeaponSlot = slotIndex;
		}

	}

}
