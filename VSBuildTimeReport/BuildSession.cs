using System;
using System.Collections;
using System.Collections.Generic;
using VSBuildTimeReport.Domain;

namespace VSBuildTimeReport
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