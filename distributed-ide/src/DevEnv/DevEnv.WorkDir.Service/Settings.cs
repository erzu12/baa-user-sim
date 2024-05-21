using DevEnv.Base.Utilities;

namespace DevEnv.WorkDir.Service
{
    public class Settings : ISettings
    {
        public string DataDirectoryPath { get; set; } = DataDirectoryUtils.GetDataDirectory();
    }
}
