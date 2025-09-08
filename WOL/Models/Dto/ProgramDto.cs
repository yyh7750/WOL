using System.Collections.Generic;

namespace WOL.Models.Dto
{
    public class ProgramDto
    {
        public List<string> Paths { get; set; }
        public bool IsStart{ get; set; }

        public ProgramDto() : this([], false) { }

        public ProgramDto(List<string> paths, bool isStart)
        {
            Paths = paths;
            IsStart = isStart;
        }
    }
}
