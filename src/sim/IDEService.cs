namespace sim;

using DevEnv.WorkDir.Client;
using DevEnv.Base.Settings;
using DevEnv.Build.Client;
using Settings = DevEnv.WorkDir.Client.Settings;
using ISettings = DevEnv.WorkDir.Client.ISettings;
using GrpcBuild;

public class IDEService : IIDEService {
    private string _workDirId;
    private IWorkDirService _workDirService;
    private IBuildService _buildService;

    public IDEService(string workDirId) {
        InMemorySettingsProvider<ISettings> settingsProvider = new InMemorySettingsProvider<ISettings>(() => {
            var settings = new Settings();
            settings.WorkDirServiceAddress = "http://localhost:5188";
            return settings;
        });
        _workDirService = new RemoteWorkDirService(settingsProvider);
        InMemorySettingsProvider<DevEnv.Build.Client.ISettings> buildSettingsProvider = new InMemorySettingsProvider<DevEnv.Build.Client.ISettings>(() => {
            var settings = new DevEnv.Build.Client.Settings();
            settings.BuildServiceAddress = "http://localhost:5288";
            return settings;
        });
        _buildService = new RemoteBuildService(buildSettingsProvider);
        _workDirId = workDirId;
    }

    public void setWorkingDirectory(string workDirId) {
        _workDirId = workDirId;
    }

    public string GetWorkingDirectory() {
        var dataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var workDirPath = Path.Combine(dataDir, "DevEnv.WorkDir.Service", _workDirId);
        return workDirPath;
    }

    public IEnumerable<string> GetFiles() {
        var files = _workDirService.GetFiles(_workDirId);
        files.Wait();
        foreach (var file in files.Result) {
            yield return file.FileName;
        }
    }

    public byte[] LoadFile(string filePath) {
        var fileContent = _workDirService.LoadFile(_workDirId, filePath);
        fileContent.Wait();
        return fileContent.Result;
    }

    public void UpdateFile(string filePath, byte[] updatedFileContent) {
        var task = _workDirService.UpdateFile(_workDirId, filePath, updatedFileContent);
        task.Wait();
    }

    public void CommitChanges(string commitMessage) {
        var task = _workDirService.CommitChanges(_workDirId, commitMessage);
        task.Wait();
    }

    public BuildResult Build(BuildSystem buildSystem, string path) {
        var task = _buildService.Build(_workDirId, buildSystem, path);
        task.Wait();
        return task.Result;
    }

}
