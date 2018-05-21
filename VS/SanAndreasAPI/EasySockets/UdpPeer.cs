using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MFatihMAR.EasySockets
{
    public class UdpPeer
    {
        public delegate void StartEvent();

        public delegate void DataEvent(IPEndPoint remoteIPEP, byte[] data);

        public delegate void StopEvent(Exception exception = null);

        public event StartEvent OnStart;

        public event DataEvent OnData;

        public event StopEvent OnStop;

        private ValueWrapper<bool> _isOpen;
        public bool IsOpen => _isOpen?.Value ?? false;
        public IPEndPoint LocalIPEP { get; private set; }
        public ushort BufferSize { get; private set; }

        private Socket _socket;
        private Thread _thread;

        public void Start(IPEndPoint localIPEP, ushort bufferSize = 512)
        {
            _Cleanup();
            if (bufferSize < 64)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            LocalIPEP = localIPEP ?? throw new ArgumentNullException(nameof(localIPEP));
            BufferSize = bufferSize;

            _isOpen = new ValueWrapper<bool>(true);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.Bind(localIPEP);

            _thread = new Thread(_ReceiveThread);
            _thread.Start();

            LocalIPEP = (IPEndPoint)_socket.LocalEndPoint;

            OnStart?.Invoke();
        }

        public void Send(IPEndPoint ipep, byte[] data)
        {
            if (!IsOpen)
            {
                return;
            }

            if (ipep == null)
            {
                throw new ArgumentNullException(nameof(ipep));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length < 1 || data.Length > BufferSize)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            _socket.SendTo(data, ipep);
        }

        public void Stop()
        {
            _Cleanup();
            OnStop?.Invoke();
        }

        private void _Cleanup()
        {
            try
            {
                _isOpen.Value = false;

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
                var buffer = new byte[BufferSize];
                var remoteIPEP = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

                while (isOpen.Value)
                {
                    var recSize = _socket.ReceiveFrom(buffer, 0, BufferSize, SocketFlags.None, ref remoteIPEP);
                    var packet = new byte[recSize];
                    Array.Copy(buffer, packet, recSize);
                    OnData?.Invoke((IPEndPoint)remoteIPEP, packet);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                _Cleanup();
                OnStop?.Invoke(ex);
            }
        }
    }
}