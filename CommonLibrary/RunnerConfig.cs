using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary;

public class RunnerConfig
{
    public string DllPath { get; set; }
    public string TmsUrl { get; set; }
    public string TmsApiKey { get; set; }
    /// <summary>
    /// Путь до дистрибутива Allure CTL
    /// </summary>
    public string TestImporterDirectory { get; set; }
}
