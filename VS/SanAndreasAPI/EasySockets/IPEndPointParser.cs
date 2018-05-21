using System.Net;

namespace MFatihMAR.EasySockets
{
    public static class IPEndPointParser
    {
        public static IPEndPoint ToIPEP(this string source)
        {
            var blocks = source.Split(':');
            if (blocks.Length != 2)
            {
                return null;
            }

            IPAddress addr;
            if (!IPAddress.TryParse(blocks[0], out addr))
            {
                return null;
            }

            ushort port;
            if (!ushort.TryParse(blocks[1], out port))
            {
                return null;
            }

            return new IPEndPoint(addr, port);
        }

        public static IPEndPoint Parse(string ipepStr)
        {
            return ipepStr.ToIPEP();
        }
    }
}
