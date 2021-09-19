namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public class IdleState : BaseState
    {
        public override void UpdateState()
        {
            _pedAI.StartWalkingAround();
        }
    }
}