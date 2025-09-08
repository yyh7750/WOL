using System.Threading.Tasks;
using System.Threading;
using WOL.Services.Interface;
using System.Net.Sockets;
using System.Net;
using System.Globalization;
using System;
using System.Diagnostics;

namespace WOL.Services
{
    class WOLClass : UdpClient
    {
        public WOLClass() : base() {}

        public void SetClientToBrodcastMode()
        {
            if (this.Active)
                this.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 0);
        }
    }
 
    public class WakeOnLanService : IWakeOnLanService
    {
        private const int WOL_PORT = 0x2fff;
        private const uint BROADCAST_PORT = 0xffffffff;
        private const uint SHOTDOWN_MESSAGE = 0x010;
        private const uint ALIVE_MESSAGE = 0x020;
        private UdpClient? _sender;
        private byte[]? _buffer;

        private UdpClient? _heartbeatReceiver;
        private CancellationTokenSource? _heartbeatCts;
        private IIniService _iniService;

        public event Action<string>? HeartbeatReceived;

        public WakeOnLanService(IIniService iniService)
        {
            _iniService = iniService;
        }

        public void StartHeartbeatListener()
        {
            if (_heartbeatReceiver != null)
            {
                return;
            }
            _heartbeatCts = new CancellationTokenSource();
            _heartbeatReceiver = new UdpClient(_iniService.HeartbeatRecvPort);
            Task.Run(() => ReceiveHeartbeats(_heartbeatCts.Token));
        }

        public void StopHeartbeatListener()
        {
            _heartbeatCts?.Cancel();
            _heartbeatReceiver?.Close();
            _heartbeatReceiver?.Dispose();
        }

        private async Task ReceiveHeartbeats(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_heartbeatReceiver == null)
                    {
                        throw new InvalidOperationException("Heartbeat receiver is not initialized.");
                    }

                    UdpReceiveResult result = await _heartbeatReceiver.ReceiveAsync(cancellationToken);
                    byte[] receivedBytes = result.Buffer;
                   
                    if (receivedBytes.Length == 4)
                    {
                        uint message = BitConverter.ToUInt32(receivedBytes, 0);
                        if (message == ALIVE_MESSAGE)
                        {
                            string deviceAddress = result.RemoteEndPoint.Address.ToString();
                            HeartbeatReceived?.Invoke(deviceAddress);
                        }
                        else
                        {
                            Debug.WriteLine("Received unknown message: " + message);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Received invalid heartbeat packet.");
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                throw new OperationCanceledException("Heartbeat listener was cancelled.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while receiving heartbeats.", ex);
            }
            finally
            {
                _heartbeatReceiver?.Close();
                _heartbeatReceiver?.Dispose();
            }
        }

        public void WakeUpAsync(string macAddress)
        {
            WOLClass client = new();
            client.Connect(new IPAddress(BROADCAST_PORT), WOL_PORT);
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
        }

        public void ShutdownAsync(string ip)
        {
            IPEndPoint sendIEP = IPEndPoint.Parse(ip + ":" + _iniService.ShutdownSendPort);

            _sender ??= new UdpClient();
            _sender.Connect(sendIEP);

            _buffer = BitConverter.GetBytes(SHOTDOWN_MESSAGE);

            try
            {
                int returnedValue = _sender.Send(_buffer, _buffer.Length);

                if (returnedValue <= 0)
                {
                    throw new Exception("Error sending shutdown packet: 0 bytes sent.");
                }
            }
            catch (SocketException ex)
            {
                throw new Exception($"Error sending shutdown packet: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred while sending shutdown packet: {ex.Message}", ex);
            }
        }
    }
}
