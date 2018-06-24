﻿using System;
using System.Collections.Generic;
using VSBuildTimeReport.Domain;

namespace VSBuildTimeReport
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for VSBuildTimeReportWindowControl.
    /// </summary>
    public partial class VSBuildTimeReportWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VSBuildTimeReportWindowControl"/> class.
        /// </summary>
        public VSBuildTimeReportWindowControl()
        {
            this.InitializeComponent();
        }

        private string _buildsFileName = $@"C:\Users\tth\AppData\Roaming\VSBuildTimeReport\"; // BuildSession_{DateTime.Today:yyyy-MM-dd}.json

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //var buildSession = new BuildSession
            //{
            //    BuildRuns = new List<BuildRun>
            //    {
            //        new BuildRun
            //        {
            //            BuildStarted = new DateTime(2018, 6, 23, 16, 0, 0),
            //            BuildEnded = new DateTime(2018, 6, 23, 16, 0, 5)
            //        },
            //        new BuildRun
            //        {
            //            BuildStarted = new DateTime(2018, 6, 23, 17, 0, 0),
            //            BuildEnded = new DateTime(2018, 6, 23, 17, 0, 5)
            //        },
            //        new BuildRun
            //        {
            //            BuildStarted = new DateTime(2018, 6, 23, 18, 0, 0),
            //            BuildEnded = new DateTime(2018, 6, 23, 18, 0, 5)
            //        },
            //    }
            //};
            //var report = new BuildReport(buildSession);
            
            var buildFieldManager = new BuildFileManager(_buildsFileName);
            var buildSessions = buildFieldManager.GetAll();

            var report = new BuildReport(buildSessions);

            var reportLines = report.GetDaily();

            reportGrid.ItemsSource = reportLines;
        }
    }
}