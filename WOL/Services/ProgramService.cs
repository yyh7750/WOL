using System.Net.NetworkInformation;
using System.Net;
using System.Threading.Tasks;
using WOL.Services.Interface;
using System.Linq;
using System.Net.Sockets;
using WOL.Models;
using System;
using WOL.Models.Dto;
using System.Diagnostics;
using System.IO;
using System.Windows;

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
            string? runProgramPath = program.Path;

            if (string.IsNullOrEmpty(runProgramPath))
            {
                throw new ArgumentException("The specified program does not belong to the given device.");
            }

            foreach (Program p in device.Programs)
            {
                if (p.Path == program.Path)
                {
                    runProgramPath = p.Path;
                    break;
                }
            }

            if (IsMyIpAddress(device.IP))
            {
                ProcessStartInfo startInfo = new()
                {
                    FileName = runProgramPath, // 파일 주소 및 파일명, 확장자까지 포함
                    Verb = "runas", // 관리자 권한 실행
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(runProgramPath)
                };

                Process? process = Process.Start(startInfo);

                if (process != null)
                {
                    if (process.HasExited)
                    {
                        MessageBox.Show($"{runProgramPath} 연결된 프로세스가 종료되었습니다.");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show($"{runProgramPath} 시작할 수 없습니다.");
                    return;
                }
            }
            else
            {
                SendProgramSignalAsync(device, runProgramPath, true).Wait();
            }
        }

        public void StopProgramAsync(Device device, Program program)
        {
            string? runProgramPath = program.Path;

            if (string.IsNullOrEmpty(runProgramPath))
            {
                throw new ArgumentException("The specified program does not belong to the given device.");
            }

            foreach (Program p in device.Programs)
            {
                if (p.Path == program.Path)
                {
                    runProgramPath = p.Path;
                    break;
                }
            }

            if (IsMyIpAddress(device.IP))
            {
                var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(runProgramPath));
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to kill process {process.Id}: {ex.Message}");
                    }
                }
            }
            else
            {
                SendProgramSignalAsync(device, runProgramPath, false).Wait();
            }
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

        public async Task SendProgramSignalAsync(Device device, string runProgramPath, bool isStart)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(runProgramPath);
            if (_iniService == null) throw new InvalidOperationException("IIniService is not initialized.");

            _udpService ??= new UdpService(device.IP, _iniService.ProgramSignalPort);

            ProgramDto sendData = new()
            {
                Path = runProgramPath,
                IsStart = isStart
            };

            await _udpService.SendAsync(sendData);
        }

        
    }
}