using System;
using System.Collections.Generic;
using System.Linq;

namespace VSBuildTimeReport.Domain
{
    public class BuildReport
    {
        private readonly IEnumerable<BuildSession> _buildSessions;

        public BuildReport(IEnumerable<BuildSession> buildSessions)
        {
            _buildSessions = buildSessions;
        }

        public IEnumerable<ReportLine> GetProjectsReport()
        {
            return _buildSessions.SelectMany(s => s.BuildRuns).GroupBy<BuildRun, string>(r => r.SolutionName).Select(group => 
                new ReportLine
                {
                    SolutionName = group.Key,
                    TotalBuildTime = TimeSpan.FromSeconds(group.Sum(r => r.BuiltTimeInSeconds))
                });
        }
    }
}