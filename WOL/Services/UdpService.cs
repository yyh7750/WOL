using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WOL.Models.Dto;
using WOL.Services.Interface;

namespace WOL.Services
{
    class UdpService : IUdpService
    {
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _endPoint;

        public UdpService(string ip, int port)
        {
            _udpClient = new UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public async Task SendAsync(ProgramDto dto)
        {
            string json = JsonSerializer.Serialize(dto);
            byte[] data = Encoding.UTF8.GetBytes(json);

            await _udpClient.SendAsync(data, data.Length, _endPoint);
            Debug.WriteLine($"Sent: {json} \n {_endPoint.Address}:{_endPoint.Port}");
        }
    }
}
