namespace WOL.Services.Interface
{
    public interface IIniService
    {
        int HeartbeatRecvPort { get; }
        int ShutdownSendPort { get; }
        int ProgramSignalPort { get; }
        int FileSelectPort { get; }
        void LoadConfig();
        void CreateDefaultConfig();
    }
}
