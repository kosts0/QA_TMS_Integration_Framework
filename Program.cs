using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using TestItAdapter;
using TestProject;
using TestRunnerWebApi;
using TestRunnerWebApi.TestItAdapter;

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

Console.WriteLine("Starting init TestItApi");
TestItApi.Init(GetConfig(builder.Configuration));
Console.WriteLine("TestItApi init sucess");

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
    List<string> autotestGlobalIdList = requestBody.SelectTokens("AutoTests[*].GlobalId").Select(t => t.ToString()).ToList();
    RunnerConfig runnerConfig = GetConfig(appConfig);
    try
    {
        foreach(var testId in autotestGlobalIdList)
        {
            string testRunReportFolder = Guid.NewGuid().ToString();
            Console.WriteLine($"Saving test result for test {testId} in {testRunReportFolder}");
            var manualTestId = TestItApi.AutoTestsApi.GetWorkItemsLinkedToAutoTest(testId).First().GlobalId;
            Console.WriteLine($"Starting Test {manualTestId}");
            CmdHelper.ExecuteCommand($"dotnet test {Path.GetFileName(runnerConfig.DllPath)} -v n --filter \"Name~{manualTestId}\" -- TestRunParameters.Parameter(name=\\\"TestRunId\\\", value=\\\"{testRunReportFolder}\\\")", workingDirectory: Path.GetDirectoryName(GetConfig(appConfig).DllPath));
            Console.WriteLine($"TestExecute Completed");
            CmdHelper.ExecuteCommand(new ImportCommand(GetConfig(appConfig)) { TestResultDirectory = @$"{Path.GetDirectoryName(GetConfig(appConfig).DllPath)}\allure-results\{testRunReportFolder}", TestRunId = testRunId }.ProcessInfo);
            Console.WriteLine($"testRunned {DateTime.Now}\n");
        }
        
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