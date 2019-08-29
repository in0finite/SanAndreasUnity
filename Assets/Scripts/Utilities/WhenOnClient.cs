

namespace SanAndreasUnity.Utilities
{
    public enum WhenOnClient
    {
        Never = 0,
        OnlyOnOtherClients,
        OnlyOnLocalPlayer,
        Always,
    }

    public static class WhenOnClientExtensions
    {
        public static bool Matches(this WhenOnClient when, bool isLocalPlayer, bool isClient)
        {
            if (when == WhenOnClient.Always)
                return true;
            if (when == WhenOnClient.Never)
                return false;
            if (when == WhenOnClient.OnlyOnLocalPlayer)
                return isLocalPlayer;
            if (when == WhenOnClient.OnlyOnOtherClients)
                return isClient && ! isLocalPlayer;
            return false;
        }
    }
    
}
