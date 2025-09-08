
namespace WOLClient.Models.Dto
{
    public class ProgramDto
    {
        public string Path { get; set; }
        public bool IsStart { get; set; }

        public ProgramDto()
        {
            Path = string.Empty;
            IsStart = false;
        }
    }
}
