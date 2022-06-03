using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace tictactoe.Network
{
    public class NetPeer
    {
        IPAddress broadcastAddress;

        public int RemotePort { get; }
        public int LocalPort { get; }

        private readonly UPnP upnp;

        TcpClient sendingClient = new();
        TcpListener receivingClient;

        private bool connected = false;
        public bool IsConnected { get { return connected; } }

        public delegate void ReceiveHandler(short message);
        public event ReceiveHandler MessageReceived;

        CancellationTokenSource receiverTokenSource = new CancellationTokenSource();
        CancellationToken receiverToken;

        public IPAddress ExternalIP
        {
            get
            {
                return upnp.ExternalIP;
            }
        }
        public IPAddress LocalIP { get; } = NetUtils.GetLocalIPAddress();

        public NetPeer(IPAddress broadcastAddress, PortPair portPair)
        {
            RemotePort = portPair.RPort;
            LocalPort = portPair.LPort;

            upnp = new();

            this.broadcastAddress = broadcastAddress;
            receivingClient = InitializeReceiver(LocalPort);
            Task.Run(async () => await InitializeSender(RemotePort));
            receiverToken = receiverTokenSource.Token;

            MessageReceived += (short message) => Debug.WriteLine("Received: {0}", message);
        }

        public NetPeer(string broadcastAddress, PortPair portPair) : this(IPAddress.Parse(broadcastAddress), portPair)
        {
        }

        public NetPeer(IPAddress broadcastAddress) : this(broadcastAddress, new PortPair())
        {
        }

        public NetPeer(string broadcastAddress) : this(IPAddress.Parse(broadcastAddress), new PortPair())
        {
        }

        public async Task InitializeSender(int port)
        {
            while (!sendingClient.Connected)
            {
                Debug.WriteLine("Trying Connect");
                if (receiverToken.IsCancellationRequested)
                {
                    return;
                }
                try
                {
                    await this.sendingClient.ConnectAsync(this.broadcastAddress, port);
                }
                catch
                {
                    Thread.Sleep(10);
                }
            }
            this.connected = true;
        }

        public TcpListener InitializeReceiver(int port)
        {
            if(broadcastAddress.ToString() == "127.0.0.1")
            {
                this.receivingClient = new(broadcastAddress, port);
            }
            else
            {
                upnp.CreatePortMap(port);
                this.receivingClient = new(LocalIP, port);
            }

            Task receiverTask = new(async () => 
            {
                try
                {
                    await ReceiverThread(receivingClient);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            });
            receiverTask.Start();

            return receivingClient;
        }

        private async Task ReceiverThread(TcpListener server)
        {
            server.Start();
            TcpClient client = await server.AcceptTcpClientAsync();

            while (true)
            {
                if (receiverToken.IsCancellationRequested)
                {
                    //server.Stop();
                    return;
                }

                NetworkStream stream = client.GetStream();

                byte[] data = new byte[2];

                try
                {
                    if (sendingClient.Connected && client.Connected)
                    {
                        await stream.ReadAsync(data, receiverToken);
                    }
                }
                catch (System.IO.IOException)
                {
                    MessageReceived.Invoke(NetCodes.Disconnect);
                    Close();
                }

                if (data[0] != 0)
                {
                    MessageReceived.Invoke((short)(BitConverter.ToInt16(data) - 1));
                }
            }
        }

        public async Task Send(short cell)
        {
            try
            {
                if (sendingClient.Connected)
                {
                    byte[] data = BitConverter.GetBytes(cell + 1);
                    NetworkStream stream = sendingClient.GetStream();
                    await stream.WriteAsync(data, 0, data.Length);
                }
            }
            catch (System.IO.IOException)
            {
                MessageReceived.Invoke(NetCodes.Disconnect);
                Close();
            }
        }

        public void Close()
        {
            Send(NetCodes.Disconnect).GetAwaiter().GetResult();
            receiverTokenSource.Cancel();
            receivingClient.Stop();
            sendingClient.Close();
            upnp.DeletePortMap(LocalPort);
        }
    }
}
