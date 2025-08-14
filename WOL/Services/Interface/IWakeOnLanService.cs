
using System;

namespace WOL.Services.Interface
{
    public interface IWakeOnLanService
    {
        void WakeUpAsync(string macAddress);
        void ShutdownAsync(string ip);
        void StartHeartbeatListener();
        void StopHeartbeatListener();

        event Action<string>? HeartbeatReceived; // ����̽� ���� ping ���� �� �̺�Ʈ ó��
    }
}
