using UnityEngine;

namespace SanAndreasUnity.Behaviours.Peds.States
{

    public class DriveByFireState : DriveByState, IFireState
    {

        public override void StartFiring()
        {
            // ignore

        }

        public virtual void StopFiring()
        {
            // switch back to drive-by state
            m_ped.GetStateOrLogError<DriveByState>().EnterVehicle(this.CurrentVehicle, this.CurrentVehicleSeatAlignment);
        }

    }

}
