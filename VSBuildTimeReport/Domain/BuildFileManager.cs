using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace VSBuildTimeReport.Domain
{
    public class BuildFileManager
    {
        private readonly string _buildSessionFolder;
        private readonly IDateTimeProvider _dateTimeProvider;
        private string BuildsFileName => $"BuildSession_{_dateTimeProvider.GetNow():yyyy-MM-dd}.json";
        private string BuildsFilePath => Path.Combine(_buildSessionFolder, BuildsFileName);

        public BuildFileManager(string buildSessionFolder, IDateTimeProvider dateTimeProvider)
        {
            _buildSessionFolder = buildSessionFolder;
            _dateTimeProvider = dateTimeProvider;
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

        public BuildSession GetTodaysBuildSession()
        {
            if (File.Exists(BuildsFilePath))
            {
                return ReadFile(BuildsFilePath);
            }
            else
            {
                return  new BuildSession
                {
                    StartTime = _dateTimeProvider.GetNow(),
                    MachineName = Environment.MachineName,
                    UserName = Environment.UserName
                };
            }
        }

        private BuildSession ReadFile(string file)
        {
            var buildSession = File.Exists(file) ? JsonConvert.DeserializeObject<BuildSession>(File.ReadAllText(file)) : null;

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

        public void WriteBuildSessionFile(BuildSession buildSession)
        {
            if (Directory.Exists(_buildSessionFolder) == false)
            {
                Directory.CreateDirectory(_buildSessionFolder);
            }

            File.WriteAllText(BuildsFilePath, JsonConvert.SerializeObject(buildSession));
        }
    }
}