using DevEnv.Base.RuntimeChecks;
using System.Diagnostics;

namespace DevEnv.Base.Processes
{
    public class ProcessManager : IProcessManager, IDisposable
    {
        private readonly IDictionary<string, ProcessAndOutput> processes = new Dictionary<string, ProcessAndOutput>();

        public string Start(string workingDirectory, string fileName, string arguments, bool useShellExecute = false)
        {
            Argument.AssertNotNull(workingDirectory, nameof(workingDirectory));
            Argument.AssertNotEmpty(fileName, nameof(fileName));
            Argument.AssertNotNull(arguments, nameof(arguments));

            // Initialize.
            var startInfo = new ProcessStartInfo
                {
                    WorkingDirectory = workingDirectory,
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = useShellExecute,
                    RedirectStandardOutput = !useShellExecute,
                    RedirectStandardError = !useShellExecute,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
            };

            var p = new ProcessAndOutput(new Process
                {
                    StartInfo = startInfo,
                });

            p.Process.OutputDataReceived += (object sender, DataReceivedEventArgs args) =>
                {
                    p.OutputLines.Add(args.Data ?? string.Empty);
                };

            p.Process.ErrorDataReceived += (object sender, DataReceivedEventArgs args) =>
                {
                    p.OutputLines.Add(args.Data ?? string.Empty);
                };

            // Store.
            var newId = Guid.NewGuid().ToString("B");
            this.processes.Add(newId, p);

            // Start.
            p.Process.Start();
            
            if (!useShellExecute)
            {
                p.Process.BeginOutputReadLine();
                p.Process.BeginErrorReadLine();
            }

            return newId;
        }

        public ProcessStatus GetStatus(string id)
        {
            Argument.AssertNotEmpty(id, nameof(id));

            var p = this.processes[id];

            return new ProcessStatus(
                p.Process.HasExited,
                p.Process.HasExited ? p.Process.ExitCode : null);
        }

        public IReadOnlyList<string> GetOutputLines(string id, int offset)
        {
            Argument.AssertNotEmpty(id, nameof(id));

            var p = this.processes[id];

            if (offset >= p.OutputLines.Count)
            {
                return new List<string>();
            }

            var lines = p.OutputLines.Skip(offset).ToList();

            return lines;
        }

        public string GetCompleteOutput(string id)
        {
            Argument.AssertNotEmpty(id, nameof(id));

            var lines = this.GetOutputLines(id, 0);
            var output = string.Join(Environment.NewLine, lines);

            return output;
        }

        public async Task<int> Wait(string id)
        {
            Argument.AssertNotEmpty(id, nameof(id));

            var p = this.processes[id];
            await p.Process.WaitForExitAsync();

            return p.Process.ExitCode;
        }

        public void Kill(string id)
        {
            Argument.AssertNotEmpty(id, nameof(id));

            var p = this.processes[id];
            p.Process.Kill();
        }

        public void Dispose()
        {
            foreach (var p in this.processes)
            {
                if (!p.Value.Process.HasExited)
                { 
                    p.Value.Process.Kill();
                }
            }
        }

        /// <summary>
        /// A private container class which comprises a process and its associated output.
        /// </summary>
        private class ProcessAndOutput
        {
            public ProcessAndOutput(Process process)
            {
                Argument.AssertNotNull(process, nameof(process));

                this.Process = process;
            }

            /// <summary>
            /// Gets the actual process.
            /// </summary>
            public Process Process { get; }

            /// <summary>
            /// Gets the process' output as a list of lines.
            /// </summary>
            public IList<string> OutputLines { get; } = new List<string>();

            /// <summary>
            /// Provides a practical string representation (mainly for debugging purposes).
            /// </summary>
            public override string ToString()
            {
                return "file: " + this.Process.StartInfo.FileName;
            }
        }
    }
}
