using System.Windows.Controls;
using DevEnv.Base.RuntimeChecks;
using DevEnv.WorkDir.Client;
using System.Linq;
using System.Threading.Tasks;
using DevEnv.Build.Client;
using System;
using DevEnv.Execute.Client;
using System.Threading;
using System.Text;

namespace DevEnv.Editor.Controls
{
    /// <summary>
    /// Interaction logic for WorkDirControl.xaml
    /// </summary>
    public partial class WorkDirControl : UserControl
    {
        private readonly IWorkDirService workDirService;
        private readonly IBuildService buildService;
        private readonly IExecuteService executeService;

        private string? workDirId;
        private string? buildId;
        private string? selectedCodeFile;
        private byte[]? selectedCodeFileBytes;

        public WorkDirControl(IWorkDirService workDirService, IBuildService buildService, IExecuteService executeService)
        {
            Argument.AssertNotNull(workDirService, nameof(workDirService));
            Argument.AssertNotNull(buildService, nameof(buildService));
            Argument.AssertNotNull(executeService, nameof(executeService));

            this.InitializeComponent();

            this.workDirService = workDirService;
            this.buildService = buildService;
            this.executeService = executeService;

            // Fill combo boxes.
            var buildSystemValues = typeof(GrpcBuild.BuildSystem).GetEnumValues();
            this.buildSystemComboBox.ItemsSource = buildSystemValues;

            var executeEnvValues = typeof(GrpcExecute.Environment).GetEnumValues();
            this.executeTypeComboBox.ItemsSource = executeEnvValues;
        }

        public void ShowWorkDir(string workDirId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            this.workDirId = workDirId;

            var files = Task.Run(() => this.workDirService.GetFiles(workDirId)).Result;
            var fileNames = files
                .Select(f => f.FileName)
                .OrderBy(f => f)
                .ToList();

            this.filesListBox.Items.Clear();

            foreach (var file in fileNames)
            {
                var item = new ListBoxItem
                {
                    Content = file,
                };

                this.filesListBox.Items.Add(item);
            }
        }

        private void BuildButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var buildSystem = (GrpcBuild.BuildSystem?)this.buildSystemComboBox.SelectedItem;
            var listBoxItem = (ListBoxItem?)this.filesListBox.SelectedItem;
            var rootFile = (string?)listBoxItem?.Content;

            if (this.workDirId == null ||
                buildSystem == null ||
                rootFile == null)
            {
                return;
            }

            // Build.
            var buildResult = Task.Run(() => this.buildService.Build(this.workDirId, buildSystem.Value, rootFile)).Result;
            
            if (buildResult.IsSuccessful)
            {
                this.buildId = buildResult.BuildId;
                var artifactFiles = Task.Run(() => this.buildService.GetArtifactFiles(this.workDirId, this.buildId)).Result;
                var artifactFileNames = artifactFiles
                    .Select(f => f.FileName)
                    .OrderBy(f => f)
                    .ToList();

                this.executeRootFileComboBox.ItemsSource = artifactFileNames;
                
                Console.WriteLine($"Build successful. Build ID: {this.buildId}");
            }
            else
            {
                this.buildId = null;
                this.executeRootFileComboBox.ItemsSource = null;
                
                Console.WriteLine($"Build failed.");
            }
        }

        private void ExecuteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var executeEnvironment = (GrpcExecute.Environment?)this.executeTypeComboBox.SelectedItem;
            var executeRootFile = (string?)this.executeRootFileComboBox.SelectedItem;

            if (this.workDirId == null ||
                this.buildId == null ||
                executeEnvironment == null ||
                executeRootFile == null)
            {
                return;
            }

            // Start executing.
            var executeId = Task.Run(() => this.executeService.StartExecute(this.workDirId, this.buildId, executeEnvironment.Value, executeRootFile)).Result;

            // Output.
            var linesOffset = 0;
            var status = Task.Run(() => this.executeService.GetStatus(executeId)).Result;
            var lastSignOfLife = DateTime.Now;

            while (!status.HasExited &&
                   (DateTime.Now - lastSignOfLife).TotalSeconds < 3)
            {
                var output = Task.Run(() => this.executeService.GetOutputLines(executeId, linesOffset)).Result;
                if (output.Any())
                {
                    lastSignOfLife = DateTime.Now;
                }

                foreach (var line in output)
                {
                    Console.WriteLine(line);
                }

                linesOffset += output.Count;
                Thread.Sleep(10);
                status = Task.Run(() => this.executeService.GetStatus(executeId)).Result;
            }

            Task.Run(() => this.executeService.Kill(executeId)).Wait();
        }

        private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.workDirId == null)
            {
                return;
            }

            var selectedItem = (ListBoxItem)this.filesListBox.SelectedItem;
            if (selectedItem == null)
            {
                this.selectedCodeFile = null;
                this.codeTextBox.Text = string.Empty;
                
                return;
            }

            this.selectedCodeFile = (string)selectedItem.Content;
            
            // Download file.
            this.selectedCodeFileBytes = Task.Run(() => this.workDirService.LoadFile(this.workDirId, this.selectedCodeFile)).Result;
            var fileText = Encoding.ASCII.GetString(this.selectedCodeFileBytes);

            this.codeTextBox.Text = fileText;
        }

        private void SaveButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.workDirId == null ||
                this.selectedCodeFile == null)
            {
                return;
            }

            var fileText = this.codeTextBox.Text;
            var fileBytes = Encoding.ASCII.GetBytes(fileText);

            // Hack: replace first couple of bytes.
            var nBytes = 24;
            var fixedFileBytes = this.selectedCodeFileBytes.Take(nBytes).Concat(fileBytes.Skip(nBytes)).ToArray();

            Task.Run(() => this.workDirService.UpdateFile(this.workDirId, this.selectedCodeFile, fixedFileBytes)).Wait();


            Console.WriteLine($"File saved.");
        }
    }
}
