using System;

namespace VSBuildTimeReport.Domain
{
    public class ReportLine
    {
        public TimeSpan TotalBuildTime { get; set; }
        public string SolutionName { get; set; }
        public int TotalNumberOfBuilds { get; set; }
        public TimeSpan AverageBuildTime { get; set; }
        public DateTime FirstBuildTime { get; set; }
        public DateTime LastBuildTime { get; set; }
    }
}