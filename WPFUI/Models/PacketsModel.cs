using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WPFUI.Models
{
    internal class PacketsModel
    {
        private const int DESTINATION_PORT = 65100;
        private const byte payloadVersion = 1;
        private const byte minPayloadVersion = 1;
        private short packetSize = 140;
        public string DESTINATION_IP = "255.255.255.255";

        UdpClient udp = new UdpClient();
        List<byte> data = new List<byte>();

        static List<NetworkInterface> _interfaces = new List<NetworkInterface>();
        static List<UdpClient> _udpClients = new List<UdpClient>();
        public PacketsModel()
        {
        }

        public static void SetInterfaces(List<NetworkInterface> interfaces)
        {
            _interfaces = interfaces;
            _udpClients.Clear();

            //var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in _interfaces)
            {
                if (ni.OperationalStatus == OperationalStatus.Up && ni.SupportsMulticast && ni.GetIPProperties().GetIPv4Properties() != null)
                {
                    int id = ni.GetIPProperties().GetIPv4Properties().Index;
                    if (NetworkInterface.LoopbackInterfaceIndex != id)
                    {
                        foreach (UnicastIPAddressInformation uip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (uip.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                IPEndPoint local = new IPEndPoint(uip.Address, 0);
                                UdpClient udpc = new UdpClient(local);
                                udpc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                                udpc.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
                                _udpClients.Add(udpc);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public void sendPackets(double[] packetDoubles, TimeModel time)
        {
            data.Clear();
            //add versions and packet size individually to the byte list
            data.Add(payloadVersion);
            data.Add(minPayloadVersion);
            data.AddRange(BitConverter.GetBytes(packetSize));

            foreach (double d in packetDoubles)
            {
                data.AddRange(BitConverter.GetBytes(d));
            }

            //add time separately to the byte list
            data.AddRange(BitConverter.GetBytes(time.GetGPSMillis()));
            data.AddRange(BitConverter.GetBytes(time.GetGPSWeek()));

            // convert list of bytes into an array to send using UdpClient.Send();
            byte[] packet = data.ToArray();

            sendBroadcast(packet);
        }

        IPEndPoint target = new IPEndPoint(IPAddress.Broadcast, DESTINATION_PORT);
        private void sendBroadcast(byte[] data)
        {
            foreach (var c in _udpClients)
            {
                c.Send(data, data.Length, target);
            }
        }

    }


}

