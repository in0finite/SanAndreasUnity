//using Facepunch.Networking;
//using ProtoBuf.Player;

namespace SanAndreasUnity.Behaviours
{
    public partial class Player
    {
#if PROTOBUF

        package ProtoBuf.Player;

        message PlayerInfo
        {
            required uint64 UserId = 1;
            optional string Username = 2;
            optional int32 Modelid = 3;
        }

        //:baseclass = INetworkableSpawnMessage
        message PlayerSpawn
        {
            required ProtoBuf.NetworkableInfo Networkable = 1;
            optional PlayerInfo Player = 2;
            optional PlayerPedestrianState PedestrianState = 3;
            optional PlayerPassengerState PassengerState = 5;
            optional bool IsLocal = 4;
        }

        //:baseclass = ISnapshot, INetworkableMessage
        message PlayerPedestrianState
        {
            optional ProtoBuf.NetworkableInfo Networkable = 1;
            required double Timestamp = 2;

            //:interpolate
            //:smoothing = 0.5
            optional UnityEngine.Vector3 Position = 3;

            //:interpolate
            optional UnityEngine.Vector3 Movement = 4;

            //:interpolate
            //:angle
            //:smoothing = 0.5
            optional float Yaw = 5;

            optional bool Running = 6;
        }

        //:baseclass = INetworkableMessage
        message PlayerPassengerState
        {
            optional ProtoBuf.NetworkableInfo Networkable = 1;

            optional ProtoBuf.NetworkableInfo Vechicle = 2;
            optional int32 SeatAlignment = 3 [default=0];
        }

#endif

        #region Private fields

        //     private readonly SnapshotBuffer<PlayerPedestrianState> _snapshots
        //         = new SnapshotBuffer<PlayerPedestrianState>(.25);

        //     private PlayerPassengerState _lastPassengerState;

        #endregion Private fields

        #region Properties

        //        public IRemote Remote { get; private set; }
        public ulong UserId { get; private set; }

        public string Username { get; private set; }

        public bool IsLocalPlayer { get; private set; }

        #endregion Properties

        /*    internal void ServerSideInit(IRemote remote, ulong userId, string username, int modelId)
            {
                Remote = remote;
                UserId = userId;
                Username = username;
                PlayerModel.PedestrianId = modelId;

                remote.Disconnected += (sender, reason) => Destroy(gameObject);

                name = string.Format("Player ({0})", Username);
            }

            private PlayerPedestrianState GetPedestrianSnapshot()
            {
                if (IsInVehicle) return null;

                return new PlayerPedestrianState {
                    Position = Position,
                    Movement = Movement,
                    Yaw = Quaternion.FromToRotation(Vector3.forward, Heading).eulerAngles.y,
                    Running = PlayerModel.Running,
                    Timestamp = NetTime
                };
            }

            private void UpdateFromPedestrianSnapshot(PlayerPedestrianState message)
            {
                if (IsLocalPlayer) return;
                if (transform.parent != null) return;

                Position = message.Position;
                Movement = message.Movement;
                Heading = Quaternion.AngleAxis(message.Yaw, Vector3.up) * Vector3.forward;

                if (Movement.sqrMagnitude > 0f) {
                    if (message.Running) {
                        PlayerModel.Running = message.Running;
                    } else {
                        PlayerModel.Walking = true;
                    }
                } else {
                    PlayerModel.Walking = false;
                }
            }

        //    private PlayerPassengerState _pendingPassenger;

            private void UpdateFromPassengerSnapshot(PlayerPassengerState message)
            {
                if (IsLocalPlayer) return;
                if (!IsInVehicle && message.Vechicle == null) return;
                if (IsInVehicle && CurrentVehicle.Info.Equals(message.Vechicle)) return;

                _pendingPassenger = null;

                if (message.Vechicle == null) {
                    ExitVehicle();
                } else {
                    var vehicle = Client.GetNetworkable<Vehicle>(message.Vechicle);

                    if (vehicle == null) {
                        _pendingPassenger = message;
                        return;
                    }

                    if (!IsInVehicle) {
                        EnterVehicle(vehicle,
                            (Vehicle.SeatAlignment) message.SeatAlignment);
                    } else {
                        ExitVehicle(true);
                        EnterVehicle(vehicle, (Vehicle.SeatAlignment) message.SeatAlignment, true);
                    }
                }
            }
        */

#if CLIENT

        [ClientSpawnMethod]
        private static Player Spawn(PlayerSpawn message)
        {
            return SpawnFromPrefab<Player>();
        }

        [MessageHandler(Domain.Client)]
        private void OnSpawn(IRemote sender, PlayerSpawn message)
        {
            UserId = message.Player.UserId;
            Username = message.Player.Username;
            PlayerModel.PedestrianId = message.Player.Modelid;

            name = string.Format("Player ({0})", Username);

            if (message.PedestrianState != null) {
                UpdateFromPedestrianSnapshot(message.PedestrianState);
            } else {
                UpdateFromPassengerSnapshot(message.PassengerState);
            }

            if (message.IsLocal) {
                IsLocalPlayer = true;
                SetupLocalPlayer();
            }
        }

        protected override void OnClientNetworkingUpdate()
        {
            if (!IsLocalPlayer) return;

            if (!IsInVehicle && transform.parent == null) {
                SendToServer(GetPedestrianSnapshot(), DeliveryMethod.UnreliableSequenced, 2);
            }
        }

        [MessageHandler(Domain.Client)]
        private void OnReceiveMessageFromServer(IRemote sender, PlayerPedestrianState message)
        {
            if (IsLocalPlayer) return;

            _snapshots.Add(message);
        }

        [MessageHandler(Domain.Client)]
        private void OnReceiveMessageFromServer(IRemote sender, PlayerPassengerState message)
        {
            if (IsLocalPlayer) return;

            UpdateFromPassengerSnapshot(message);
        }
#endif

        /*
                private void NetworkingFixedUpdate()
                {
                    if (_pendingPassenger != null) {
                        UpdateFromPassengerSnapshot(_pendingPassenger);
                    }

                    if (IsInVehicle) return;
                    if (IsLocalPlayer) return;

                    if (_snapshots.Update()) UpdateFromPedestrianSnapshot(_snapshots.Current);
                }

                protected override void OnFirstObserve(IEnumerable<IRemote> clients)
                {
                    var msg = new PlayerSpawn {
                        Player = new PlayerInfo {
                            UserId = UserId,
                            Username = Username,
                            Modelid = PlayerModel.PedestrianId
                        },
                        PedestrianState = GetPedestrianSnapshot(),
                        PassengerState = _lastPassengerState
                    };

                    SendToClients(msg, clients.Where(x => x != Remote), DeliveryMethod.ReliableOrdered, 1);

                    if (!clients.Contains(Remote)) return;

                    msg.IsLocal = true;

                    SendToClient(msg, Remote, DeliveryMethod.ReliableOrdered, 1);
                }

                [MessageHandler(Domain.Server)]
                private void OnReceiveMessageFromClient(IRemote sender, PlayerPedestrianState message)
                {
                    if (sender != Remote) return;

        #if CLIENT
                    if (!IsClient) {
                        _snapshots.Add(message);
                    }
        #endif

                    SendToClients(message, Group.Subscribers.Where(x => x != Remote),
                        DeliveryMethod.UnreliableSequenced, 2);
                }

                [MessageHandler(Domain.Server)]
                private void OnReceiveMessageFromClient(IRemote sender, PlayerPassengerState message)
                {
                    if (sender != Remote) return;

        #if CLIENT
                    if (!IsClient) {
                        UpdateFromPassengerSnapshot(message);
                    }
        #endif

                    SendToClients(message, Group.Subscribers.Where(x => x != Remote),
                        DeliveryMethod.ReliableOrdered, 1);
                }
        */
    }
}