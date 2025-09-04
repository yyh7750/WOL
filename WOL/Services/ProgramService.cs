using System.Net.NetworkInformation;
using System.Net;
using System.Threading.Tasks;
using WOL.Services.Interface;
using System.Linq;
using System.Net.Sockets;

namespace WOL.Services
{
    public class ProgramService : IProgramService
    {
        public Task StartAllProgramsAsync()
        {
            // view model에서 프로그램 리스트 받아온 다음 프로그램 별로 StartProgramAsync 호출
            throw new System.NotImplementedException();
        }

        public Task StartProgramAsync()
        {
            // UDP 통신
            throw new System.NotImplementedException();
        }

        public Task StopAllProgramsAsync()
        {
            // view model에서 프로그램 리스트 받아온 다음 프로그램 별로 StopProgramAsync 호출
            throw new System.NotImplementedException();
        }

        public Task StopProgramAsync()
        {
            // UDP 통신
            throw new System.NotImplementedException();
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
    }
}
