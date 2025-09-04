using System.Collections.Generic;

namespace WOL.Models.Dto
{
    public class ProgramDto
    {
        public List<string> Paths { get; set; }
        public int IsStart{ get; set; }

        public ProgramDto() : this([], 0) { }

        public ProgramDto(List<string> paths, int isStart)
        {
            Paths = paths;
            IsStart = isStart;
        }
    }
}
