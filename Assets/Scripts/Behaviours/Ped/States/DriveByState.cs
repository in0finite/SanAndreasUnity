using UnityEngine;
using SanAndreasUnity.Behaviours.Vehicles;
using System.Collections.Generic;

namespace SanAndreasUnity.Behaviours.Peds.States
{

    public class DriveByState : VehicleSittingState, IAimState
    {
        
        // TODO:
        // - add real aim anims ?
        // - drive-by exiting state - activated when going from drive-by to sitting state, or when trying to exit vehicle
        // - weapon's gun flash should depend on last time when fired, not on anim time - maybe don't change it, because we may play real aim anims



        readonly List<GameObject> m_gameObjectToIgnoreWhenRaycasting = new List<GameObject>();
        readonly List<int> m_layersToIgnoreWhenRaycasting = new List<int>();

        public WeaponAttackParams WeaponAttackParams
        {
            get
            {
                m_gameObjectToIgnoreWhenRaycasting.Clear();
                m_layersToIgnoreWhenRaycasting.Clear();

                if (this.CurrentVehicle != null)
                {
                    m_gameObjectToIgnoreWhenRaycasting.Add(this.CurrentVehicle.gameObject);
                    m_layersToIgnoreWhenRaycasting.Add(LayerMask.NameToLayer(Ped.PedBoneLayerName));

                    if (this.CurrentVehicle.HighDetailMeshesParent != null)
                    {
                        m_gameObjectToIgnoreWhenRaycasting.Add(this.CurrentVehicle.HighDetailMeshesParent.gameObject);
                        m_layersToIgnoreWhenRaycasting.Add(Vehicle.MeshLayer);
                    }

                    return new WeaponAttackParams
                    {
                        GameObjectsToIgnoreWhenRaycasting = m_gameObjectToIgnoreWhenRaycasting,
                        LayersToIgnoreWhenRaycasting = m_layersToIgnoreWhenRaycasting,
                    };
                }

                return WeaponAttackParams.Default;
            }
        }

        public float timeUntilAbleToEnterState = 0.5f;
        public float timeUntilAbleToExitState = 0.5f;

        private double m_lastTimeWhenChangedAnim = 0f;
        private string m_lastAnim = null;
        public float timeUntilAbleToChangeAnim = 0.5f;



        public override void OnBecameInactive()
        {
            m_lastAnim = null;
            base.OnBecameInactive();
        }

        protected override void EnterVehicleInternal()
        {
            m_vehicleParentOffset = Vector3.zero;
            m_model.VehicleParentOffset = Vector3.zero;

			BaseVehicleState.PreparePedForVehicle(m_ped, this.CurrentVehicle, this.CurrentVehicleSeat);

            // only update firing if ped is not currently firing, because otherwise it can cause stack overflow
            // by switching states indefinitely
            // also, we must update firing here, because otherwise shooting rate will be lower
            this.UpdateAnimsInternal(!m_ped.IsFiring);

        }

        public bool CanEnterState(Vehicle vehicle, Vehicle.SeatAlignment seatAlignment)
        {
            if (this.TimeSinceDeactivated < this.timeUntilAbleToEnterState)
                return false;

            var w = m_ped.CurrentWeapon;
            return null != w && w.IsGun;
        }

        public override void UpdateState()
        {
            // exit drive-by state if ped doesn't have gun weapon
            if (m_isServer)
            {
                var w = m_ped.CurrentWeapon;
                if (null == w || !w.IsGun)
                {
                    m_ped.GetStateOrLogError<VehicleSittingState>().EnterVehicle(this.CurrentVehicle, this.CurrentVehicleSeatAlignment);
                    return;
                }
            }

            base.UpdateState();
        }

        protected override void UpdateAnimsInternal()
        {
            this.UpdateAnimsInternal(true);
        }

        void UpdateAnimsInternal(bool bUpdateFiring)
        {
            if (this.CurrentVehicleSeat != null)
            {
                var animId = new Importing.Animation.AnimId("drivebys", this.GetAnimBasedOnAimDirSmoothed());
                m_model.PlayAnim(animId);
                m_model.LastAnimState.wrapMode = WrapMode.ClampForever;

                if (bUpdateFiring)
                {
                    if (m_ped.CurrentWeapon != null)
                    {
                        m_ped.CurrentWeapon.AimAnimState = m_model.LastAnimState;

                        this.UpdateAimAnim(() => BaseAimMovementState.TryFire(m_ped, this.WeaponAttackParams));
                    }
                }

                m_model.VehicleParentOffset = m_model.GetAnim(animId.AnimName).RootEnd;
                m_model.RootFrame.transform.localPosition = Vector3.zero;
            }
        }

        string GetAnimBasedOnAimDirSmoothed()
        {
            if (m_lastAnim != null && Time.timeAsDouble - m_lastTimeWhenChangedAnim < this.timeUntilAbleToChangeAnim)
                return m_lastAnim;

            m_lastTimeWhenChangedAnim = Time.timeAsDouble;

            m_lastAnim = this.GetAnimBasedOnAimDir();
            return m_lastAnim;
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

        void UpdateAimAnim(System.Func<bool> tryFireFunc)
        {
            var ped = m_ped;
            var weapon = ped.CurrentWeapon;
            var state = m_model.LastAnimState;
            float aimAnimMaxTime = state.length * 0.5f;

            if (state.time >= aimAnimMaxTime)
            {
                // keep the anim at max time
                state.time = aimAnimMaxTime;
                ped.AnimComponent.Sample();
                state.enabled = false;

                if (ped.IsFiring)
                {
                    // check if weapon finished firing
                    if (weapon != null && weapon.TimeSinceFired >= (weapon.AimAnimFireMaxTime - weapon.AimAnimMaxTime))
                    {
                        if (Net.NetStatus.IsServer)
                        {
                            ped.StopFiring();
                        }
                    }
                }
                else
                {
                    // check if we should start firing

                    if (ped.IsFireOn && tryFireFunc())
                    {
                        // we started firing

                    }
                    else
                    {
                        // we should remain in aim state
                        
                    }
                }

            }

        }

        public virtual void StartFiring()
        {
            // switch to firing state
            m_ped.GetStateOrLogError<DriveByFireState>().EnterVehicle(this.CurrentVehicle, this.CurrentVehicleSeatAlignment);
        }

        
        public override void UpdateCameraZoom()
        {
            // ignore
        }

        public override void CheckCameraCollision()
        {
            BaseScriptState.CheckCameraCollision(m_ped, this.GetCameraFocusPos(), - m_ped.Camera.transform.forward, this.GetCameraDistance());
        }

        public override Vector3 GetCameraFocusPos()
        {
            var seat = m_ped.CurrentVehicleSeat;
            if (seat != null && seat.Parent != null)
                return seat.Parent.transform.position + Vector3.up * DriveByManager.Instance.cameraHeightOffset;
            else
                return base.GetCameraFocusPos();

            //return m_ped.PlayerModel.Head.transform.position;
        }

        public override float GetCameraDistance()
        {
            return DriveByManager.Instance.cameraBackwardOffset;
        }


        public override void OnAimButtonPressed()
        {
            // switch to sitting state
            if (m_isServer)
            {
                var vehicleSittingState = m_ped.GetStateOrLogError<VehicleSittingState>();
                if (vehicleSittingState.TimeSinceDeactivated < this.timeUntilAbleToExitState)
                    return;
                vehicleSittingState.EnterVehicle(this.CurrentVehicle, this.CurrentVehicleSeatAlignment);
            }
            else
                base.OnAimButtonPressed();
        }

        public virtual Vector3 GetFirePosition()
        {
            return BaseAimMovementState.GetFirePosition(m_ped);
        }

        public virtual Vector3 GetFireDirection()
        {
            return BaseAimMovementState.GetFireDirection(m_ped, () => false, this.WeaponAttackParams);
        }

    }

}
