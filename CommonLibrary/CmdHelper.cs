using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary;

public static class CmdHelper
{
    /// <summary>
    /// Выпонлнить консольную команду
    /// </summary>
    /// <param name="command"></param>
    public static void ExecuteCommand(string command, string workingDirectory, string fileName = "cmd.exe")
    {
        var processInfo = new ProcessStartInfo(fileName, "/c " + command);
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;
        processInfo.RedirectStandardError = true;
        processInfo.RedirectStandardOutput = true;
        processInfo.WorkingDirectory = workingDirectory;
        var process = Process.Start(processInfo);
        process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            Console.WriteLine("output>>" + e.Data);
        process.BeginOutputReadLine();
        
        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            Console.WriteLine("error>>" + e.Data);
        process.BeginErrorReadLine();
        process.WaitForExit();
        Console.WriteLine("ExitCode: {0}", process.ExitCode);
        process.Close();
    }
    /// <summary>
    /// Выпонлнить консольную команду
    /// </summary>
    /// <param name="command"></param>
    public static void ExecuteCommand(ProcessStartInfo processInfo)
    {
        var process = Process.Start(processInfo);
        process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            Console.WriteLine("output>>" + e.Data);
        process.BeginOutputReadLine();

        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            Console.WriteLine("error>>" + e.Data);
        process.BeginErrorReadLine();
        process.WaitForExit();
        Console.WriteLine("ExitCode: {0}", process.ExitCode);
        process.Close();
    }
}
