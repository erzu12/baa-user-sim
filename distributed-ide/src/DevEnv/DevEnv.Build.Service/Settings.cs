using DevEnv.Base.Utilities;

namespace DevEnv.Build.Service
{
    public class Settings : ISettings, WorkDir.Client.ISettings
    {
        public string DataDirectoryPath { get; set; } = DataDirectoryUtils.GetDataDirectory();

        public string WorkDirServiceAddress { get; set; } = "https://localhost:7188";
    }
}
