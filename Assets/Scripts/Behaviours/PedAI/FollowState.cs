using System.Linq;
using SanAndreasUnity.Behaviours.Vehicles;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class FollowState : BaseState
    {
        public Ped LeaderPed { get; private set; }

        private Ped _currentlyEngagedPed;

        private UpdateAttackParams _updateAttackParams = new UpdateAttackParams();

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

            _updateAttackParams.Cleanup();
            this.LeaderPed = this.ParameterForEnteringState as Ped;
        }

        public override void OnBecameInactive()
        {
            this.LeaderPed = null;
            _currentlyEngagedPed = null;

            _ped.MovementAgent.Destination = null;

            base.OnBecameInactive();
        }

        public override void UpdateState2Seconds()
        {
            if (null == this.LeaderPed)
                return;

            _chaseState.ChooseBestWeapon();

            // try to find new target, or remove the current one if needed

            if (null == _ped.CurrentWeapon)
            {
                // we have no weapon to attack with, or no ammo
                // remove current target and go to leader
                _currentlyEngagedPed = null;
                return;
            }

            if (this.LeaderPed.IsInVehicle && !_ped.IsInVehicle)
            {
                // follow our leader into the vehicle
                _currentlyEngagedPed = null;
                return;
            }

            if (this.IsFarAwayFromLeader())
            {
                if (null == _currentlyEngagedPed || !this.IsInRange(_currentlyEngagedPed))
                {
                    _currentlyEngagedPed = this.GetNextPedToAttack();
                    return;
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
                if (nextPedToAttack == _currentlyEngagedPed)
                    nextPedToAttack = null;

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

                // current target is not in range and we failed to switch to next target

                // if current target is also not in leader range, forget about it
                if (!this.IsInRangeOfLeader(_currentlyEngagedPed))
                {
                    _currentlyEngagedPed = null;
                    return;
                }
            }

        }

        public override void UpdateState()
        {
            if (null == this.LeaderPed)
            {
                _pedAI.StartIdling();
                return;
            }

            // handle vehicle logic - follow ped in or out of vehicle
            if (this.MyPed.IsInVehicle || this.LeaderPed.IsInVehicle)
                this.UpdateFollowing_MovementPart();

            // update attacking or following

            if (_currentlyEngagedPed != null)
            {
                if (_ped.IsInVehicle)
                    _updateAttackParams.wasInRange = true;
                _chaseState.UpdateAttackOnPed(_currentlyEngagedPed, _updateAttackParams);
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

            if (null == this.LeaderPed)
            {
                _pedAI.StartIdling();
                return;
            }

            Vector3 targetPos = this.LeaderPed.transform.position;
            float currentStoppingDistance = 3f;

            if (this.LeaderPed.IsInVehicleSeat && !this.MyPed.IsInVehicle)
            {
                // find a free vehicle seat to enter vehicle

                var vehicle = this.LeaderPed.CurrentVehicle;

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
            else if (!this.LeaderPed.IsInVehicle && this.MyPed.IsInVehicleSeat)
            {
                // target player is not in vehicle, and ours is
                // exit the vehicle

                this.MyPed.OnSubmitPressed();
                return;
            }


            if (this.MyPed.IsInVehicle)
            {
                _ped.MovementAgent.Destination = null;
                return;
            }

            _ped.MovementAgent.Destination = targetPos;
            _ped.MovementAgent.StoppingDistance = currentStoppingDistance;

            Vector3 moveInput = _ped.MovementAgent.DesiredDirectionXZ;
            
            if (moveInput != Vector3.zero)
            {
                this.MyPed.IsRunOn = true;
                this.MyPed.Movement = moveInput;
                this.MyPed.Heading = moveInput;
            }
        }

        protected internal override void OnMyPedDamaged(DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            Ped attackerPed = dmgInfo.GetAttackerPed();

            if (null == attackerPed)
                return;

            if (attackerPed == this.LeaderPed)
            {
                // our leader attacked us
                // stop following him
                this.LeaderPed = null;
                return;
            }

            if (!this.IsMemberOfOurGroup(attackerPed))
            {
                _enemyPeds.AddIfNotPresent(attackerPed);
                this.UpdateState2Seconds();
                return;
            }

        }

        protected internal override void OnOtherPedDamaged(Ped damagedPed, DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            Ped attackerPed = dmgInfo.GetAttackerPed();
            if (null == attackerPed)
                return;

            if (null == this.LeaderPed) // we are not in a group
                return;

            bool isAttackerPedMember = this.IsMemberOfOurGroup(attackerPed);
            bool isDamagedPedMember = this.IsMemberOfOurGroup(damagedPed);

            if (attackerPed == this.LeaderPed && !isDamagedPedMember && dmgInfo.damageType != DamageType.Explosion)
            {
                // our leader attacked someone, not as part of explosion
                // make that someone our enemy
                _enemyPeds.AddIfNotPresent(damagedPed);
                this.UpdateState2Seconds();
                return;
            }

            if (this.LeaderPed == damagedPed && !isAttackerPedMember)
            {
                // our leader was attacked
                // his enemies are also our enemies
                _enemyPeds.AddIfNotPresent(attackerPed);
                this.UpdateState2Seconds();
                return;
            }

            if (isDamagedPedMember && !isAttackerPedMember)
            {
                // attacked ped is member of our group
                // his enemy will be also our enemy
                _enemyPeds.AddIfNotPresent(attackerPed);
                this.UpdateState2Seconds();
                return;
            }

        }

        protected internal override void OnVehicleDamaged(Vehicle vehicle, DamageInfo damageInfo)
        {
            Ped attackerPed = damageInfo.GetAttackerPed();
            if (null == attackerPed)
                return;

            if (null == this.LeaderPed) // not member of group
                return;

            // ignore explosion damage, it can be "accidental"
            if (damageInfo.damageType == DamageType.Explosion)
                return;

            if (this.IsMemberOfOurGroup(attackerPed))
                return;

            if (vehicle.Seats.Exists(s => s.OccupyingPed == this.MyPed || s.OccupyingPed == this.LeaderPed))
            {
                // either our leader or we are in the vehicle
                _enemyPeds.AddIfNotPresent(attackerPed);
                this.UpdateState2Seconds();
                return;
            }

        }

        protected internal override void OnRecruit(Ped recruiterPed)
        {
            if (this.LeaderPed == recruiterPed)
            {
                // unfollow
                this.LeaderPed = null;
            }
        }

        public bool IsMemberOfOurGroup(Ped ped)
        {
            if (this.LeaderPed == null) // we are not part of any group
                return false;

            if (this.LeaderPed == ped) // our leader
                return true;

            var pedAI = ped.GetComponent<PedAI>();
            if (pedAI != null && pedAI.CurrentState is FollowState followState && followState.LeaderPed == this.LeaderPed)
                return true;

            return false;
        }

        public bool IsFarAwayFromLeader()
        {
            return Vector2.Distance(
                _ped.transform.position.ToVec2WithXAndZ(),
                this.LeaderPed.transform.position.ToVec2WithXAndZ())
                   > this.maxDistanceFromLeader;
        }

        public bool IsInRange(Ped ped)
        {
            return _chaseState.IsInRange(ped);
        }

        public bool IsInRangeOfLeader(Ped ped)
        {
            return Vector3.Distance(ped.transform.position, this.LeaderPed.transform.position) < this.maxDistanceFromLeader;
        }

        public Ped GetNextPedToAttack()
        {
            if (null == this.LeaderPed)
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