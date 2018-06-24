using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using VSBuildTimeReport.Domain;

namespace VSBuildTimeReport.Tests
{
    [TestClass]
    public class BuildReportTests
    {
        [TestMethod]
        public void GetDailyReport_SeveralBuildsOnSameDay_ReturnOneLineWithSummedTime()
        {
            var buildSession = new BuildSession
            {
                BuildRuns = new List<BuildRun>
                {
                    new BuildRun
                    {
                        BuildStarted = new DateTime(2018, 6, 23, 16, 0, 0),
                        BuildEnded = new DateTime(2018, 6, 23, 16, 0, 5)
                    },
                    new BuildRun
                    {
                        BuildStarted = new DateTime(2018, 6, 23, 17, 0, 0),
                        BuildEnded = new DateTime(2018, 6, 23, 17, 0, 5)
                    },
                    new BuildRun
                    {
                        BuildStarted = new DateTime(2018, 6, 23, 18, 0, 0),
                        BuildEnded = new DateTime(2018, 6, 23, 18, 0, 5)
                    },
                }
            };
            var report = new BuildReport(buildSession);

            var reportLines = report.GetDaily();

            reportLines.ShouldHaveSingleItem();
            reportLines.First().TotalBuildTime.Seconds.ShouldBe(15);
        }
    }
}
