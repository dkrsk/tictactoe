using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Mono.Nat;

namespace tictactoe.Network
{
    public class UPnP
    {
        private INatDevice? device;

        public IPAddress ExternalIP { get; }

        public bool IsDeviceFound
        {
            get
            {
                return device != null;
            }
        }

        public UPnP()
        {
            NatUtility.DeviceFound += NatUtility_DeviceFound;
            NatUtility.StartDiscovery();

            int timer = 0;
            while (!IsDeviceFound && timer < 10000)
            {
                timer += 100;
                Thread.Sleep(100);
            }

            ExternalIP = device.GetExternalIP();
        }

        private void NatUtility_DeviceFound(object? sender, DeviceEventArgs e)
        {
            device = e.Device;
            NatUtility.StopDiscovery();
        }

        public async Task CreatePortMapAsync(int port)
        {
            if (device != null && device.GetSpecificMapping(Protocol.Tcp, port).PublicPort == -1)
            {
                await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, port, port));
            }
        }
        public void CreatePortMap(int port)
        {
            if (device != null )//&& device.GetSpecificMapping(Protocol.Tcp, port).PublicPort == -1)
            {
                device.CreatePortMap(new Mapping(Protocol.Tcp, port, port));
            }
        }

        public async Task DeletePortMapAsync(int port)
        {
            try
            {
                if (device != null && device.GetSpecificMapping(Protocol.Tcp, port).PublicPort != -1)
                {
                    await device.DeletePortMapAsync(new Mapping(Protocol.Tcp, port, port));
                }
            }
            catch (Mono.Nat.MappingException)
            {

            }
        }
        public void DeletePortMap(int port)
        {
            try
            {
                if (device != null && device.GetSpecificMapping(Protocol.Tcp, port).PublicPort != -1)
                {
                    device.DeletePortMap(new Mapping(Protocol.Tcp, port, port));
                }
            }
            catch (Mono.Nat.MappingException)
            {

            }
        }
    }
}
