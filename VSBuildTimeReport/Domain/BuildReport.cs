using System;
using System.Collections.Generic;
using System.Linq;

namespace VSBuildTimeReport.Domain
{
    public class BuildReport
    {
        private readonly BuildSession _buildSession;

        public BuildReport(BuildSession buildSession)
        {
            _buildSession = buildSession;
        }

        public IEnumerable<ReportLine> GetDaily()
        {
            return Enumerable.GroupBy<BuildRun, string>(_buildSession.BuildRuns, r => $"{r.BuildStarted:yyyyMMdd}").Select(group => new ReportLine
            {
                Date = group.First().BuildStarted.Date,
                TotalBuildTime = TimeSpan.FromSeconds(group.Sum(r => r.BuiltTimeInSeconds))
            });
        }
    }
}