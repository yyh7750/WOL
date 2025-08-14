
namespace WOL.Services.Interface
{
    public interface IIniService
    {
        int ShutdownSendPort { get; }
        int HeartbeatRecvPort { get; }
        void LoadConfig();
        void CreateDefaultConfig();
    }
}
