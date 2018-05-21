using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MFatihMAR.EasySockets
{
    public class TcpClient
    {
        public delegate void OpenEvent();
        public delegate void ConnectEvent();
        public delegate void DataEvent(byte[] data);
        public delegate void DisconnectEvent(Exception exception = null);

        public event OpenEvent OnOpen;
        public event ConnectEvent OnConnect;
        public event DataEvent OnData;
        public event DisconnectEvent OnDisconnect;

        private ValueWrapper<bool> _isOpen;
        public bool IsOpen => _isOpen?.Value ?? false;

        private ValueWrapper<bool> _isConnected;
        public bool IsConnected => IsOpen && (_isConnected?.Value ?? false);

        public IPEndPoint LocalIPEP { get; private set; }
        public IPEndPoint ServerIPEP { get; private set; }
        public ushort BufferSize { get; private set; }

        private Socket _socket;
        private Thread _thread;

        public void Connect(IPEndPoint serverIPEP, ushort bufferSize = 512)
        {
            _Cleanup();

            if (serverIPEP == null)
            {
                throw new ArgumentNullException(nameof(serverIPEP));
            }

            if (bufferSize < 64)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            ServerIPEP = serverIPEP;
            BufferSize = bufferSize;

            _isOpen = new ValueWrapper<bool>(true);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Bind(new IPEndPoint(IPAddress.Any, 0));

            _thread = new Thread(_ReceiveThread);
            _thread.Start();

            LocalIPEP = (IPEndPoint)_socket.LocalEndPoint;

            OnOpen?.Invoke();
        }

        public void Send(byte[] data)
        {
            if (!IsConnected)
            {
                return;
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length < 1 || data.Length > BufferSize)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            _socket.Send(data);
        }

        public void Disconnect()
        {
            _Cleanup();
            OnDisconnect?.Invoke();
        }

        private void _Cleanup()
        {
            try
            {
                _isOpen.Value = false;
                _isConnected.Value = false;

                _socket?.Close();

                if (_thread != null && _thread.IsAlive)
                {
                    _thread.Abort();
                }
            }
            catch
            {
            }
        }

        private void _ReceiveThread()
        {
            try
            {
                var isOpen = _isOpen;
                if (!isOpen.Value)
                {
                    return;
                }

                _socket.Connect(ServerIPEP);
                _isConnected = new ValueWrapper<bool>(true);
                OnConnect?.Invoke();

                var buffer = new byte[BufferSize];
                while (isOpen.Value)
                {
                    var recSize = _socket.Receive(buffer, 0, BufferSize, SocketFlags.None);

                    if (recSize == 0)
                    {
                        OnDisconnect?.Invoke();
                        _Cleanup();
                        return;
                    }

                    var packet = new byte[recSize];
                    Array.Copy(buffer, packet, recSize);
                    OnData?.Invoke(packet);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                OnDisconnect?.Invoke(ex);
                _Cleanup();
            }
        }
    }
}
