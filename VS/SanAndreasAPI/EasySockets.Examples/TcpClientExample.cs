using SanAndreasAPI;
using System;
using System.Net;
using System.Text;

namespace MFatihMAR.EasySockets.Examples
{
    public class TcpClientExample
    {
        private TcpClient _client;
        private bool flagStart, flagConnect, flagData, flagDisconnect, flagStop;
        public Logger logger;

        public static TcpClientExample Init(bool isUnity, string logPath)
        {
            TcpClientExample c = new TcpClientExample();

            c._client = new TcpClient();
            c.logger = new Logger(logPath, isUnity);

            return c;
        }

        public void SetOnOpen(Action start)
        {
            _client.OnOpen += () => { start(); };
            flagStart = true;
        }

        public void SetOnConnect(Action connect)
        {
            _client.OnConnect += () => { connect(); };
            flagConnect = true;
        }

        public void SetOnData(Action<byte[]> data)
        {
            _client.OnData += (a) => { data(a); };
            flagData = true;
        }

        public void SetOnDisconnect(Action<Exception> disconnect)
        {
            _client.OnDisconnect += (a) => { disconnect(a); };
            flagDisconnect = true;
        }

        public void Run(IPAddress ip, ushort port, bool autoStart = true)
        {
            if (!flagStart) _client.OnOpen += _OnOpen;
            if (!flagConnect) _client.OnConnect += _OnConnect;
            if (!flagData) _client.OnData += _OnData;
            if (!flagDisconnect) _client.OnDisconnect += _OnDisconnect;

            if (autoStart)
                _client.Connect(new IPEndPoint(ip, port));

            /*_client.OnOpen += _OnOpen;
            _client.OnConnect += _OnConnect;
            _client.OnData += _OnData;
            _client.OnDisconnect += _OnDisconnect;*/

            /*var isAlive = true;
            while (isAlive)
            {
                var input = Console.ReadLine();
                var blocks = input.Split(' ');

                switch (blocks[0])
                {
                    default: logger.Log("commands: isOpen / isConnected / connect <ipep> / send <message> / disconnect / exit"); break;
                    case "isOpen": logger.Log(_client.IsOpen ? "socket open" : "socket closed"); break;
                    case "isConnected": logger.Log(_client.IsConnected ? "socket connected" : "socket disconnected"); break;
                    case "connect":
                        {
                            var ipep = blocks[1].ToIPEP();
                            if (ipep == null)
                            {
                                logger.Log("bad ipendpoint");
                            }
                            else
                            {
                                _client.Connect(ipep);
                            }
                        }
                        break;

                    case "send":
                        {
                            var message = input.Substring("send ".Length);
                            _client.Send(Encoding.UTF8.GetBytes(message));
                        }
                        break;

                    case "disconnect": _client.Disconnect(); break;
                    case "exit":
                        {
                            if (_client.IsOpen)
                            {
                                _client.Disconnect();
                            }

                            isAlive = false;
                        }
                        break;
                }
            }*/
        }

        public void Send(object obj)
        {
            try
            {
                _client.Send(obj.Serialize());
            }
            catch (Exception ex)
            {
                if (ex.Message.Length + ex.StackTrace.Length < _client.BufferSize)
                    logger.LogError(ex.Message, ex.StackTrace);
                else
                    logger.LogError("Exception ocurred while sending data though socket client!");
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                _client.Send(data);
            }
            catch (Exception ex)
            {
                if (ex.Message.Length + ex.StackTrace.Length < _client.BufferSize)
                    logger.LogError(ex.Message, ex.StackTrace);
                else
                    logger.LogError("Exception ocurred while sending data though socket client!");
            }
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }

        public void _OnOpen()
        {
            logger.Log("[open] " + _client.LocalIPEP);
        }

        public void _OnConnect()
        {
            logger.Log("[connect] " + _client.ServerIPEP);
        }

        public void _OnData(byte[] data)
        {
            logger.Log($"[data] ({data.Length}) {Encoding.UTF8.GetString(data)}");
        }

        public void _OnDisconnect(Exception exception)
        {
            logger.Log($"[disconnect] exception: {exception?.Message ?? "null"}");
        }
    }
}