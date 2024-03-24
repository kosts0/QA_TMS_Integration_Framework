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
                    Arguments = $"{TestResultDirectory} {TestRunId}",
                    FileName = RunnerConfig.TestImporterDirectory + '/' + "UpdateResults.bat"
                };
            }
            set => throw new NotImplementedException();
        }
    }
}
