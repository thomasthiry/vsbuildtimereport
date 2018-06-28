using System;
using System.Collections.Generic;

namespace VSBuildTimeReport.Domain
{
    public class BuildSession
    {
        public BuildSession()
        {
            BuildRuns = new List<BuildRun>();
        }
        public string MachineName { get; set; }
        public string UserName { get; set; }
        public DateTime StartTime { get; set; }
        public IList<BuildRun> BuildRuns { get; set; }
    }
}