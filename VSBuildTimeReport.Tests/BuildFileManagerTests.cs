using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using VSBuildTimeReport.Domain;

namespace VSBuildTimeReport.Tests
{
    [TestClass]
    public class BuildFileManagerTests
    {
        [TestMethod]
        public void GetAll_ReadsAllFiles()
        {
            var buildFileManager = new BuildFileManager("../../BuildSessionFiles");

            var buildSessions = buildFileManager.GetAll();

            buildSessions.Count().ShouldBeGreaterThan(1);
        }
    }
}
