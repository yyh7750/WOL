
using System;

namespace WOL.Services.Interface
{
    public interface IWakeOnLanService
    {
        void WakeUpAsync(string macAddress);
        void ShutdownAsync(string ip);
        void StartHeartbeatListener();
        void StopHeartbeatListener();

        event Action<string>? HeartbeatReceived; // 디바이스 별로 ping 수신 시 이벤트 처리
    }
}
