using System;

namespace VSBuildTimeReport.Domain
{
    public class ReportLine
    {
        public TimeSpan TotalBuildTime { get; set; }
        public string SolutionName { get; set; }
    }
}