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
            var buildsFileName = $"BuildSession_{_dateTimeProvider.GetNow():yyyy-MM-dd}.json";
            var buildsFilePath = Path.Combine(_buildSessionFolder, buildsFileName);

            if (File.Exists(buildsFilePath))
            {
                return ReadFile(buildsFilePath);
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
    }
}