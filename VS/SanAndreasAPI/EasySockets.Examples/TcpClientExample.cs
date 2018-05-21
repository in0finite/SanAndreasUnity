using System;
using System.Text;

namespace MFatihMAR.EasySockets.Examples
{
    public class TcpClientExample
    {
        private TcpClient _client;

        public void Run(ushort port)
        {
            _client = new TcpClient();

            _client.OnOpen += _OnOpen;
            _client.OnConnect += _OnConnect;
            _client.OnData += _OnData;
            _client.OnDisconnect += _OnDisconnect;

            var isAlive = true;
            while (isAlive)
            {
                var input = Console.ReadLine();
                var blocks = input.Split(' ');

                switch (blocks[0])
                {
                    default: Console.WriteLine("commands: isOpen / isConnected / connect <ipep> / send <message> / disconnect / exit"); break;
                    case "isOpen": Console.WriteLine(_client.IsOpen ? "socket open" : "socket closed"); break;
                    case "isConnected": Console.WriteLine(_client.IsConnected ? "socket connected" : "socket disconnected"); break;
                    case "connect":
                        {
                            var ipep = blocks[1].ToIPEP();
                            if (ipep == null)
                            {
                                Console.WriteLine("bad ipendpoint");
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
            }
        }

        private void _OnOpen()
        {
            Console.WriteLine("[open] " + _client.LocalIPEP);
        }

        private void _OnConnect()
        {
            Console.WriteLine("[connect] " + _client.ServerIPEP);
        }

        private void _OnData(byte[] data)
        {
            Console.WriteLine($"[data] ({data.Length}) {Encoding.UTF8.GetString(data)}");
        }

        private void _OnDisconnect(Exception exception)
        {
            Console.WriteLine($"[disconnect] exception: {exception?.Message ?? "null"}");
        }
    }
}
