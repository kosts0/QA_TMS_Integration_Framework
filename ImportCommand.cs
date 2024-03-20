using System.Diagnostics;
using TestItAdapter;

namespace TestRunnerWebApi
{
    public class ImportCommand : ProcessInfoBase
    {
        public string TestResultDirectory { get; set; }
        public Guid TestRunId { get; set; }
        public ImportCommand(RunnerConfig runnerConfig) : base(runnerConfig)
        {
        }
        public override ProcessStartInfo ProcessInfo
        {
            get
            {
                return new ProcessStartInfo()
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = RunnerConfig.TestImporterDirectory,
                    Arguments = $"{TestResultDirectory} {TestRunId}",
                    FileName = "UpdateResults.bat"
                };
            }
            set => throw new NotImplementedException();
        }
    }
}
