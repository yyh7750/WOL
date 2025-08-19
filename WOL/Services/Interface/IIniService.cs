
namespace WOL.Services.Interface
{
    public interface IIniService
    {
        int ShutdownSendPort { get; }
        int HeartbeatRecvPort { get; }
        int FileSelectPort { get; }
        string ServerIp { get; }
        void LoadConfig();
        void CreateDefaultConfig();
    }
}
