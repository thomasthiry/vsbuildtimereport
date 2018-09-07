using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
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
        private IVsSolutionBuildManager2 _sbm;

        private SolutionEvents _events;

        private DTE _dte;

        private BuildRun _ongoingBuild;
        private readonly BuildFileManager _buildFileManager;

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

            var buildTimeReportFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VSBuildTimeReport");

            _buildFileManager = new BuildFileManager(buildTimeReportFolderPath, new DateTimeProvider());
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
            _sbm = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager2;
            if (_sbm != null)
            {
                _sbm.AdviseUpdateSolutionEvents(this, out _);
            }

            // Must hold a reference to the solution events object or the events wont fire, garbage collection related
            _events = GetDTE().Events.SolutionEvents;
            _events.Opened += Solution_Opened;

            LoadAndInitializeBuildSession();
        }

        private void LoadAndInitializeBuildSession()
        {

            BuildSession = _buildFileManager.GetTodaysBuildSession();
        }

        private void WriteToOutputWindow(string context)
        {
            try
            {
                var det2 = GetDTE() as EnvDTE80.DTE2;

                if (det2 == null) return;

                var pane = det2.ToolWindows.OutputWindow.ActivePane;

                pane.OutputString(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private DTE GetDTE()
        {
            if (_dte == null)
            {
                _dte = ServiceProvider.GlobalProvider.GetService(typeof(SDTE)) as DTE;
            }
            return _dte;
        }

        private void Solution_Opened()
        {
            SolutionName = GetSolutionName();
        }

        private string GetSolutionName()
        {
            var vsSolution = ServiceProvider.GlobalProvider.GetService(typeof(SVsSolution)) as IVsSolution2;
            vsSolution.GetProperty((int)__VSPROPID.VSPROPID_SolutionBaseName, out var solutionName);
            return (string)solutionName;
        }

        public int UpdateSolution_Begin(ref int pfCancelUpdate)
        {
            _ongoingBuild = new BuildRun { SolutionName = SolutionName, BuildStarted = DateTime.Now };
            return VSConstants.S_OK;
        }

        public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
        {
            _ongoingBuild.BuildEnded = DateTime.Now;
            BuildSession.BuildRuns.Add(_ongoingBuild);

            WriteToOutputWindow(
                string.Format("========== Build Time : {0} s ==========\r\n",
                _ongoingBuild.BuildTimeInSeconds));

            _ongoingBuild = null;

            _buildFileManager.WriteBuildSessionFile(BuildSession);

            return VSConstants.S_OK;
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
