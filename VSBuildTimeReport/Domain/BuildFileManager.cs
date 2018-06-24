using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VSBuildTimeReport.Domain
{
    public class BuildFileManager
    {
        private readonly string _buildSessionFolder;

        public BuildFileManager(string buildSessionFolder)
        {
            _buildSessionFolder = buildSessionFolder;
        }

        private static string BuildsFileName => $@"C:\Users\tth\AppData\Roaming\VSBuildTimeReport\BuildSession_{DateTime.Today:yyyy-MM-dd}.json";

        private BuildSession ReadFile(string file)
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
            return buildSession;
        }

        public IEnumerable<BuildSession> GetAll()
        {
            var files = Directory.GetFiles(_buildSessionFolder);

            var buildSessions = new List<BuildSession>();
            foreach (var file in files)
            {
                var buildSession = ReadFile(file);
                buildSessions.Add(buildSession);
            }

            return buildSessions;
        }
    }
}