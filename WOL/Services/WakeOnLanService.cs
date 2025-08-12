using WOL.Services.Interface;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using System;

namespace WOL.Services
{
    public class WakeOnLanService : IWakeOnLanService
    {
        public void WakeUpAsync(string macAddress)
        {
            WOLClass client = new WOLClass();
            client.Connect(new IPAddress(0xffffffff), 0x2fff);
            client.SetClientToBrodcastMode();
            
            int counter = 0;
            
            byte[] bytes = new byte[1024];
            
            for (int y = 0; y < 6; y++)
                bytes[counter++] = 0xFF;
 
            for (int y = 0; y < 16; y++)
            {
                int i = 0;
                for (int z = 0; z < 6; z++)
                {
                    bytes[counter++] = byte.Parse(macAddress.Substring(i, 2), NumberStyles.HexNumber);
                    i += 2;
                }
            }

            int reterned_value = client.Send(bytes, 1024);
            if (reterned_value < 0)
            {
                throw new Exception("Error sending WOL packet");
            }
            else
            {
                Console.WriteLine("WOL packet sent successfully.");
            }
        }
    }

    public class WOLClass : UdpClient
    {
        public WOLClass()
            : base()
        { }
        //this is needed to send broadcast packet
        public void SetClientToBrodcastMode()
        {
            if (this.Active)
                this.Client.SetSocketOption(SocketOptionLevel.Socket,
                                          SocketOptionName.Broadcast, 0);
        }
    }
}