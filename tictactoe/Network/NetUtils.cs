using System;
using System.Net;
using System.Net.Sockets;


namespace tictactoe.Network
{
    public static class NetCodes
    {
        public const short Connect = 410;
        public const short Disconnect = 143;
        public const short Revenge = 169;
    }

    //public enum NetCodes
    //{
    //    Connect = 410,
    //    Disconnect = 143,
    //    Revenge = 69
    //}

    public class PortPair
    {
        private int localPort;
        private int remotePort;
        public int LPort { get { return localPort; } set { localPort = value; } }
        public int RPort { get { return remotePort; } set { remotePort = value; } }

        public PortPair(int lPort, int rPort)
        {
            this.localPort = lPort;
            this.remotePort = rPort;
        }

        public PortPair() : this(25566, 25566)
        {
        }
    }

    public static class NetUtils
    {
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
