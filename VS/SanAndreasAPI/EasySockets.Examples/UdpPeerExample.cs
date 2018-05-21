using System;
using System.Net;
using System.Text;

namespace MFatihMAR.EasySockets.Examples
{
    public class UdpPeerExample
    {
        private UdpPeer _peer;

        public void Run(ushort port)
        {
            _peer = new UdpPeer();

            _peer.OnStart += _OnStart;
            _peer.OnData += _OnData;
            _peer.OnStop += _OnStop;

            var isAlive = true;
            while (isAlive)
            {
                var input = Console.ReadLine();
                var blocks = input.Split(' ');

                switch (blocks[0])
                {
                    default: Console.WriteLine("commands: isOpen / start / send <ipep> <message> / stop / exit"); break;
                    case "isOpen": Console.WriteLine(_peer.IsOpen ? "socket open" : "socket closed"); break;
                    case "start": _peer.Start(new IPEndPoint(IPAddress.Any, port)); break;
                    case "send":
                        {
                            if (blocks.Length < 3)
                            {
                                Console.WriteLine("usage: send <ipep> <message>");
                            }
                            else
                            {
                                var ipep = blocks[1].ToIPEP();
                                if (ipep == null)
                                {
                                    Console.WriteLine("bad ipendpoint");
                                }
                                else
                                {
                                    var message = input.Substring(("send " + blocks[1] + " ").Length);
                                    _peer.Send(ipep, Encoding.UTF8.GetBytes(message));
                                }
                            }
                        }
                        break;
                    case "stop": _peer.Stop(); break;
                    case "exit":
                        {
                            if (_peer.IsOpen)
                            {
                                _peer.Stop();
                            }

                            isAlive = false;
                        }
                        break;
                }
            }
        }

        private void _OnStart()
        {
            Console.WriteLine("[start] " + _peer.LocalIPEP);
        }

        private void _OnData(IPEndPoint remoteIPEP, byte[] data)
        {
            Console.WriteLine($"[data] {remoteIPEP} ({data.Length}) {Encoding.UTF8.GetString(data)}");
        }

        private void _OnStop(Exception exception)
        {
            Console.WriteLine($"[stop] exception: {exception?.Message ?? "null"}");
        }
    }
}
