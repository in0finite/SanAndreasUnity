using SanAndreasUnity.Importing.Animation;

namespace SanAndreasUnity.Behaviours.Peds.States
{
    public class SurrenderState : BaseMovementState
    {
        public override AnimId movementAnim => new AnimId("ped", "handsup");
        public override AnimId movementWeaponAnim => this.movementAnim;


        protected override void SwitchToMovementState()
        {
            // prevent switching to Stand state

            System.Type type = BaseMovementState.GetMovementStateToSwitchToBasedOnInput(m_ped);

            if (typeof(StandState).IsAssignableFrom(type))
                return;

            base.SwitchToMovementState();
        }

        protected override void UpdateAnims()
        {
            base.UpdateAnims();
            
            if (!this.IsActiveState)
                return;

            if (m_model.LastAnimState != null)
                m_model.LastAnimState.wrapMode = UnityEngine.WrapMode.ClampForever;
        }

        public override void OnSurrenderButtonPressed()
        {
            if (m_isServer)
                this.SwitchToMovementStateIfEnoughTimePassed(typeof(StandState));
            else
                base.OnSurrenderButtonPressed();
        }
    }

    public static class SurrenderStatePedExtensions
    {
        public static bool IsSurrendering(this Ped ped) => ped.CurrentState is SurrenderState;
    }
}
