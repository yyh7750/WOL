
namespace WOLClient.Models.Dto
{
    public class ProgramDto
    {
        public List<string> Paths { get; set; }
        public bool IsStart { get; set; }

        public ProgramDto()
        {
            Paths = [];
            IsStart = false;
        }
    }
}
