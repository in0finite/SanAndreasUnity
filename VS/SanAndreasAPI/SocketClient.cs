using System;
using System.Net;
using System.Net.Sockets;
using static SanAndreasAPI.SocketGlobals;

namespace SanAndreasAPI
{
    /// <summary>
    /// Class SocketClientSocket.
    /// </summary>
    public class SocketClient : IDisposable
    {
        #region "Fields"

        // ManualResetEvent instances signal completion.
        public SocketClientConsole myLogger = new SocketClientConsole();

        /// <summary>
        /// The client socket
        /// </summary>
        public Socket ClientSocket;

        /// <summary>
        /// The ip
        /// </summary>
        public IPAddress IP;

        /// <summary>
        /// The port
        /// </summary>
        public ushort Port;

        public ulong Id;

        [Obsolete("Use IPEnd instead.")]
        private IPEndPoint _endpoint;

        //private StateObject stateObject = new StateObject();

        public Action<object, Socket> ReceivedServerMessageCallback;
        public Action OnConnectedCallback;

        #endregion "Fields"

        #region "Properties"

        private SocketState _state;

        public SocketState myState
        {
            get
            {
                return _state;
            }
        }

#pragma warning disable 0618

        internal IPEndPoint IPEnd
        {
            get
            {
                if (IP != null)
                {
                    if (_endpoint == null)
                        _endpoint = new IPEndPoint(IP, Port);
                    return _endpoint;
                }
                else return null;
            }
        }

#pragma warning restore 0618

        public ulong maxReqId
        {
            get
            {
                return Id + ushort.MaxValue;
            }
        }

        #endregion "Properties"

        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClient"/> class.
        /// </summary>
        /// <param name="doConnection">if set to <c>true</c> [do connection].</param>
        public SocketClient(bool doConnection = false) :
            this(IPAddress.Loopback, SocketServer.DefPort, SocketType.Stream, ProtocolType.Tcp, null, doConnection)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClient"/> class.
        /// </summary>
        /// <param name="everyFunc">The every function.</param>
        /// <param name="doConnection">if set to <c>true</c> [do connection].</param>
        public SocketClient(Action<object, Socket> everyFunc, bool doConnection = false) :
            this(IPAddress.Loopback, SocketServer.DefPort, SocketType.Stream, ProtocolType.Tcp, everyFunc, doConnection)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClient"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="doConnection">if set to <c>true</c> [do connection].</param>
        public SocketClient(string ip, ushort port, bool doConnection = false) :
            this(ip, port, null, doConnection)
        { }

        public SocketClient(IPAddress ip, ushort port, bool doConnection = false) :
            this(ip, port, SocketType.Stream, ProtocolType.Tcp, null, doConnection)
        { }

        public SocketClient(IPAddress ip, ushort port, Action<object, Socket> everyFunc, bool doConnection = false) :
            this(ip, port, SocketType.Stream, ProtocolType.Tcp, everyFunc, doConnection)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClient"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="readEvery">The read every.</param>
        /// <param name="everyFunc">The every function.</param>
        /// <param name="doConnection">if set to <c>true</c> [do connection].</param>
        public SocketClient(string ip, ushort port, Action<object, Socket> everyFunc, bool doConnection = false) :
            this(IPAddress.Parse(ip), port, SocketType.Stream, ProtocolType.Tcp, everyFunc, doConnection)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketClient"/> class.
        /// </summary>
        /// <param name="ipAddr">The ip addr.</param>
        /// <param name="port">The port.</param>
        /// <param name="sType">Type of the s.</param>
        /// <param name="pType">Type of the p.</param>
        /// <param name="readEvery">The read every.</param>
        /// <param name="everyFunc">The every function.</param>
        /// <param name="doConnection">if set to <c>true</c> [do connection].</param>
        public SocketClient(IPAddress ipAddr, ushort port, SocketType sType, ProtocolType pType, Action<object, Socket> everyFunc, bool doConnection = false)
        {
            //period = readEvery;

            ReceivedServerMessageCallback = everyFunc;
            //TimerCallback timerDelegate = new TimerCallback(Timering);

            //if (everyFunc != null)
            //    task = new Timer(timerDelegate, null, 5, readEvery);

            IP = ipAddr;
            Port = port;

            ClientSocket = new Socket(ipAddr.AddressFamily, sType, pType)
            {
                NoDelay = false
            };

            if (doConnection)
                DoConnection();
        }

        #endregion "Constructors"

        #region "Socket Methods"

        #region "Timering Methods"

        /// <summary>
        /// Starts the receiving.
        /// </summary>
        /*[Obsolete]
        protected void StartReceiving()
        {
            _Receiving(period);
        }

        /// <summary>
        /// Stops the receiving.
        /// </summary>
        [Obsolete]
        protected void StopReceiving()
        {
            _Receiving();
        }

        [Obsolete]
        private void _Receiving(int p = 0)
        {
            if (task != null)
                task.Change(5, p);
        }

        [Obsolete]
        private void Timering(object stateInfo)
        {
            //Receive();
            //ClientCallback(null);
            //if (deserialize) deserialize = false;
        }*/

        #endregion "Timering Methods"

        public void DoConnection()
        {
            // connect to server async
            try
            {
                ClientSocket.BeginConnect(IPEnd, new AsyncCallback(ConnectToServerCompleted), new SocketGlobals.AsyncSendState(ClientSocket));
            }
            catch (Exception ex)
            {
                myLogger.Log("ConnectToServer error: " + ex.Message);
            }
        }

        public void DisconnectFromServer()
        {
            ClientSocket.Disconnect(false);
        }

        /// <summary>
        /// Fires right when a client is connected to the server.
        /// </summary>
        /// <param name="ar"></param>
        /// <remarks></remarks>
        public void ConnectToServerCompleted(IAsyncResult ar)
        {
            // get the async state object which was returned by the async beginconnect method
            SocketGlobals.AsyncSendState mState = (SocketGlobals.AsyncSendState)ar.AsyncState;

            // end the async connection request so we can check if we are connected to the server
            try
            {
                // call the EndConnect method which will succeed or throw an error depending on the result of the connection
                mState.Socket.EndConnect(ar);

                // at this point, the EndConnect succeeded and we are connected to the server!
                // send a welcome message

                Send(SocketManager.ManagedConn()); // ??? --> el problema estába en que estaba llamado a Socket.Send directamente y estamos dentro de un Socket async xD

                // start waiting for messages from the server
                SocketGlobals.AsyncReceiveState mReceiveState = new SocketGlobals.AsyncReceiveState();
                mReceiveState.Socket = mState.Socket;

                Console.WriteLine("Client ConnectedToServer => CompletedSynchronously: {0}; IsCompleted: {1}", ar.CompletedSynchronously, ar.IsCompleted);

                mReceiveState.Socket.BeginReceive(mReceiveState.Buffer, 0, SocketGlobals.gBufferSize, SocketFlags.None, new AsyncCallback(ServerMessageReceived), mReceiveState);

                //mReceiveState.Socket.Dispose(); // x?x? n
            }
            catch (Exception ex)
            {
                // at this point, the EndConnect failed and we are NOT connected to the server!
                myLogger.Log("Connect error: " + ex.Message);
            }
        }

        public void ServerMessageReceived(IAsyncResult ar)
        {
            // get the async state object from the async BeginReceive method
            SocketGlobals.AsyncReceiveState mState = (SocketGlobals.AsyncReceiveState)ar.AsyncState;

            // call EndReceive which will give us the number of bytes received
            int numBytesReceived = 0;
            numBytesReceived = mState.Socket.EndReceive(ar);

            // determine if this is the first data received
            if (mState.ReceiveSize == 0)
            {
                // this is the first data recieved, so parse the receive size which is encoded in the first four bytes of the buffer
                mState.ReceiveSize = BitConverter.ToInt32(mState.Buffer, 0);
                // write the received bytes thus far to the packet data stream
                mState.PacketBufferStream.Write(mState.Buffer, 4, numBytesReceived - 4);
            }
            else
            {
                // write the received bytes thus far to the packet data stream
                mState.PacketBufferStream.Write(mState.Buffer, 0, numBytesReceived);
            }

            // increment the total bytes received so far on the state object
            mState.TotalBytesReceived += numBytesReceived;
            // check for the end of the packet
            // bytesReceived = Carcassonne.Library.PacketBufferSize Then
            if (mState.TotalBytesReceived < mState.ReceiveSize)
            {
                // ## STILL MORE DATA FOR THIS PACKET, CONTINUE RECEIVING ##
                // the TotalBytesReceived is less than the ReceiveSize so we need to continue receiving more data for this packet
                mState.Socket.BeginReceive(mState.Buffer, 0, SocketGlobals.gBufferSize, SocketFlags.None, new AsyncCallback(ServerMessageReceived), mState);
            }
            else
            {
                // ## FINAL DATA RECEIVED, PARSE AND PROCESS THE PACKET ##
                // the TotalBytesReceived is equal to the ReceiveSize, so we are done receiving this Packet...parse it!
                System.Runtime.Serialization.Formatters.Binary.BinaryFormatter mSerializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                // rewind the PacketBufferStream so we can de-serialize it
                mState.PacketBufferStream.Position = 0;
                // de-serialize the PacketBufferStream which will give us an actual Packet object
                mState.Packet = mSerializer.Deserialize(mState.PacketBufferStream);
                // parse the complete message that was received from the server
                ParseReceivedServerMessage(mState.Packet, mState.Socket);
                // call BeginReceive again, so we can start receiving another packet from this client socket
                SocketGlobals.AsyncReceiveState mNextState = new SocketGlobals.AsyncReceiveState();
                mNextState.Socket = mState.Socket;

                // ???
                mState.PacketBufferStream.Close();
                mState.PacketBufferStream.Dispose();
                mState.PacketBufferStream = null;
                Array.Clear(mState.Buffer, 0, mState.Buffer.Length);

                //Console.WriteLine("Client ServerMessageReceived => CompletedSynchronously: {0}; IsCompleted: {1}", ar.CompletedSynchronously, ar.IsCompleted);

                mNextState.Socket.BeginReceive(mNextState.Buffer, 0, SocketGlobals.gBufferSize, SocketFlags.None, new AsyncCallback(ServerMessageReceived), mNextState);

                //mState.Socket.Dispose(); // x?x? s
                mState = null;
            }
        }

        public void ParseReceivedServerMessage(object obj, Socket argClient)
        {
            Console.WriteLine("Received object of type: {0}", obj.GetType().Name);

            if (obj is string)
                myLogger.Log((string)obj);
            else if (obj is SocketMessage)
                HandleAction((SocketMessage)obj, argClient);
        }

        public void Send(object obj)
        {
            // serialize the Packet into a stream of bytes which is suitable for sending with the Socket
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter mSerializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (System.IO.MemoryStream mSerializerStream = new System.IO.MemoryStream())
            {
                mSerializer.Serialize(mSerializerStream, obj);

                // get the serialized Packet bytes
                byte[] mPacketBytes = mSerializerStream.GetBuffer();

                // convert the size into a byte array
                byte[] mSizeBytes = BitConverter.GetBytes(mPacketBytes.Length + 4);

                // create the async state object which we can pass between async methods
                SocketGlobals.AsyncSendState mState = new SocketGlobals.AsyncSendState(ClientSocket);

                // resize the BytesToSend array to fit both the mSizeBytes and the mPacketBytes
                // ERROR: Not supported in C#: ReDimStatement
                Array.Resize(ref mState.BytesToSend, mPacketBytes.Length + mSizeBytes.Length);

                // copy the mSizeBytes and mPacketBytes to the BytesToSend array
                Buffer.BlockCopy(mSizeBytes, 0, mState.BytesToSend, 0, mSizeBytes.Length);
                Buffer.BlockCopy(mPacketBytes, 0, mState.BytesToSend, mSizeBytes.Length, mPacketBytes.Length);

                Array.Clear(mSizeBytes, 0, mSizeBytes.Length);
                Array.Clear(mPacketBytes, 0, mPacketBytes.Length);

                Console.WriteLine("Ready to send a object of {0} bytes length", mState.BytesToSend.Length);

                ClientSocket.BeginSend(mState.BytesToSend, mState.NextOffset(), mState.NextLength(), SocketFlags.None, new AsyncCallback(MessagePartSent), mState);
            }
        }

        public void SendMessageToServer(string argCommandString)
        {
            Send(argCommandString);
        }

        public void MessagePartSent(IAsyncResult ar)
        {
            // get the async state object which was returned by the async beginsend method
            SocketGlobals.AsyncSendState mState = (SocketGlobals.AsyncSendState)ar.AsyncState;
            try
            {
                int numBytesSent = 0;
                // call the EndSend method which will succeed or throw an error depending on if we are still connected
                numBytesSent = mState.Socket.EndSend(ar);
                // increment the total amount of bytes processed so far
                mState.Progress += numBytesSent;
                // determine if we havent' sent all the data for this Packet yet
                if (mState.NextLength() > 0)
                {
                    // we need to send more data
                    mState.Socket.BeginSend(mState.BytesToSend, mState.NextOffset(), mState.NextLength(), SocketFlags.None, new AsyncCallback(MessagePartSent), mState);
                }
                else
                {
                    Console.WriteLine("Client MessagePartSent => CompletedSynchronously: {0}; IsCompleted: {1}", ar.CompletedSynchronously, ar.IsCompleted);
                    //Console.WriteLine("Client MessagePartSent completed. Clearing stuff...");

                    //Dispose mState when received completed
                    Array.Clear(mState.BytesToSend, 0, mState.BytesToSend.Length);

                    //mState.Socket.Dispose(); // x?x? n
                    mState = null;
                }
                // at this point, the EndSend succeeded and we are ready to send something else!
                // TODO: use the queue to determine what message was sent and show it in the local chat buffer
                //RaiseEvent MessageSentToServer()
            }
            catch (Exception ex)
            {
                myLogger.Log("DataSent error: " + ex.Message);
            }
        }

        #endregion "Socket Methods"

        #region "Class Methods"

        private void HandleAction(SocketMessage sm, Socket argClient)
        {
            //Before we connect we request an id to the master server...
            if (sm.msg is SocketCommand)
            {
                SocketCommand cmd = sm.TryGetObject<SocketCommand>();
                if (cmd != null)
                {
                    switch (cmd.Command)
                    {
                        case SocketCommands.CreateConnId:
                            myLogger.Log("Starting new CLIENT connection with ID: {0}", sm.id);
                            Id = sm.id;

                            Send(SocketManager.ConfirmConnId(Id)); //???
                            OnConnectedCallback?.Invoke();
                            break;

                        case SocketCommands.CloseInstance:
                            myLogger.Log("Client is closing connection...");
                            Stop(false);
                            break;

                        default:
                            myLogger.Log("Unknown ClientCallbackion to take! Case: {0}", cmd);
                            break;
                    }
                }
                else
                {
                    myLogger.Log("Empty string received by client!");
                }
            }
            else
                ReceivedServerMessageCallback?.Invoke(sm.msg, argClient);
        }

        #region "Error & Close & Stop & Dispose"

        private void CloseConnection(SocketShutdown soShutdown)
        {
            if (soShutdown == SocketShutdown.Receive)
            {
                myLogger.Log("Remember that you're in a Client, you can't only close Both connections or only your connection.");
                return;
            }
            if (ClientSocket.Connected)
            {
                ClientSocket.Disconnect(false);
                if (ClientSocket.Connected)
                    ClientSocket.Shutdown(soShutdown);
            }
        }

        private bool disposed;

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                Send(SocketManager.ClientClosed(Id));
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        /// <summary>
        /// Ends this instance.
        /// </summary>
        public void Stop(bool dis)
        {
            if (_state == SocketState.ClientStarted)
            {
                try
                {
                    myLogger.Log("Closing client (#{0})", Id);

                    _state = SocketState.ClientStopped;
                    CloseConnection(SocketShutdown.Both); //No hace falta comprobar si estamos connected

                    if (dis)
                    {
                        ClientSocket.Close();
                        //ClientSocket.Dispose();
                    }
                    else Send(SocketManager.ClientClosed(Id)); //If not disposed then send this
                }
                catch (Exception ex)
                {
                    myLogger.Log("Exception ocurred while trying to stop client: " + ex);
                }
            }
            else
                myLogger.Log("Client cannot be stopped because it hasn't been started!");
        }

        #endregion "Error & Close & Stop & Dispose"

        #endregion "Class Methods"
    }
}