using System.Linq;
using SanAndreasUnity.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class FollowState : BaseState
    {
        public Ped TargetPed { get; private set; }

        private Ped _currentlyEngagedPed;

        private ChaseState _chaseState;


        protected internal override void OnAwake(PedAI pedAI)
        {
            base.OnAwake(pedAI);

            _chaseState = _pedAI.StateContainer.GetStateOrThrow<ChaseState>();
        }

        public override void OnBecameActive()
        {
            base.OnBecameActive();

            this.TargetPed = this.ParameterForEnteringState as Ped;
        }

        public override void OnBecameInactive()
        {
            this.TargetPed = null;
            _currentlyEngagedPed = null;

            base.OnBecameInactive();
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

            if (null == _currentlyEngagedPed)
                _currentlyEngagedPed = _chaseState.GetNextPedToAttack();

            if (_currentlyEngagedPed != null)
            {
                _chaseState.UpdateAttackOnPed(_currentlyEngagedPed);
            }
            else
            {
                // no peds to attack
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
    }
}