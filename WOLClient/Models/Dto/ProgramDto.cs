
namespace WOLClient.Models.Dto
{
    public class ProgramDto
    {
        public List<string> Paths { get; set; }
        public int IsStart { get; set; }

        public ProgramDto()
        {
            Paths = [];
            IsStart = 0;
        }
    }
}
