using System.Collections.Generic;

namespace WOL.Models.Dto
{
    public class ProgramDto
    {
        public string Path { get; set; }
        public bool IsStart{ get; set; }

        public ProgramDto() : this(string.Empty, false) { }

        public ProgramDto(string path, bool isStart)
        {
            Path = path;
            IsStart = isStart;
        }
    }
}
