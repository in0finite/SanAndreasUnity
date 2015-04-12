using Facepunch.Networking;
using ProtoBuf;
using ProtoBuf.Player;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours.Networking
{
    public class Client : Facepunch.Networking.Client
    {

#if PROTOBUF
        
        package ProtoBuf;

        message ConnectRequestData
        {
            required int32 ModelId = 1;
        }

#endif

        public Player LocalPlayer { get; private set; }

        protected override void OnPrepareConnectRequest(ProtoBuf.ConnectRequest request)
        {
            request.Data = new ConnectRequestData {
                ModelId = Config.Get<int>("cl_model_id")
            }.ToProtoBytes();
        }

        protected override void OnDispatchNetworkableMessage(Facepunch.Networking.Networkable target, IRemote sender, INetworkableMessage message)
        {
            var plySpawn = message as PlayerSpawn;
            if (plySpawn != null && plySpawn.IsLocal) {
                LocalPlayer = (Player) target;
            }

            base.OnDispatchNetworkableMessage(target, sender, message);
        }
    }
}
