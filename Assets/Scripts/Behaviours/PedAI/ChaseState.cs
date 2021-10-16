using System.Linq;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class ChaseState : BaseState
    {
        public Ped TargetPed { get; private set; }

        private bool _wasInRange = false;


        public override void OnBecameActive()
        {
            base.OnBecameActive();

            _wasInRange = false;

            this.TargetPed = this.ParameterForEnteringState as Ped;
            if (this.TargetPed != null)
                _enemyPeds.AddIfNotPresent(this.TargetPed);
        }

        public override void UpdateState2Seconds()
        {
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

            this.UpdateAttackOnPed(this.TargetPed, ref _wasInRange);
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

        public void UpdateAttackOnPed(Ped ped, ref bool wasInRange)
        {
            //var weapon = this.MyPed.CurrentWeapon;
            Vector3 myHeadPos = GetHeadOrTransform(this.MyPed).position;
            Vector3 targetHeadPos = GetHeadOrTransform(ped).position;
            Vector3 firePos = this.MyPed.IsAiming ? this.MyPed.FirePosition : myHeadPos;

            Vector3 diff = targetHeadPos - myHeadPos;
            Vector3 dir = diff.normalized;
            this.MyPed.Heading = dir;

            Vector3 aimDir = (targetHeadPos - firePos).normalized;

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
                float rangeRequired = wasInRange ? 10f : 8f;

                wasInRange = false;

                if (diff.magnitude < rangeRequired)
                {
                    wasInRange = true;
                    this.MyPed.AimDirection = aimDir;
                    this.MyPed.IsAimOn = true;
                    this.MyPed.IsFireOn = true;
                }
                else if (Vector2.Distance(ped.transform.position.ToVec2WithXAndZ(), this.MyPed.transform.position.ToVec2WithXAndZ()) > 3f)
                {
                    this.MyPed.IsRunOn = true;
                    this.MyPed.Movement = dir;
                }
            }
        }

        private static Transform GetHeadOrTransform(Ped ped)
        {
            return ped.PlayerModel.Head != null ? ped.PlayerModel.Head : ped.transform;
        }
    }
}