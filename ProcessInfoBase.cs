using System.Diagnostics;
using TestItAdapter;

namespace TestRunnerWebApi
{
    /// <summary>
    /// Базовый класс, для выполнения команды
    /// </summary>
    public abstract class ProcessInfoBase
    {
        public abstract ProcessStartInfo ProcessInfo { get; set; }
        protected RunnerConfig RunnerConfig { get; set; }
        public ProcessInfoBase(RunnerConfig runnerConfig)
        {
            RunnerConfig = runnerConfig;
        }
    }
}
