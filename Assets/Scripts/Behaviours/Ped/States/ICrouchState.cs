namespace SanAndreasUnity.Behaviours.Peds.States
{
    public interface ICrouchState : IPedState
    {

    }

    public static class ICrouchStatePedExtensions
    {
        public static bool IsCrouching(this Ped ped)
        {
            return ped.CurrentState is ICrouchState;
        }
    }
}
