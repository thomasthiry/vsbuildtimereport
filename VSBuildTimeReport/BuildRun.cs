using System;

namespace VSBuildTimeReport
{
    public class BuildRun
    {
        public string SolutionName { get; set; }
        public DateTime BuildStarted { get; set; }
        public DateTime BuildEnded { get; set; }
    }
}