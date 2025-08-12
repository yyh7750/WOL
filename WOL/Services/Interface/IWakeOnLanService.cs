
namespace WOL.Services.Interface
{
    public interface IWakeOnLanService
    {
        void WakeUpAsync(string macAddress);
    }
}
