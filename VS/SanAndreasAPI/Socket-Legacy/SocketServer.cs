using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using static SanAndreasAPI.SocketGlobals;

namespace SanAndreasAPI
{
    /// <summary>
    /// Class SocketServer.
    /// </summary>
    public class SocketServer : IDisposable
    {
        #region "Fields"

        public SocketServerConsole myLogger = new SocketServerConsole();

        /// <summary>
        /// The lerped port
        /// </summary>
        public const int DefPort = 7776;

        /// <summary>
        /// The server socket
        /// </summary>
        public Socket ServerSocket;

        /// <summary>
        /// The permision
        /// </summary>
        public SocketPermission Permision;

        /// <summary>
        /// The ip
        /// </summary>
        public IPAddress IP;

        /// <summary>
        /// The port
        /// </summary>
        public int Port;

        [Obsolete("Use IPEnd instead.")]
        private IPEndPoint _endpoint;

        /// <summary>
        /// All done
        /// </summary>
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        /// <summary>
        /// The routing table
        /// </summary>
        public static Dictionary<ulong, Socket> routingTable = new Dictionary<ulong, Socket>(); //With this we can assume that ulong.MaxValue clients can connect to the Socket (2^64 - 1)

        public Action<object, Socket> ReceivedClientMessageCallback;

        private static bool dispose, debug;

        //Check this vvv

        public event ClientConnectedEventHandler ClientConnected;

        public event MessageReceivedEventHandler MessageReceived;

        public event ClientDisconnectedEventHandler ClientDisconnected;

        public delegate void ClientConnectedEventHandler(Socket argClientSocket);

        public delegate void MessageReceivedEventHandler(string argMessage, Socket argClientSocket);

        public delegate void ClientDisconnectedEventHandler(Socket argClientSocket);

        #endregion "Fields"

        #region "Propierties"

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

        #endregion "Propierties"

        #region "Socket Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketServer"/> class.
        /// </summary>
        /// <param name="debug">if set to <c>true</c> [debug].</param>
        /// <param name="doConnection">if set to <c>true</c> [do connection].</param>
        public SocketServer(bool debug, bool doConnection = false) :
            this(new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", SocketPermission.AllPorts), Dns.GetHostEntry("").AddressList[0], DefPort, SocketType.Stream, ProtocolType.Tcp, debug, doConnection)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketServer"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="debug">if set to <c>true</c> [debug].</param>
        /// <param name="doConnection">if set to <c>true</c> [do connection].</param>
        public SocketServer(string ip, int port, bool debug, bool doConnection = false) :
            this(new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", SocketPermission.AllPorts), IPAddress.Parse(ip), port, SocketType.Stream, ProtocolType.Tcp, debug, doConnection)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketServer"/> class.
        /// </summary>
        /// <param name="permission">The permission.</param>
        /// <param name="ipAddr">The ip addr.</param>
        /// <param name="port">The port.</param>
        /// <param name="sType">Type of the s.</param>
        /// <param name="pType">Type of the p.</param>
        /// <param name="curDebug">if set to <c>true</c> [current debug].</param>
        /// <param name="doConnection">if set to <c>true</c> [do connection].</param>
        public SocketServer(SocketPermission permission, IPAddress ipAddr, int port, SocketType sType, ProtocolType pType, bool curDebug, bool doConnection = false)
        {
            permission.Demand();

            IP = ipAddr;
            Port = port;

            debug = curDebug;

            ServerSocket = new Socket(ipAddr.AddressFamily, sType, pType);

            if (doConnection)
                StartServer(); // ??? --> ComeAlive
        }

        #endregion "Socket Constructors"

        #region "Socket Methods"

        public void InitializeServer()
        {
        }

        /// <summary>
        /// StartServer starts the server by listening for new client connections with a TcpListener.
        /// </summary>
        /// <remarks></remarks>
        public void StartServer()
        {
            // create the TcpListener which will listen for and accept new client connections asynchronously

            // bind to the server's ipendpoint
            ServerSocket.Bind(IPEnd);

            // configure the listener to allow 1 incoming connection at a time
            ServerSocket.Listen(1000);

            // accept client connection async
            ServerSocket.BeginAccept(new AsyncCallback(ClientAccepted), ServerSocket);
        }

        public void StopServer()
        {
            //cServerSocket.Disconnect(True)
            ServerSocket.Close();
            //cStopRequested = True
        }

        /// <summary>
        /// ClientConnected is a callback that gets called when the server accepts a client connection from the async BeginAccept method.
        /// </summary>
        /// <param name="ar"></param>
        /// <remarks></remarks>
        public void ClientAccepted(IAsyncResult ar)
        {
            // get the async state object from the async BeginAccept method, which contains the server's listening socket
            Socket mServerSocket = (Socket)ar.AsyncState;
            // call EndAccept which will connect the client and give us the the client socket
            Socket mClientSocket = null;
            try
            {
                mClientSocket = mServerSocket.EndAccept(ar);
            }
            catch //(ObjectDisposedException ex)
            {
                // if we get an ObjectDisposedException it that means the server socket terminated while this async method was still active
                return;
            }
            // instruct the client to begin receiving data
            SocketGlobals.AsyncReceiveState mState = new SocketGlobals.AsyncReceiveState();
            mState.Socket = mClientSocket;
            ClientConnected?.Invoke(mState.Socket);
            mState.Socket.BeginReceive(mState.Buffer, 0, SocketGlobals.gBufferSize, SocketFlags.None, new AsyncCallback(ClientMessageReceived), mState);
            // begin accepting another client connection
            mServerSocket.BeginAccept(new AsyncCallback(ClientAccepted), mServerSocket);

            //mServerSocket.Dispose(); // x?x?
            //Console.WriteLine("Server ClientAccepted => CompletedSynchronously: {0}; IsCompleted: {1}", ar.CompletedSynchronously, ar.IsCompleted);
        }

        /// <summary>
        /// BeginReceiveCallback is an async callback method that gets called when the server receives some data from a client socket after calling the async BeginReceive method.
        /// </summary>
        /// <param name="ar"></param>
        /// <remarks></remarks>
        public void ClientMessageReceived(IAsyncResult ar)
        {
            // get the async state object from the async BeginReceive method
            SocketGlobals.AsyncReceiveState mState = (SocketGlobals.AsyncReceiveState)ar.AsyncState;

            // call EndReceive which will give us the number of bytes received
            int numBytesReceived = 0;
            try
            {
                numBytesReceived = mState.Socket.EndReceive(ar);
            }
            catch (SocketException ex)
            {
                // if we get a ConnectionReset exception, it could indicate that the client has disconnected
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    ClientDisconnected?.Invoke(mState.Socket);
                    return;
                }
            }
            // if we get numBytesReceived equal to zero, it could indicate that the client has disconnected
            if (numBytesReceived == 0)
            {
                ClientDisconnected?.Invoke(mState.Socket);
                return;
            }

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
                mState.Socket.BeginReceive(mState.Buffer, 0, SocketGlobals.gBufferSize, SocketFlags.None, new AsyncCallback(ClientMessageReceived), mState);
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
                Console.WriteLine("Succesfully deserialized object of type: {0}", mState.Packet.GetType().Name);
                // handle the message
                ParseReceivedClientMessage(mState.Packet, mState.Socket);
                // call BeginReceive again, so we can start receiving another packet from this client socket
                SocketGlobals.AsyncReceiveState mNextState = new SocketGlobals.AsyncReceiveState();
                mNextState.Socket = mState.Socket;

                // ???
                mState.PacketBufferStream.Close();
                mState.PacketBufferStream.Dispose();
                mState.PacketBufferStream = null;
                Array.Clear(mState.Buffer, 0, mState.Buffer.Length);

                Console.WriteLine("Server ClientMessageReceived => CompletedSynchronously: {0}; IsCompleted: {1}", ar.CompletedSynchronously, ar.IsCompleted);

                mNextState.Socket.BeginReceive(mNextState.Buffer, 0, SocketGlobals.gBufferSize, SocketFlags.None, new AsyncCallback(ClientMessageReceived), mNextState);

                //mState.Socket.Dispose(); // x?x?
                mState = null;
            }
        }

        public void ParseReceivedClientMessage(object obj, Socket argClient)
        {
            //Aquí ocurre la magia
            if (obj is string)
            {
                string argCommandString = (string)obj;
                myLogger.Log("ParseReceivedClientMessage: " + argCommandString);

                // parse the command string
                string argCommand = null;
                string argText = null;

                if (argCommandString.StartsWith("/"))
                {
                    argCommand = argCommandString.Substring(0, argCommandString.IndexOf(" "));
                    argText = argCommandString.Remove(0, argCommand.Length + 1);
                }
                else
                    argText = argCommandString;

                switch (argText)
                {
                    case "hi server":
                        SendMessageToClient("/say Server replied.", argClient);
                        break;
                }

                MessageReceived?.Invoke(argCommandString, argClient);
            }
            else if (obj is SocketMessage)
                HandleAction((SocketMessage)obj, argClient);

            //This is a server-only method, that is called for "Debugging purpouses" by the moment
            ReceivedClientMessageCallback?.Invoke(obj, argClient);
            //Check there in case of ((SocketMessage)obj).DestsId[0] == 0??
        }

        /// <summary>
        /// QueueMessage prepares a Message object containing our data to send and queues this Message object in the OutboundMessageQueue.
        /// </summary>
        /// <remarks></remarks>
        public void SendToClient(object obj, Socket argClient)
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
                SocketGlobals.AsyncSendState mState = new SocketGlobals.AsyncSendState(argClient);

                // resize the BytesToSend array to fit both the mSizeBytes and the mPacketBytes
                // TODO: ReDim mState.BytesToSend(mPacketBytes.Length + mSizeBytes.Length - 1)
                Array.Resize(ref mState.BytesToSend, mPacketBytes.Length + mSizeBytes.Length);

                // copy the mSizeBytes and mPacketBytes to the BytesToSend array
                Buffer.BlockCopy(mSizeBytes, 0, mState.BytesToSend, 0, mSizeBytes.Length);
                Buffer.BlockCopy(mPacketBytes, 0, mState.BytesToSend, mSizeBytes.Length, mPacketBytes.Length);

                Array.Clear(mSizeBytes, 0, mSizeBytes.Length);
                Array.Clear(mPacketBytes, 0, mPacketBytes.Length);

                Console.Write("");

                // queue the Message
                Console.WriteLine("Server (SendToClient): NextOffset: {0}; NextLength: {1}", mState.NextOffset(), mState.NextLength());
                argClient.BeginSend(mState.BytesToSend, mState.NextOffset(), mState.NextLength(), SocketFlags.None, new AsyncCallback(MessagePartSent), mState);
            }
        }

        /// <summary>
        /// QueueMessage prepares a Message object containing our data to send and queues this Message object in the OutboundMessageQueue.
        /// </summary>
        /// <remarks></remarks>
        public void SendMessageToClient(string argCommandString, Socket argClient)
        {
            SendToClient(argCommandString, argClient);
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
                    Console.WriteLine("Server MessagePartSent completed. Clearing stuff...");

                    Array.Clear(mState.BytesToSend, 0, mState.BytesToSend.Length);

                    //mState.Socket.Dispose(); // x?x? n
                    mState = null;
                }

                // at this point, the EndSend succeeded and we are ready to send something else!
            }
            catch (Exception ex)
            {
                myLogger.Log("DataSent error: " + ex.Message);
            }
        }

        #endregion "Socket Methods"

        #region "Class Methods"

        private void HandleAction(SocketMessage sm, Socket handler)
        {
            //string val = sm.StringValue;
            if (sm.msg is SocketCommand)
            {
                SocketCommand cmd = sm.TryGetObject<SocketCommand>();
                if (cmd != null)
                {
                    switch (cmd.Command)
                    {
                        case SocketCommands.Conn:
                            //Para que algo se añade aqui debe ocurrir algo...
                            //Give an id for a client before we add it to the routing table
                            //and create a request id for the next action that needs it

                            //First, we have to assure that there are free id on the current KeyValuePair to optimize the process...
                            ulong genID = 1;

                            //Give id in a range...
                            bool b = routingTable.Keys.FindFirstMissingNumberFromSequenceUlong(out genID, new MinMax<ulong>(1, (ulong)routingTable.Count));
                            myLogger.Log("Adding #{0} client to routing table!", genID); //Esto ni parece funcionar bien

                            SendToClient(SocketManager.SendConnId(genID), handler);
                            break;

                        case SocketCommands.ConfirmConnId:
                            routingTable.Add(sm.id, handler);
                            break;

                        case SocketCommands.CloseClients:
                            CloseAllClients(sm.id);
                            break;

                        case SocketCommands.ClosedClient:
                            //closedClients.Add(sm.id);
                            SocketManager.PoliteClose(sm.id); //Tell to the client that it has been disconnected from it
                            routingTable.Remove(sm.id);
                            CloseServerAfterClientsClose(dispose);
                            break;

                        case SocketCommands.Stop:
                            CloseAllClients(sm.id);
                            break;

                        case SocketCommands.UnpoliteStop:
                            object d = cmd.Metadata["Dispose"];
                            Stop(d != null && ((bool)d));
                            break;

                        default:
                            DoServerError(string.Format("Cannot de-encrypt the message! Unrecognized 'enum' case: {0}", cmd.Command), sm.id);
                            break;
                    }
                }
            }
            else
                //If not is a command, then send the object to other clients...
                SendToClient(sm, sm.DestsId);
        }

        #region "Send Methods"

        private void SendToAllClients(SocketMessage sm, object obj = null, int bytesRead = 0)
        {
            myLogger.Log("---------------------------");
            if (bytesRead > 0) myLogger.Log("Client with ID {0} sent {1} bytes (JSON).", sm.id, bytesRead);
            myLogger.Log("Sending to the other clients.");
            myLogger.Log("---------------------------");
            myLogger.Log("");

            //Send to the other clients
            foreach (KeyValuePair<ulong, Socket> soc in routingTable)
                if (soc.Key != sm.id)
                    SendToClient(obj == null ? sm : obj, soc.Value); // ??? <-- byteData ??
        }

        private void SendToClient(SocketMessage sm, object obj = null, params ulong[] dests)
        {
            SendToClient(sm, dests.AsEnumerable(), obj);
            dests = null;
        }

        private void SendToClient(SocketMessage sm, IEnumerable<ulong> dests, object obj = null)
        {
            if (dests == null)
            {
                Console.WriteLine("Can't send null list of Destinations!");
                return;
            }
            if (dests.Count() == 1)
            {
                if (dests.First() == ulong.MaxValue)
                { //Send to all users
                    foreach (KeyValuePair<ulong, Socket> soc in routingTable)
                        if (soc.Key != sm.id)
                            SendToClient(obj == null ? sm : obj, soc.Value);
                }
            }
            else if (dests.Count() > 1)
            { //Select dictionary keys that contains dests
                foreach (KeyValuePair<ulong, Socket> soc in routingTable.Where(x => dests.Contains(x.Key)))
                    if (soc.Key != sm.id)
                        SendToClient(obj == null ? sm : obj, soc.Value);
            }
            else
            {
                //Error
                Console.WriteLine("Destinations var isn't null, but it's length is 0.");
            }
        }

        #endregion "Send Methods"

        #region "Error & Close & Stop & Dispose"

        private void DoServerError(string msg, ulong id = 0, bool dis = false)
        {
            PoliteStop(dis, id);
            myLogger.Log("{0} CLOSING SERVER due to: " + msg,
                id == 0 ? "" : string.Format("(FirstClient: #{0})", id));
        }

        private void CloseAllClients(ulong id = 0)
        {
            if (id > 0) SendToClient(SocketManager.PoliteClose(id), routingTable[id]); //First, close the client that has send make the request...
            myLogger.Log("Closing all {0} clients connected!", routingTable.Count);
            foreach (KeyValuePair<ulong, Socket> soc in routingTable)
            {
                if (soc.Key != id) //Then, close the others one
                {
                    myLogger.Log("Sending to CLIENT #{0} order to CLOSE", soc.Key);
                    SendToClient(SocketManager.PoliteClose(soc.Key), soc.Value);
                }
            }
        }

        private void CloseServerAfterClientsClose(bool dis)
        {
            if (routingTable.Count == 0)
                Stop(dis); //Close the server, when all the clients has been closed.
        }

        public void PoliteStop(bool dis = false, ulong id = 0)
        {
            dispose = dis;
            CloseAllClients(id); //And then, the server will autoclose itself...
        }

        /// <summary>
        /// Closes the server.
        /// </summary>
        private void Stop(bool dis = true)
        {
            if (_state == SocketState.ServerStarted)
            {
                try
                {
                    myLogger.Log("Closing server");

                    _state = SocketState.ServerStopped;
                    if (ServerSocket.Connected) //Aqui lo que tengo que hacer es que se desconecten los clientes...
                        ServerSocket.Shutdown(SocketShutdown.Both);

                    ServerSocket.Close();

                    if (dis)
                    { //Dispose
                        //ServerSocket.Dispose();
                        ServerSocket = null;
                    }
                }
                catch (Exception ex)
                {
                    myLogger.Log("Exception ocurred while trying to stop server: " + ex);
                }
            }
            else
                myLogger.Log("Server cannot be stopped because it hasn't been started!");
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
                myLogger.Log("Disposing server");
                PoliteStop(true);
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }

        #endregion "Error & Close & Stop & Dispose"

        #endregion "Class Methods"
    }
}