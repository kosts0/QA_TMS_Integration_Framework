using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using TestItAdapter;
using TestProject;
using TestRunnerWebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});
builder.Configuration.AddJsonFile("config.json");
var app = builder.Build();

// Configure the HTTP request pipeline.
//app.UseHttpsRedirection();

app.MapPut("/runTest", (HttpRequest request, IConfiguration appConfig) =>
{
    var requestContent = "";
    request.EnableBuffering();
    using (var reader = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
    {
        requestContent = reader.ReadToEnd();
    }
    request.Body.Position = 0;

    JToken requestBody = JToken.Parse(requestContent);

    Guid testRunId = Guid.Parse(requestBody["TestRunId"]?.ToString());
    List<string> autotestList = requestBody.SelectTokens("$..AutoTests..Id").Select(t => t.ToString()).ToList();
    try
    {
        string testRunReportFolder = "ExampleTestRun2";
        CmdHelper.ExecuteCommand($"dotnet test TestProject.dll -v n --filter \"Name~NewTest\" -- TestRunParameters.Parameter(name=\\\"TestRunId\\\", value=\\\"{testRunReportFolder}\\\")", workingDirectory: @"D:\SPBPU\dipl\NUnit\bin\Debug\net6.0");
        //CmdHelper.ExecuteCommand($@"D:\SPBPU\dipl\NUnit\bin\Debug\net6.0\allure-results\TestRun2 1c7d6d95-86e3-4afc-9a9a-6c8a6694a20a", workingDirectory: @"D:\SPBPU\dipl\TestItAllureImporterVenv", fileName: "UpdateResults.bat");
        CmdHelper.ExecuteCommand(new ImportCommand(GetConfig(appConfig)) { TestResultDirectory = @$"D:\SPBPU\dipl\NUnit\bin\Debug\net6.0\allure-results\{testRunReportFolder}", TestRunId = testRunId }.ProcessInfo);
        Console.WriteLine($"testRunned {DateTime.Now}\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"TestExec failed {ex.Message}");
    } 
    return;
});
app.MapGet("/", (IConfiguration appConfig) =>
{
    RunnerConfig config = GetConfig(appConfig);
    return JsonSerializer.Serialize<RunnerConfig>(config);
});
app.Run();


RunnerConfig GetConfig(IConfiguration appConfig)
{
    RunnerConfig config = new();
    appConfig.GetSection("RunnerConfig").Bind(config);
    return config;
}