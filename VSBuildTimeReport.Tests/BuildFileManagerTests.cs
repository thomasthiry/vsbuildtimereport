using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using VSBuildTimeReport.Domain;

namespace VSBuildTimeReport.Tests
{
    [TestClass]
    public class BuildFileManagerTests
    {
        private const string BuildSessionFolder = "../../BuildSessionFiles";
        private static readonly DateTime now = new DateTime(2018, 06, 24, 21, 18, 43);
        private readonly DateTimeProviderMock _dateTimeProviderMock = new DateTimeProviderMock(now);

        [TestMethod]
        public void GetAll_ReadsAllFiles()
        {
            var buildFileManager = new BuildFileManager("../../BuildSessionFiles", _dateTimeProviderMock);

            var buildSessions = buildFileManager.GetAll();

            buildSessions.Count().ShouldBeGreaterThan(1);
        }

        [TestMethod]
        public void GetTodaysBuildSession_FileExists_ReadOnlyThisFile()
        {
            var buildFileManager = new BuildFileManager("../../BuildSessionFiles", _dateTimeProviderMock);

            var buildSessions = buildFileManager.GetTodaysBuildSession();

            buildSessions.StartTime.Date.ShouldBe(now.Date);
        }

        [TestMethod]
        public void GetTodaysBuildSession_FileDoesNotExist_CreatesNewSession()
        {
            var veryOldDate = new DateTime(1980, 1, 1, 1, 1, 1);
            var veryOldDateTimeProviderMock = new DateTimeProviderMock(veryOldDate);
            var buildFileManager = new BuildFileManager(BuildSessionFolder, veryOldDateTimeProviderMock);

            var buildSessions = buildFileManager.GetTodaysBuildSession();

            buildSessions.StartTime.Date.ShouldBe(veryOldDate.Date);
        }
    }
}
