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

        public void StartProgramAsync(Device device, Program program)
        {
            SendProgramSignalAsync(device, program, true).Wait();
        }

        public void StopProgramAsync(Device device, Program program)
        {
            SendProgramSignalAsync(device, program, false).Wait();
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

        public async Task SendProgramSignalAsync(Device device, Program program, bool isStart)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(program);
            if (_iniService == null) throw new InvalidOperationException("IIniService is not initialized.");

            _udpService ??= new UdpService(device.IP, _iniService.ProgramSignalPort);

            string? runProgramPath = null;
            foreach (Program p in device.Programs)
            {
                if (p.Path == program.Path)
                {
                    runProgramPath = p.Path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(runProgramPath))
            {
                throw new ArgumentException("The specified program does not belong to the given device.");
            }

            ProgramDto sendData = new()
            {
                Path = runProgramPath,
                IsStart = isStart
            };

            await _udpService.SendAsync(sendData);
        }
    }
}