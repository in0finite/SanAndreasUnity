#if STEAM

using System;
using System.Collections.Generic;
using Steamworks;

namespace Facepunch.Steam
{
    public class SteamServerRequest
    {
        public struct Response
        {
            public string name;
            public int players;
            public int maxplayers;
            public string map;
            public int ping;
            public string address;
            public uint ip;
            public int port;
            public int queryPort;
            public int id;
            public string gamedir;
            public string description;
            public ulong steamid;
        }

        public struct Filter
        {
            public String key;
            public String value;
        }

        public enum RequestType
        {
            Internet,
            Friends,
            History,
            LAN,
            Favourites
        }

        private HServerListRequest _handle = HServerListRequest.Invalid;
        private ISteamMatchmakingServerListResponse _responder = null;

        public delegate void ResponseDelegate(Response response);
        private ResponseDelegate _onResponse;

        private List<MatchMakingKeyValuePair_t> _filters = new List<MatchMakingKeyValuePair_t>();

        public string GameDirectory { get; set; }
        public bool Secure { get; set; }

        public void SetFilters(Filter[] filters)
        {
            _filters.Clear();

            AddFilter("gamedir", GameDirectory);
            AddFilter("secure", Secure ? "1" : "0");

            for (int i = 0; i < filters.Length; ++i)
            {
                AddFilter(filters[i]);
            }
        }

        public void AddFilter(string key, string value)
        {
            _filters.Add(new MatchMakingKeyValuePair_t { m_szKey = key, m_szValue = value });
        }

        public void AddFilter(Filter filter)
        {
            AddFilter(filter.key, filter.value);
        }

        public void CancelRequest()
        {
            if (!SteamService.IsInitialized) return;

            if (_handle != HServerListRequest.Invalid)
            {
                SteamMatchmakingServers.ReleaseRequest(_handle);
                _handle = HServerListRequest.Invalid;
            }
        }

        public void Request(RequestType type, ResponseDelegate onResponse)
        {
            if (!SteamService.IsInitialized) return;

            if (_responder == null)
            {
                _responder = new ISteamMatchmakingServerListResponse(OnResponded, OnFailedToRespond, OnRefreshComplete);
            }

            CancelRequest();

            _onResponse = onResponse;

            var filters = _filters.ToArray();
            uint filterCount = (uint)_filters.Count;

            var appid = SteamService.SteamAppid;

            switch (type)
            {
                case RequestType.Favourites:
                    _handle = SteamMatchmakingServers.RequestFavoritesServerList(appid, filters, filterCount, _responder);
                    break;

                case RequestType.Friends:
                    _handle = SteamMatchmakingServers.RequestFriendsServerList(appid, filters, filterCount, _responder);
                    break;

                case RequestType.History:
                    _handle = SteamMatchmakingServers.RequestHistoryServerList(appid, filters, filterCount, _responder);
                    break;

                case RequestType.Internet:
                    _handle = SteamMatchmakingServers.RequestInternetServerList(appid, filters, filterCount, _responder);
                    break;

                case RequestType.LAN:
                    _handle = SteamMatchmakingServers.RequestLANServerList(appid, _responder);
                    break;
            }
        }

        private void OnResponded(HServerListRequest request, int server)
        {
            var serverDetails = SteamMatchmakingServers.GetServerDetails(request, server);

            var response = new Response
            {
                id = server,
                name = serverDetails.GetServerName(),
                players = serverDetails.m_nPlayers,
                maxplayers = serverDetails.m_nMaxPlayers,
                map = serverDetails.GetMap(),
                ping = serverDetails.m_nPing,
                ip = serverDetails.m_NetAdr.GetIP(),
                address = serverDetails.m_NetAdr.GetConnectionAddressString(),
                port = serverDetails.m_NetAdr.GetConnectionPort(),
                queryPort = serverDetails.m_NetAdr.GetQueryPort(),
                gamedir = serverDetails.GetGameDir(),
                description = serverDetails.GetGameDescription(),
                steamid = serverDetails.m_steamID.m_SteamID,
            };

            _onResponse(response);
        }

        private void OnFailedToRespond(HServerListRequest request, int server)
        {
        }

        private void OnRefreshComplete(HServerListRequest request, EMatchMakingServerResponse response)
        {
            CancelRequest();
        }

        public static void Favourite(bool favourite, uint ip, int port, int queryPort)
        {
            if (!SteamService.IsInitialized) return;

            if (favourite)
            {
                Steamworks.SteamMatchmaking.AddFavoriteGame(SteamService.SteamAppid, ip, (ushort)port, (ushort)queryPort, (uint)Steamworks.Constants.k_unFavoriteFlagFavorite, 0);
            }
            else
            {
                Steamworks.SteamMatchmaking.RemoveFavoriteGame(SteamService.SteamAppid, ip, (ushort)port, (ushort)queryPort, (uint)Steamworks.Constants.k_unFavoriteFlagFavorite);
            }
        }

        public static bool IsFavourited(uint ip, int port)
        {
            if (!SteamService.IsInitialized) return false;

            int count = SteamMatchmaking.GetFavoriteGameCount();

            for (int i = 0; i < count; ++i)
            {
                Steamworks.AppId_t appid;
                uint pnIP;
                ushort pnConnPort;
                ushort pnQueryPort;
                uint punFlags;
                uint pRTime32LastPlayedOnServer;

                if (Steamworks.SteamMatchmaking.GetFavoriteGame(i, out appid, out pnIP, out pnConnPort, out pnQueryPort, out punFlags, out pRTime32LastPlayedOnServer) == false)
                {
                    continue;
                }

                if (appid != SteamService.SteamAppid)
                {
                    continue;
                }

                if ((((int)punFlags) & Steamworks.Constants.k_unFavoriteFlagFavorite) != Steamworks.Constants.k_unFavoriteFlagFavorite)
                {
                    continue;
                }

                if (pnIP != ip)
                {
                    continue;
                }

                if (pnConnPort != port)
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}

#endif