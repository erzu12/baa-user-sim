using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevEnv.Base.RuntimeChecks;
using DevEnv.Build.Client;
using DevEnv.Execute.Client;
using DevEnv.WorkDir.Client;

namespace DevEnv.Editor.Controls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IWorkDirService workDirService;
        private readonly IBuildService buildService;
        private readonly IExecuteService executeService;

        public MainWindow(IWorkDirService workDirService, IBuildService buildService, IExecuteService executeService)
        {
            Argument.AssertNotNull(workDirService, nameof(workDirService));
            Argument.AssertNotNull(buildService, nameof(buildService));
            Argument.AssertNotNull(executeService, nameof(executeService));

            this.workDirService = workDirService;
            this.buildService = buildService;
            this.executeService = executeService;

            this.InitializeComponent();
            this.tabControl.Items.Clear();

            Console.WriteLine("Started " + this.Title + " window.");
        }

        private void CreateWorkDirButton_Click(object sender, RoutedEventArgs e)
        {
            // Initialize the work dir.
            var description = this.workDirDescriptionTextBox.Text;
            var repoUrl = this.repoUrlTextBox.Text;

            var workDirId = Task.Run(() => this.workDirService.StartNewWorkDir(description, repoUrl, "test")).Result;

            // Add tab.
            var workDirControl = new WorkDirControl(this.workDirService, this.buildService, this.executeService);

            var tabPage = new TabItem
            {
                Header = description,
                Content = workDirControl,
            };

            var index = this.tabControl.Items.Add(tabPage);

            // Display work dir.
            workDirControl.ShowWorkDir(workDirId);
        }
    }
}
