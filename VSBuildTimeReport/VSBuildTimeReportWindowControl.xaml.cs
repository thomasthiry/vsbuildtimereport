using System;
using System.Collections.Generic;
using VSBuildTimeReport.Domain;

namespace VSBuildTimeReport
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {            
            var buildTimeReportFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VSBuildTimeReport");

            var buildFieldManager = new BuildFileManager(buildTimeReportFolderPath);
            var buildSessions = buildFieldManager.GetAll();

            var report = new BuildReport(buildSessions);

            var reportLines = report.GetProjectsReport();

            reportGrid.ItemsSource = reportLines;
        }
    }
}