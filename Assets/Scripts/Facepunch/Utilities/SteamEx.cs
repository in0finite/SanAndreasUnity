#if STEAM

using Steamworks;

namespace Facepunch.Steam
{
    public static class SteamEx
    {
        public static string Format(this CSteamID steamID)
        {
            if (steamID.GetEAccountType() == EAccountType.k_EAccountTypeInvalid ||
                steamID.GetEAccountType() == EAccountType.k_EAccountTypeIndividual) {
                uint accountID = steamID.GetAccountID().m_AccountID;

                if (steamID.GetEUniverse() <= EUniverse.k_EUniversePublic) {
                    return string.Format("STEAM_0:{0}:{1}", accountID & 1, accountID >> 1);
                } else {
                    return string.Format("STEAM_{2}:{0}:{1}", accountID & 1, accountID >> 1,
                        (int) steamID.GetEUniverse());
                }
            } else {
                return steamID.ToString();
            }
        }
    }
}

#endif