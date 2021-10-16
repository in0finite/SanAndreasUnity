using System.Linq;
using SanAndreasUnity.Behaviours.Vehicles;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class FollowState : BaseState
    {
        public Ped TargetPed { get; private set; }

        private Ped _currentlyEngagedPed;

        private bool _wasInRange = false;

        private ChaseState _chaseState;

        public float maxDistanceFromLeader = 30f;


        protected internal override void OnAwake(PedAI pedAI)
        {
            base.OnAwake(pedAI);

            _chaseState = _pedAI.StateContainer.GetStateOrThrow<ChaseState>();
        }

        public override void OnBecameActive()
        {
            base.OnBecameActive();

            _wasInRange = false;
            this.TargetPed = this.ParameterForEnteringState as Ped;
        }

        public override void OnBecameInactive()
        {
            this.TargetPed = null;
            _currentlyEngagedPed = null;

            base.OnBecameInactive();
        }

        public override void UpdateState2Seconds()
        {
            if (null == this.TargetPed)
                return;

            // try to find new target, or remove the current one if needed
            if (this.IsFarAwayFromLeader())
            {
                bool isCurrentTargetInRange = _currentlyEngagedPed != null && this.IsInRange(_currentlyEngagedPed);
                Ped nextPedToAttack = this.GetNextPedToAttack();
                bool isNextTargetInRange = nextPedToAttack != null && this.IsInRange(nextPedToAttack);

                if (isCurrentTargetInRange || isNextTargetInRange)
                {
                    // we have a target in range
                    // attack this target
                    if (!isCurrentTargetInRange)
                        _currentlyEngagedPed = nextPedToAttack;
                }
                else
                {
                    // go back to leader - remove current target
                    _currentlyEngagedPed = null;
                }
            }
            else // we are close enough to leader
            {
                if (null == _currentlyEngagedPed)
                {
                    _currentlyEngagedPed = this.GetNextPedToAttack();
                    return;
                }

                if (this.IsInRange(_currentlyEngagedPed)) // current target is in range, continue attacking it
                    return;

                // current target is not in range

                float currentDistance = Vector3.Distance(_currentlyEngagedPed.transform.position, _ped.transform.position);
                Ped nextPedToAttack = this.GetNextPedToAttack();

                if (nextPedToAttack != null && this.IsInRange(nextPedToAttack))
                {
                    // next target is in range - switch to it
                    _currentlyEngagedPed = nextPedToAttack;
                    return;
                }

                // neither current target nor next target are in range

                if (nextPedToAttack != null)
                {
                    float distanceToNextPed = Vector3.Distance(nextPedToAttack.transform.position, _ped.transform.position);
                    if (currentDistance - distanceToNextPed > 12f)
                    {
                        // next target is closer by some delta value - switch to it
                        _currentlyEngagedPed = nextPedToAttack;
                        return;
                    }
                }
            }

        }

        public override void UpdateState()
        {
            if (null == this.TargetPed)
            {
                _pedAI.StartIdling();
                return;
            }

            // handle vehicle logic - follow ped in or out of vehicle
            if (this.MyPed.IsInVehicle || this.TargetPed.IsInVehicle)
                this.UpdateFollowing_MovementPart();

            // this we do every frame: if we are close enough to leader and have no target, find one
            if (!this.IsFarAwayFromLeader() && null == _currentlyEngagedPed)
            {
                _currentlyEngagedPed = this.GetNextPedToAttack();
            }

            // update attacking or following

            if (_currentlyEngagedPed != null)
            {
                _chaseState.UpdateAttackOnPed(_currentlyEngagedPed, ref _wasInRange);
            }
            else
            {
                // we don't have a target to attack
                // follow our leader

                this.UpdateFollowing_MovementPart();

                if (this.MyPed.IsInVehicle && this.MyPed.IsAiming)
                {
                    // stop aiming
                    this.MyPed.OnAimButtonPressed();
                    return;
                }
            }
        }

        void UpdateFollowing_MovementPart()
        {
            // follow target ped

            if (null == this.TargetPed)
            {
                _pedAI.StartIdling();
                return;
            }

            Vector3 targetPos = this.TargetPed.transform.position;
            float currentStoppingDistance = 3f;

            if (this.TargetPed.IsInVehicleSeat && !this.MyPed.IsInVehicle)
            {
                // find a free vehicle seat to enter vehicle

                var vehicle = this.TargetPed.CurrentVehicle;

                var closestfreeSeat = Ped.GetFreeSeats(vehicle).Select(sa => new {sa = sa, tr = vehicle.GetSeatTransform(sa)})
                    .OrderBy(s => s.tr.Distance(_ped.transform.position))
                    .FirstOrDefault();

                if (closestfreeSeat != null)
                {
                    // check if we would enter this seat on attempt
                    var vehicleThatPedWouldEnter = this.MyPed.GetVehicleThatPedWouldEnterOnAttempt();
                    if (vehicleThatPedWouldEnter.vehicle == vehicle && vehicleThatPedWouldEnter.seatAlignment == closestfreeSeat.sa)
                    {
                        // we would enter this seat
                        // go ahead and enter it
                        this.MyPed.OnSubmitPressed();
                        return;
                    }
                    else
                    {
                        // we would not enter this seat - it's not close enough, or maybe some other seat (occupied one) is closer
                        // move toward the seat
                        targetPos = closestfreeSeat.tr.position;
                        currentStoppingDistance = 0.01f;
                    }
                }

            }
            else if (!this.TargetPed.IsInVehicle && this.MyPed.IsInVehicleSeat)
            {
                // target player is not in vehicle, and ours is
                // exit the vehicle

                this.MyPed.OnSubmitPressed();
                return;
            }


            if (this.MyPed.IsInVehicle)
                return;

            Vector3 diff = targetPos - _ped.transform.position;
            float distance = diff.ToVec2WithXAndZ().magnitude;

            if (distance > currentStoppingDistance)
            {
                Vector3 diffDir = diff.normalized;

                this.MyPed.IsRunOn = true;
                this.MyPed.Movement = diffDir;
                this.MyPed.Heading = diffDir;
            }
        }

        protected internal override void OnMyPedDamaged(DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            Ped attackerPed = dmgInfo.GetAttackerPed();

            if (null == attackerPed)
                return;

            if (attackerPed == this.TargetPed)
            {
                // our leader attacked us
                // stop following him
                this.TargetPed = null;
                return;
            }

            if (!this.IsMemberOfOurGroup(attackerPed))
            {
                _enemyPeds.AddIfNotPresent(attackerPed);
                return;
            }

        }

        protected internal override void OnOtherPedDamaged(Ped damagedPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            Ped attackerPed = dmgInfo.GetAttackerPed();
            if (null == attackerPed)
                return;

            if (null == this.TargetPed) // we are not in a group
                return;

            bool isAttackerPedMember = this.IsMemberOfOurGroup(attackerPed);
            bool isDamagedPedMember = this.IsMemberOfOurGroup(damagedPed);

            if (attackerPed == this.TargetPed && !isDamagedPedMember && dmgInfo.damageType != DamageType.Explosion)
            {
                // our leader attacked someone, not as part of explosion
                // make that someone our enemy
                _enemyPeds.AddIfNotPresent(damagedPed);
                return;
            }

            if (this.TargetPed == damagedPed && !isAttackerPedMember)
            {
                // our leader was attacked
                // his enemies are also our enemies
                _enemyPeds.AddIfNotPresent(attackerPed);
                return;
            }

            if (isDamagedPedMember && !isAttackerPedMember)
            {
                // attacked ped is member of our group
                // his enemy will be also our enemy
                _enemyPeds.AddIfNotPresent(attackerPed);
                return;
            }

        }

        protected internal override void OnVehicleDamaged(Vehicle vehicle, DamageInfo damageInfo)
        {
            Ped attackerPed = damageInfo.GetAttackerPed();
            if (null == attackerPed)
                return;

            if (null == this.TargetPed) // not member of group
                return;

            // ignore explosion damage, it can be "accidental"
            if (damageInfo.damageType == DamageType.Explosion)
                return;

            if (this.IsMemberOfOurGroup(attackerPed))
                return;

            if (vehicle.Seats.Exists(s => s.OccupyingPed == this.MyPed || s.OccupyingPed == this.TargetPed))
            {
                // either our leader or we are in the vehicle
                _enemyPeds.AddIfNotPresent(attackerPed);
                return;
            }

        }

        protected internal override void OnRecruit(Ped recruiterPed)
        {
            if (this.TargetPed == recruiterPed)
            {
                // unfollow
                this.TargetPed = null;
            }
        }

        public bool IsMemberOfOurGroup(Ped ped)
        {
            if (this.TargetPed == null) // we are not part of any group
                return false;

            if (this.TargetPed == ped) // our leader
                return true;

            var pedAI = ped.GetComponent<PedAI>();
            if (pedAI != null && pedAI.CurrentState is FollowState followState && followState.TargetPed == this.TargetPed)
                return true;

            return false;
        }

        public bool IsFarAwayFromLeader()
        {
            return Vector2.Distance(
                _ped.transform.position.ToVec2WithXAndZ(),
                this.TargetPed.transform.position.ToVec2WithXAndZ())
                   > this.maxDistanceFromLeader;
        }

        public bool IsInRange(Ped ped)
        {
            return _chaseState.IsInRange(ped);
        }

        public bool IsInRangeOfLeader(Ped ped)
        {
            return Vector3.Distance(ped.transform.position, this.TargetPed.transform.position) < this.maxDistanceFromLeader;
        }

        public Ped GetNextPedToAttack()
        {
            if (null == this.TargetPed)
                return _chaseState.GetNextPedToAttack();

            _enemyPeds.RemoveDeadObjectsIfNotEmpty();
            if (_enemyPeds.Count == 0)
                return null;

            Vector3 myPosition = _ped.transform.position;

            Ped closestPedInRange = _enemyPeds
                .Where(p => this.IsInRangeOfLeader(p) || this.IsInRange(p))
                .MinBy(p => Vector3.Distance(p.transform.position, myPosition), null);

            return closestPedInRange;
        }
    }
}