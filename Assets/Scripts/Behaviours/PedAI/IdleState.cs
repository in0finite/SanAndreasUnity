using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class IdleState : BaseState
    {
        public override void UpdateState()
        {
            _pedAI.StartWalkingAround();
        }

        protected internal override void OnMyPedDamaged(DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            this.HandleOnMyPedDamaged(dmgInfo, dmgResult);
        }

        public void HandleOnMyPedDamaged(DamageInfo dmgInfo, Ped.DamageResult dmgResult)
        {
            base.OnMyPedDamaged(dmgInfo, dmgResult);

            Ped attackerPed = dmgInfo.GetAttackerPed();

            Ped hitPed = this.MyPed;

            if (hitPed.PedDef != null &&
                (hitPed.PedDef.DefaultType.IsCriminal() ||
                 hitPed.PedDef.DefaultType.IsCop() ||
                 hitPed.PedDef.DefaultType.IsGangMember()))
            {
                if (attackerPed != null)
                {
                    if (_pedAI.StateContainer.GetStateOrThrow<ChaseState>().CanStartChasing())
                        _pedAI.StartChasing();
                    else
                        _pedAI.StartEscaping();
                }
            }
            else
                _pedAI.StartEscaping();

        }

        protected internal override void OnRecruit(Ped recruiterPed)
        {
            this.HandleOnRecruit(recruiterPed);
        }

        public void HandleOnRecruit(Ped recruiterPed)
        {
            if (!_pedAI.PedestrianType.IsGangMember() && !_pedAI.PedestrianType.IsCriminal())
                return;

            _pedAI.StartFollowing(recruiterPed);
        }
    }
}