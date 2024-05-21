using DevEnv.Base.Utilities;

namespace DevEnv.Execute.Service
{
    public class Settings : ISettings, Build.Client.ISettings
    {
        public string DataDirectoryPath { get; set; } = DataDirectoryUtils.GetDataDirectory();
        public string BuildServiceAddress => "https://localhost:7288";
    }
}
