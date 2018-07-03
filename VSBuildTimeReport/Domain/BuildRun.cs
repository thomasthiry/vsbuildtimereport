using System;
using Newtonsoft.Json;

namespace VSBuildTimeReport.Domain
{
    public class BuildRun
    {
        public string SolutionName { get; set; }
        public DateTime BuildStarted { get; set; }
        public DateTime BuildEnded { get; set; }

        [JsonIgnore]
        public double BuildTimeInSeconds => (BuildEnded - BuildStarted).TotalSeconds;
    }
}