using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WOL.Models;
using WOL.Models.Dto;
using WOL.Services.Interface;

namespace WOL.Services
{
    public class ProgramStatusService : IProgramStatusService
    {
        private readonly IDataService _dataService;
        private readonly IProgramService _programService;
        private readonly IIniService _iniService;
        private CancellationTokenSource? _cts;

        public event Action<int, ProgramStatus>? ProgramStatusChanged;

        public ProgramStatusService(IDataService dataService, IProgramService programService, IIniService iniService)
        {
            _dataService = dataService;
            _programService = programService;
            _iniService = iniService;
        }

        public void Start()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                return;
            }
            _cts = new CancellationTokenSource();
            Task.Run(() => MonitorPrograms(_cts.Token), _cts.Token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async Task MonitorPrograms(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var devices = await _dataService.DeviceRepository.GetAllDevicesAsync();
                    var programs = await _dataService.ProgramRepository.GetAllProgramsAsync();
                    var deviceDict = devices.ToDictionary(d => d.Id);

                    var checkTasks = new List<Task>();
                    foreach (var program in programs)
                    {
                        checkTasks.Add(Task.Run(async () =>
                        {
                            if (!deviceDict.TryGetValue(program.DeviceId, out var device))
                            {
                                return;
                            }

                            bool isRunning = false;
                            if (_programService.IsMyIpAddress(device.IP))
                            {
                                isRunning = await IsLocalProgramRunningAsync(program);
                            }
                            else
                            {
                                isRunning = await IsRemoteProgramRunningAsync(device, program);
                            }

                            var newStatus = isRunning ? ProgramStatus.Running : ProgramStatus.Stopped;
                            ProgramStatusChanged?.Invoke(program.Id, newStatus);
                        }, token));
                    }

                    await Task.WhenAll(checkTasks);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in MonitorPrograms: {ex.Message}");
                }

                await Task.Delay(1000, token); // 1�ʸ��� �˻�
            }
        }

        public Task<bool> IsLocalProgramRunningAsync(Program program)
        {
            try
            {
                var programName = Path.GetFileNameWithoutExtension(program.Path);
                var processes = Process.GetProcessesByName(programName);
                foreach (var process in processes)
                {
                    try
                    {
                        string? filePath = process.MainModule?.FileName;
                        if (!string.IsNullOrEmpty(filePath) && filePath.Equals(program.Path, StringComparison.OrdinalIgnoreCase))
                        {
                            process.Dispose();
                            return Task.FromResult(true);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Could not get process module info: {e.Message}");
                        continue;
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error checking process: {e.Message}");
            }
            return Task.FromResult(false);
        }

        public async Task<bool> IsRemoteProgramRunningAsync(Device device, Program program)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(device.IP, _iniService.CommandListenPort);
                if (await Task.WhenAny(connectTask, Task.Delay(1000)) != connectTask)
                {
                    return false; // time out
                }

                await using var stream = client.GetStream();
                using var writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true);
                using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

                // Send command
                await writer.WriteLineAsync("GET_RUNNING_PROCESSES");
                await writer.FlushAsync();

                // Read response
                string? jsonResponse = await reader.ReadLineAsync();

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    return false;
                }

                var runningProcesses = JsonSerializer.Deserialize<List<ProcessDto>>(jsonResponse);

                if (runningProcesses == null)
                {
                    return false;
                }

                return runningProcesses.Any(p => p.FilePath.Equals(program.Path, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking remote process for {device.IP}: {ex.Message}");
                return false;
            }
        }
    }
}
