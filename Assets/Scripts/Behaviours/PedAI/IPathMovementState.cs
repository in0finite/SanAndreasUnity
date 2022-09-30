using UGameCore.Utilities;

namespace SanAndreasUnity.Behaviours.Peds.AI
{
    public interface IPathMovementState : IState
    {
        PathMovementData PathMovementData { get; }
    }
}
