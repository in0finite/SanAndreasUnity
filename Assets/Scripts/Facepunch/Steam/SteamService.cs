#if STEAM

using System;
using UnityEngine;
using Steamworks;
using System.Collections.Generic;
using System.Linq;

namespace Facepunch.Steam
{
    public class SteamService : SingletonComponent<SteamService>
    {
        public static AppId_t SteamAppid = new AppId_t(274060);

        public static ulong LocalSteamID { get; private set; }
        public static string LocalName { get; private set; }

#if CLIENT

        public static string CurrentVersion = "unset";

        public static string AvailableVersion
        {
            get
            {
                string betaName = "";
                SteamApps.GetCurrentBetaName(out betaName, 512);
                if (betaName == "development") betaName = "dev";

                return SteamApps.GetAppBuildId() + betaName;
            }
        }

        public static bool IsInitialized { get; private set; }

        protected Callback<P2PSessionRequest_t> CallbackP2PSessionRequest { get; private set; }

        private bool _isOwningInstance;

        private HAuthTicket _authTicket = HAuthTicket.Invalid;

        static SteamService()
        {
            LocalName = "unnamed";
        }

        public SteamService()
        {
            _isOwningInstance = false;
        }

        protected override void OnAwake()
        {
            if (IsInitialized) return;

            if (!SelfTest())
                return;

            if (!IsInitialized) {
                Debug.LogError("[Steamworks.NET] Not initialized!");
                return;
            }

            Debug.Log("Steam Awake");
            _isOwningInstance = true;

            _steamAPIWarningMessageHook = SteamAPIDebugTextHook;
            SteamClient.SetWarningMessageHook(_steamAPIWarningMessageHook);

            LocalSteamID = SteamUser.GetSteamID().m_SteamID;
            LocalName = SteamFriends.GetPersonaName();

            CallbackP2PSessionRequest = Callback<P2PSessionRequest_t>.Create(CBACK_P2PSessionRequest);
        }

        private void CBACK_P2PSessionRequest(P2PSessionRequest_t rq)
        {
            //
            // TODO: This needs to verify that they're in this game with us!
            //
            SteamNetworking.AcceptP2PSessionWithUser(rq.m_steamIDRemote);
        }

        private SteamAPIWarningMessageHook_t _steamAPIWarningMessageHook;

        private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning("[Steam] " + pchDebugText);
        }

// ReSharper disable once UnusedMember.Local
        private void Update()
        {
            if (!IsInitialized) return;

            Profiler.BeginSample("SteamClient.RunCallbacks");
            SteamAPI.RunCallbacks();
            Profiler.EndSample();

            Profiler.BeginSample("SteamAvatarCache.Cycle");
            SteamAvatarCache.Cycle();
            Profiler.EndSample();

            Profiler.BeginSample("SteamNetworking.P2P");
            uint DataAvailable = 0;
            while (SteamNetworking.IsP2PPacketAvailable(out DataAvailable)) {
                var dest = new byte[DataAvailable];
                CSteamID steamid;
                if (SteamNetworking.ReadP2PPacket(dest, DataAvailable, out DataAvailable, out steamid)) {
                    ProcessSteamNetworkingPacket(dest, steamid);
                }
            }
            Profiler.EndSample();
        }

        public void ProcessSteamNetworkingPacket(byte[] data, CSteamID steamid)
        {
            throw new NotImplementedException();
        }

// ReSharper disable once UnusedMember.Local
        private void OnDisable()
        {
            if (_isOwningInstance)
            {
                CancelAuthTicket();

                SteamAPI.Shutdown();

                IsInitialized = false;
            }
        }

        public void CancelAuthTicket()
        {
            if (_authTicket != Steamworks.HAuthTicket.Invalid)
            {
                SteamUser.CancelAuthTicket(_authTicket);

                _authTicket = Steamworks.HAuthTicket.Invalid;
            }
        }

        public byte[] GetAuthSessionTicket()
        {
            CancelAuthTicket();

            byte[] data = new byte[1024];
            uint dataLength = 0;

            _authTicket = SteamUser.GetAuthSessionTicket(data, data.Length, out dataLength);

            return data.Take((int)dataLength).ToArray();
        }

        private class SteamAvatarCache
        {
            private static readonly List<SteamAvatarCache> _avatarsLoading = new List<SteamAvatarCache>();
            private static readonly List<SteamAvatarCache> _avatars = new List<SteamAvatarCache>();

            private Texture2D _texture = null;
            private ulong _steamid = 0;
            private int _imageId = -1;

            public static Texture FindTexture(ulong steamid)
            {
                var found = _avatars.Find(x => x._steamid == steamid);
                if (found != null) {
                    return found._texture;
                }

                var created = new SteamAvatarCache() {
                    _texture = new Texture2D(64, 64, TextureFormat.ARGB32, false),
                    _steamid = steamid,
                    _imageId = SteamFriends.GetMediumFriendAvatar(new CSteamID(steamid))
                };

                created._texture.filterMode = FilterMode.Trilinear;
                created._texture.wrapMode = TextureWrapMode.Clamp;
                created._texture.anisoLevel = 8;

                for (int x = 0; x < created._texture.width; x++) {
                    for (int y = 0; y < created._texture.height; y++) {
                        created._texture.SetPixel(x, y, new Color32(0, 0, 0, 255));
                    }
                }

                created._texture.Apply();

                _avatars.Add(created);
                _avatarsLoading.Add(created);

                created.Load();

                return created._texture;
            }

            public void Load()
            {
                _imageId = SteamFriends.GetMediumFriendAvatar(new CSteamID(_steamid));
                if (_imageId <= 0)
                    return;

                uint w, h;

                if (!SteamUtils.GetImageSize(_imageId, out w, out h))
                    return;

                if (w != _texture.width && h != _texture.height) {
                    _avatarsLoading.Remove(this);
                    return;
                }

                int destBufferSize = (int) (4 * w * h);
                byte[] destBuffer = new byte[destBufferSize];

                if (!SteamUtils.GetImageRGBA(_imageId, destBuffer, destBufferSize))
                    return;

                for (int x = 0; x < w; x++) {
                    for (int y = 0; y < h; y++) {
                        int index = ((int) y * (int) w + (int) x) * 4;
                        byte r = destBuffer[index + 0];
                        byte g = destBuffer[index + 1];
                        byte b = destBuffer[index + 2];
                        byte a = destBuffer[index + 3];

                        _texture.SetPixel(x, (int) h - 1 - y, new Color32(r, g, b, a));
                    }
                }

                _texture.Apply();
                _avatarsLoading.Remove(this);
            }

            public static void Cycle()
            {
                foreach (var avatar in _avatarsLoading.ToArray()) {
                    avatar.Load();
                }
            }
        }

        public static Texture GetAvatarTexture(ulong steamid)
        {
            SteamFriends.RequestUserInformation(new CSteamID(steamid), false);

            return SteamAvatarCache.FindTexture(steamid);
        }
#endif

        public static bool SelfTest()
        {
            if (!Packsize.Test()) {
                Debug.LogError(
                    "[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.");
                return false;
            }

            if (!DllCheck.Test()) {
                Debug.LogWarning(
                    "[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.");
            }

            try {
                if (SteamAPI.RestartAppIfNecessary(SteamAppid)) {
                    Application.Quit();
                    return false;
                }
            } catch (DllNotFoundException e) {
                Debug.LogError(
                    "[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" +
                    e);
                Application.Quit();
                return false;
            }

#if CLIENT

            IsInitialized = SteamAPI.Init();
            if (!IsInitialized) {
                Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Is Steam running?");
                Application.Quit();
                return false;
            }

            CurrentVersion = AvailableVersion;
#endif

            return true;
        }
    }
}

#endif