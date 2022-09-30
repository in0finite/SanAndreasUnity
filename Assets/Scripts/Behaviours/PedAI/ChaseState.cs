using System.Linq;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class UpdateAttackParams
    {
        public bool wasInRange = false;
        public double timeWhenAddedFireOffset = 0;
        public float timeUntilOffsetChanges = 1f;
        public Vector3 newFireOffset = Vector3.zero;

        public void Cleanup()
        {
            this.wasInRange = false;
            this.timeWhenAddedFireOffset = 0f;
            this.timeUntilOffsetChanges = 1f;
            this.newFireOffset = Vector3.zero;
        }
    }

    public class ChaseState : BaseState
    {
        public Ped TargetPed { get; private set; }

        private readonly UpdateAttackParams _updateAttackParams = new UpdateAttackParams();

        private static int[] s_weaponSlotsOrdered = new[]
        {
            WeaponSlot.Machine,
            WeaponSlot.Submachine,
            WeaponSlot.Rifle,
            WeaponSlot.Heavy,
            WeaponSlot.Shotgun,
            WeaponSlot.Pistol,
        };


        public override void OnBecameActive()
        {
            base.OnBecameActive();

            _updateAttackParams.Cleanup();

            this.TargetPed = this.ParameterForEnteringState as Ped;
            if (this.TargetPed != null)
                _enemyPeds.AddIfNotPresent(this.TargetPed);
        }

        public override void OnBecameInactive()
        {
            _ped.MovementAgent.Destination = null;

            base.OnBecameInactive();
        }

        public override void UpdateState2Seconds()
        {
            this.ChooseBestWeapon();

            if (null == _ped.CurrentWeapon)
            {
                // we have no weapon to attack with, or no ammo
                _pedAI.StartWalkingAround();
                return;
            }

            if (null == this.TargetPed)
            {
                this.TargetPed = this.GetNextPedToAttack();
                return;
            }

            if (this.IsInRange(this.TargetPed))
                return; // current target is in range, continue attacking it

            this.TargetPed = this.GetNextPedToAttack();

            /*Ped nextPedToAttack = this.GetNextPedToAttack();
            if (null == nextPedToAttack || nextPedToAttack == this.TargetPed)
                return;

            // check if we should switch to next target

            Vector3 myPosition = _ped.transform.position;
            float currentDistance = Vector3.Distance(this.TargetPed.transform.position, myPosition);
            float nextDistance = Vector3.Distance(nextPedToAttack.transform.position, myPosition);

            if (currentDistance - nextDistance > 6f)
            {
                // next target is closer by some delta value - switch to it
                this.TargetPed = nextPedToAttack;
                return;
            }*/
        }

        public override void UpdateState()
        {
            if (null == this.TargetPed)
                this.TargetPed = this.GetNextPedToAttack();

            if (null == this.TargetPed)
            {
                // we finished attacking all enemies, now start walking
                _pedAI.StartWalkingAround();
                return;
            }

            if (_ped.IsInVehicle)
            {
                _ped.OnSubmitPressed();
                return;
            }

            this.UpdateAttackOnPed(this.TargetPed, _updateAttackParams);
        }

        public Ped GetNextPedToAttack()
        {
            _enemyPeds.RemoveDeadObjectsIfNotEmpty();
            if (_enemyPeds.Count == 0)
                return null;

            Vector3 myPosition = _ped.transform.position;

            Ped closestPed = _enemyPeds.MinBy(p => Vector3.Distance(p.transform.position, myPosition), null);

            return closestPed;
        }

        public bool IsInRange(Ped ped)
        {
            return Vector3.Distance(ped.transform.position, _ped.transform.position) < 10f;
        }

        public void UpdateAttackOnPed(Ped ped, UpdateAttackParams updateAttackParams)
        {
            if (Time.timeAsDouble - updateAttackParams.timeWhenAddedFireOffset > updateAttackParams.timeUntilOffsetChanges)
            {
                updateAttackParams.timeWhenAddedFireOffset = Time.timeAsDouble;
                updateAttackParams.newFireOffset = Random.onUnitSphere * 0.2f;
            }

            Vector3 myHeadPos = GetHeadOrTransform(this.MyPed).position;
            Vector3 targetHeadPos = GetHeadOrTransform(ped).position;
            Vector3 targetChestPos = GetChestPosition(ped) + updateAttackParams.newFireOffset;
            Vector3 firePos = this.MyPed.IsAiming ? this.MyPed.FirePosition : myHeadPos;

            Vector3 diff = targetHeadPos - myHeadPos;
            Vector3 dir = diff.normalized;
            this.MyPed.Heading = dir;

            Vector3 aimDir = (targetChestPos - firePos).normalized;

            // fix for stuttering which happens when target ped is too close:
            // we assign AimDir here, which changes skeleton and therefore changes fire position, which then
            // changes AimDir in next frame
            if (diff.ToVec2WithXAndZ().magnitude < 2.5f)
                aimDir = (ped.transform.position + updateAttackParams.newFireOffset - _ped.transform.position).normalized;

            if (this.MyPed.IsInVehicle)
            {
                if (diff.magnitude < 10f)
                {
                    this.MyPed.AimDirection = aimDir;
                    if (!this.MyPed.IsAiming)
                        this.MyPed.OnAimButtonPressed();
                    this.MyPed.IsFireOn = true;
                }
                else
                {
                    if (this.MyPed.IsAiming)
                        this.MyPed.OnAimButtonPressed();
                }
            }
            else
            {
                float rangeRequired = updateAttackParams.wasInRange ? 10f : 8f;

                _ped.MovementAgent.Destination = targetHeadPos;
                _ped.MovementAgent.StoppingDistance = 1f;

                updateAttackParams.wasInRange = false;

                if (diff.magnitude < rangeRequired)
                {
                    updateAttackParams.wasInRange = true;
                    this.MyPed.AimDirection = aimDir;
                    this.MyPed.IsAimOn = true;
                    this.MyPed.IsFireOn = true;
                }
                else
                {
                    Vector3 moveInput = _ped.MovementAgent.DesiredDirectionXZ;
                    if (moveInput != Vector3.zero)
                    {
                        this.MyPed.IsRunOn = true;
                        this.MyPed.Movement = moveInput;
                        this.MyPed.Heading = moveInput;
                    }
                }
            }
        }

        private static Transform GetHeadOrTransform(Ped ped)
        {
            return ped.PlayerModel.Head != null ? ped.PlayerModel.Head : ped.transform;
        }

        private static Vector3 GetChestPosition(Ped ped)
        {
            return ped.PlayerModel.UpperSpine != null
                ? ped.PlayerModel.UpperSpine.position
                : ped.transform.position + new Vector3(0f, 0.3f, 0f);
        }

        public void ChooseBestWeapon()
        {
            _ped.WeaponHolder.SwitchWeapon(this.GetBestWeaponSlot());
        }

        public int GetBestWeaponSlot()
        {
            for (int i = 0; i < s_weaponSlotsOrdered.Length; i++)
            {
                int slot = s_weaponSlotsOrdered[i];
                Weapon weapon = _ped.WeaponHolder.GetWeaponAtSlot(slot);
                if (weapon != null && weapon.TotalAmmo > 0)
                    return slot;
            }

            return WeaponSlot.Hand;
        }

        public bool CanStartChasing()
        {
            int slot = this.GetBestWeaponSlot();
            return slot != WeaponSlot.Hand;
        }
    }
}