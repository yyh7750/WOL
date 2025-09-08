using System.Threading.Tasks;
using WOL.Models.Dto;

namespace WOL.Services.Interface
{
    public interface IUdpService
    {
        Task SendAsync(ProgramDto dto);
    }
}
