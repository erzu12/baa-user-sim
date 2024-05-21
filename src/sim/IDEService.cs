namespace sim;

using DevEnv.WorkDir.Client;
using DevEnv.Base.Settings;


public class IDEService : IIDEService {
    private string _workDirId = "{b051c1f8-89b7-4a2f-89da-5c49ae027ac4}";
    private IWorkDirService _workDirService;

    public IDEService() {
        InMemorySettingsProvider<ISettings> settingsProvider = new InMemorySettingsProvider<ISettings>(() => {
            var settings = new Settings();
            settings.WorkDirServiceAddress = "http://localhost:5188";
            return settings;
        });
        _workDirService = new RemoteWorkDirService(settingsProvider);
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

}
