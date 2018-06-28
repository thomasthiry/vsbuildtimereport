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
            var buildSessions = new List<BuildSession> {
                new BuildSession
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
                }
            };
            var report = new BuildReport(buildSessions);

            var reportLines = report.GetDaily();

            reportLines.ShouldHaveSingleItem();
            reportLines.First().TotalBuildTime.Seconds.ShouldBe(15);
        }

        [TestMethod]
        public void GetDailyReport_SeveralBuildSessions_MergeRunsInReport()
        {
            var buildSessions = new List<BuildSession> {
                new BuildSession
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
                    }
                },
                new BuildSession
                {
                    BuildRuns = new List<BuildRun>
                    {
                        new BuildRun
                        {
                            BuildStarted = new DateTime(2018, 6, 23, 18, 0, 0),
                            BuildEnded = new DateTime(2018, 6, 23, 18, 0, 5)
                        },
                    }
                }
            };
            var report = new BuildReport(buildSessions);

            var reportLines = report.GetDaily();

            reportLines.ShouldHaveSingleItem();
            reportLines.First().TotalBuildTime.Seconds.ShouldBe(15);
        }

        [TestMethod]
        public void GetProjectsReport_SeveralBuildsForSeveralProjects_ReturnOneLinePerProjectWithSummedTime()
        {
            var project1 = "Project1.sln";
            var project2 = "Project2.sln";

            var buildSessions = new List<BuildSession> {
                new BuildSession
                {
                    BuildRuns = new List<BuildRun>
                    {
                        new BuildRun
                        {
                            SolutionName = project1,
                            BuildStarted = new DateTime(2018, 6, 23, 16, 0, 0),
                            BuildEnded = new DateTime(2018, 6, 23, 16, 0, 5)
                        },
                        new BuildRun
                        {
                            SolutionName = project1,
                            BuildStarted = new DateTime(2018, 6, 23, 17, 0, 0),
                            BuildEnded = new DateTime(2018, 6, 23, 17, 0, 5)
                        },
                        new BuildRun
                        {
                            SolutionName = project2,
                            BuildStarted = new DateTime(2018, 6, 23, 18, 0, 0),
                            BuildEnded = new DateTime(2018, 6, 23, 18, 0, 5)
                        },
                        new BuildRun
                        {
                            SolutionName = project2,
                            BuildStarted = new DateTime(2018, 6, 23, 19, 0, 0),
                            BuildEnded = new DateTime(2018, 6, 23, 19, 0, 5)
                        },
                        new BuildRun
                        {
                            SolutionName = project2,
                            BuildStarted = new DateTime(2018, 6, 23, 20, 0, 0),
                            BuildEnded = new DateTime(2018, 6, 23, 20, 0, 5)
                        },
                    }
                }
            };
            var report = new BuildReport(buildSessions);

            var reportLines = report.GetProjectsReport();

            reportLines.Single(l => l.SolutionName == project1).TotalBuildTime.Seconds.ShouldBe(10);
            reportLines.Single(l => l.SolutionName == project2).TotalBuildTime.Seconds.ShouldBe(15);
        }
    }
}
