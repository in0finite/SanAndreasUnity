using UnityEngine;
using SanAndreasUnity.Behaviours.Vehicles;

namespace SanAndreasUnity.Behaviours.Peds.States
{

    public class DriveByState : VehicleSittingState, IAimState
    {
        
        protected override void EnterVehicleInternal()
        {
            m_vehicleParentOffset = Vector3.zero;
            m_model.VehicleParentOffset = Vector3.zero;

			BaseVehicleState.PreparePedForVehicle(m_ped, this.CurrentVehicle, this.CurrentVehicleSeat);

            UpdateAnimsInternal();

        }

        protected override void UpdateAnimsInternal()
        {
            if (this.CurrentVehicleSeat != null)
            {
                var animId = new Importing.Animation.AnimId("drivebys", this.GetAnimBasedOnAimDir());
                m_model.PlayAnim(animId);
                m_model.LastAnimState.wrapMode = WrapMode.ClampForever;
                m_model.VehicleParentOffset = m_model.GetAnim(animId.AnimName).RootEnd;
                m_model.RootFrame.transform.localPosition = Vector3.zero;
            }
        }

        string GetAnimBasedOnAimDir()
        {
            // 4 types: forward, backward, same side, opposite side

            Vector3 aimDir = m_ped.AimDirection;
            Vector3 vehicleDir = this.CurrentVehicle.transform.forward;
            bool isLeftSeat = this.CurrentVehicleSeat.IsLeftHand;
            string leftOrRightLetter = isLeftSeat ? "L" : "R";

            float angle = Vector3.Angle(aimDir, vehicleDir);
            float rightAngle = Vector3.Angle(aimDir, this.CurrentVehicle.transform.right);

            if (angle < 45)
            {
                // aiming forward
                return "Gang_Driveby" + leftOrRightLetter + "HS_Fwd";
            }
            else if (angle < 135)
            {
                // aiming to left or right side
                bool isAimingToLeftSide = rightAngle > 90;
                if (isLeftSeat != isAimingToLeftSide)   // aiming to opposite side
                {
                    return "Gang_DrivebyTop_" + leftOrRightLetter + "HS";
                }
                else    // aiming to same side
                {
                    return "Gang_Driveby" + leftOrRightLetter + "HS";
                }
            }
            else
            {
                // aiming backward
                return "Gang_Driveby" + leftOrRightLetter + "HS_Bwd";
            }

        }

        void IAimState.StartFiring()
        {
            // switch to firing state

        }

        // camera

        public override void OnAimButtonPressed()
        {
            // switch to sitting state
            if (m_isServer)
                m_ped.GetStateOrLogError<VehicleSittingState>().EnterVehicle(this.CurrentVehicle, this.CurrentVehicleSeatAlignment);
            else
                base.OnAimButtonPressed();
        }

    }

}
