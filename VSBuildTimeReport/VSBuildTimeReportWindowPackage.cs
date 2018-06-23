using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Newtonsoft.Json;
using VSBuildTimeReport.Domain;
using Task = System.Threading.Tasks.Task;

namespace VSBuildTimeReport
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(VSBuildTimeReportWindow))]
    [Guid(VSBuildTimeReportWindowPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideBindingPath]
    public sealed class VSBuildTimeReportWindowPackage : AsyncPackage, IVsUpdateSolutionEvents2
    {
        private static string BuildsFileName => $@"C:\Users\tth\AppData\Roaming\VSBuildTimeReport\BuildSession_{DateTime.Today:yyyy-MM-dd}.json";

        private IVsSolutionBuildManager2 sbm;
        private uint updateSolutionEventsCookie;
        private SolutionEvents events;
        private DTE dte;

        private BuildRun OngoingBuild;

        public string SolutionName { get; set; }

        private BuildSession BuildSession { get; set; }

        /// <summary>
        /// VSBuildTimeReportWindowPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "d02948b0-bbde-4ec5-ac09-29f94ae9914d";

        /// <summary>
        /// Initializes a new instance of the <see cref="VSBuildTimeReportWindow"/> class.
        /// </summary>
        public VSBuildTimeReportWindowPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await VSBuildTimeReportWindowCommand.InitializeAsync(this);

            // Get solution build manager
            sbm = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            if (sbm != null)
            {
                sbm.AdviseUpdateSolutionEvents(this, out updateSolutionEventsCookie);
            }

            // Must hold a reference to the solution events object or the events wont fire, garbage collection related
            events = GetDTE().Events.SolutionEvents;
            events.Opened += Solution_Opened;

            InitializeBuildSession();
        }

        private void InitializeBuildSession()
        {
            var buildSession = File.Exists(BuildsFileName) ? JsonConvert.DeserializeObject<BuildSession>(File.ReadAllText(BuildsFileName)) : null;

            if (buildSession == null)
            {
                buildSession = new BuildSession
                {
                    StartTime = DateTime.Now,
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName
                };
            }
            BuildSession = buildSession;
        }

        private static List<BuildRun> ReadBuildSessionFile()
        {
            var buildRuns = JsonConvert.DeserializeObject<List<BuildRun>>(File.ReadAllText(BuildsFileName));
            return buildRuns ?? new List<BuildRun>();
        }

        private DTE GetDTE()
        {
            if (dte == null)
            {
                dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE;
            }
            return dte;
        }

        private void Solution_Opened()
        {
            SolutionName = GetSolutionName();
        }

        private string GetSolutionName()
        {
            object solutionName;
            var vsSolution = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution)) as IVsSolution2;
            vsSolution.GetProperty((int)__VSPROPID.VSPROPID_SolutionBaseName, out solutionName);
            return (string)solutionName;
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            OngoingBuild = new BuildRun { SolutionName = SolutionName, BuildStarted = DateTime.Now };
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            OngoingBuild.BuildEnded = DateTime.Now;
            BuildSession.BuildRuns.Add(OngoingBuild);
            OngoingBuild = null;

            WriteBuildSessionFile();

            return VSConstants.S_OK;
        }

        private void WriteBuildSessionFile()
        {
            File.WriteAllText(BuildsFileName, JsonConvert.SerializeObject(BuildSession));
        }


        public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
        {
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Cancel()
        {
            return VSConstants.S_OK;
        }

        public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
        {
            return VSConstants.S_OK;
        }

        #endregion
    }
}
