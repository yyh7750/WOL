using System.Net.NetworkInformation;
using System.Net;
using System.Threading.Tasks;
using WOL.Services.Interface;
using System.Linq;
using System.Net.Sockets;
using WOL.Models;
using System;
using WOL.Models.Dto;

namespace WOL.Services
{
    public class ProgramService : IProgramService
    {
        private IUdpService? _udpService;
        private IIniService? _iniService;

        public ProgramService(IIniService iniService)
        {
            _iniService = iniService;
        }

        public Task StartAllProgramsAsync()
        {
            // view model에서 프로그램 리스트 받아온 다음 프로그램 별로 StartProgramAsync 호출
            throw new System.NotImplementedException();
        }

        public void StartProgramAsync(Device device)
        {
            SendProgramSignalAsync(device, true).Wait();
        }

        public Task StopAllProgramsAsync()
        {
            // view model에서 프로그램 리스트 받아온 다음 프로그램 별로 StopProgramAsync 호출
            throw new NotImplementedException();
        }

        public void StopProgramAsync(Device device)
        {
            SendProgramSignalAsync(device, false).Wait();
        }

        public bool IsMyIpAddress(string ip)
        {
            if (!IPAddress.TryParse(ip, out IPAddress? inputIp))
            {
                return false;
            }

            // 내 PC의 모든 네트워크 인터페이스 확인
            var myAddresses = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Select(ua => ua.Address)
                .Where(addr => addr.AddressFamily == AddressFamily.InterNetwork); // IPv4만

            return myAddresses.Any(myIp => myIp.Equals(inputIp));
        }

        public async Task SendProgramSignalAsync(Device device, bool isStart)
        {
            ArgumentNullException.ThrowIfNull(device);
            if (_iniService == null) throw new InvalidOperationException("IIniService is not initialized.");

            _udpService ??= new UdpService(device.IP, _iniService.ProgramSignalPort);

            ProgramDto sendData = new()
            {
                Paths = device.Programs.Select(p => p.Path).ToList(),
                IsStart = isStart
            };

            await _udpService.SendAsync(sendData);
        }
    }
}