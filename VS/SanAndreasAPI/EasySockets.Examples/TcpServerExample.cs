using SanAndreasAPI;
using System;
using System.Net;
using System.Text;

namespace MFatihMAR.EasySockets.Examples
{
    public class TcpServerExample
    {
        private TcpServer _server;
        private bool flagStart, flagConnect, flagData, flagDisconnect, flagStop;
        public Logger logger;

        public static TcpServerExample Init(bool isUnity, string logPath)
        {
            TcpServerExample s = new TcpServerExample();

            s._server = new TcpServer();
            s.logger = new Logger(logPath, isUnity);

            return s;
        }

        public void SetOnStart(Action start)
        {
            _server.OnStart += () => { start(); };
            flagStart = true;
        }

        public void SetOnConnect(Action<IPEndPoint> connect)
        {
            _server.OnConnect += (a) => { connect(a); };
            flagConnect = true;
        }

        public void SetOnData(Action<IPEndPoint, byte[]> data)
        {
            _server.OnData += (a, b) => { data(a, b); };
            flagData = true;
        }

        public void SetOnDisconnect(Action<IPEndPoint, Exception> disconnect)
        {
            _server.OnDisconnect += (a, b) => { disconnect(a, b); };
            flagDisconnect = true;
        }

        public void SetOnStop(Action<Exception> stop)
        {
            _server.OnStop += (a) => { stop(a); };
            flagStop = true;
        }

        public void Run(ushort port, bool autoStart = true)
        {
            if (!flagStart) _server.OnStart += _OnStart;
            if (!flagConnect) _server.OnConnect += _OnConnect;
            if (!flagData) _server.OnData += _OnData;
            if (!flagDisconnect) _server.OnDisconnect += _OnDisconnect;
            if (!flagStop) _server.OnStop += _OnStop;

            bool isAlive = true;
            while (isAlive)
            {
                var input = autoStart ? "start" : Console.ReadLine();
                var blocks = input.Split(' ');

                switch (blocks[0])
                {
                    default: logger.Log("commands: isListening / start / send <ipep> <message> / disconnect <ipep> / stop / exit"); break;
                    case "start": _server.Start(new IPEndPoint(IPAddress.Any, port)); break;
                    case "isListening": logger.Log(_server.IsListening ? "server listening" : "server not listening"); break;
                    case "send":
                        {
                            if (blocks.Length < 3)
                            {
                                logger.Log("usage: send <ipep> <message>");
                            }
                            else
                            {
                                var ipep = blocks[1].ToIPEP();
                                if (ipep == null)
                                {
                                    logger.Log("bad ipendpoint");
                                }
                                else
                                {
                                    var message = input.Substring(("send " + blocks[1] + " ").Length);
                                    _server.Send(ipep, Encoding.UTF8.GetBytes(message));
                                }
                            }
                        }
                        break;

                    case "disconnect": _server.Disconnect(blocks[1].ToIPEP()); break;
                    case "stop": _server.Stop(); break;
                    case "exit":
                        {
                            if (_server.IsListening)
                            {
                                _server.Stop();
                            }

                            isAlive = false;
                        }
                        break;
                }

                if (autoStart) autoStart = false;
            }
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void _OnStart()
        {
            logger.Log("[start] " + _server.LocalIPEP);
        }

        public void _OnConnect(IPEndPoint remoteIPEP)
        {
            logger.Log("[connect] " + remoteIPEP);
        }

        public void _OnData(IPEndPoint remoteIPEP, byte[] data)
        {
            logger.Log($"[data] {remoteIPEP} ({data.Length}) {Encoding.UTF8.GetString(data)}");
        }

        public void _OnDisconnect(IPEndPoint remoteIPEP, Exception exception)
        {
            logger.Log($"[disconnect] {remoteIPEP} exception: {exception?.Message ?? "null"}");
        }

        public void _OnStop(Exception exception)
        {
            logger.Log($"[stop] exception: {exception?.Message ?? "null"}");
        }
    }
}