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
using System.ComponentModel;
using System.Threading;

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
                TerminateSameNameProcesses(runProgramPath);

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

        public void TerminateSameNameProcesses(string exePath, int waitMs = 500)
        {
            if (string.IsNullOrWhiteSpace(exePath)) return;

            string procName = Path.GetFileNameWithoutExtension(exePath);
            if (string.IsNullOrWhiteSpace(procName)) return;

            int currentPid = Process.GetCurrentProcess().Id;
            Process[] procs = Process.GetProcessesByName(procName);

            foreach (Process p in procs)
            {
                try
                {
                    if (p.Id == currentPid) continue;

                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        p.CloseMainWindow();
                        if (p.WaitForExit(waitMs)) continue;
                    }

                    p.Kill(entireProcessTree: true);
                    p.WaitForExit(waitMs);
                }
                catch (InvalidOperationException) { /* 이미 종료됨 */ }
                catch (Win32Exception) { /* 권한 부족 등 - 로깅만 */ }
                catch (Exception) { /* 필요시 로깅 */ }
            }

            // 레이스 컨디션 방지: 잠깐 대기 후 동일 이름 프로세스 잔존 여부 재확인
            Thread.Sleep(200);
        }
    }
}
