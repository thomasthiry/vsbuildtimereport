using System;

namespace VSBuildTimeReport.Domain
{
    public class ReportLine
    {
        public DateTime Date { get; set; }
        public TimeSpan TotalBuildTime { get; set; }
        public string SolutionName { get; set; }
    }
}