using CommonLibrary;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

public class WebSocketClient
{
    private RunnerConfig config { get; set; }
    static ILoggerFactory loggerFactory => LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                .AddSimpleConsole(options => {
                    options.TimestampFormat = "HH:mm:ss ";
                    options.SingleLine = true;
                 });
        });
    static async Task Main(string[] args)
    {
        IConfiguration Configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json")
              .AddEnvironmentVariables()
              .AddCommandLine(args)
              .AddJsonFile("config.json")
              .Build();
        ILogger logger = loggerFactory.CreateLogger<WebSocketClient>();
        logger.LogInformation("RunnerClient logger inited.");
        RunnerConfig config = new RunnerConfig();
        Configuration.GetSection("RunnerConfig").Bind(config);
        TestItApi.Init(config);
        using (ClientWebSocket webSocket = new ClientWebSocket())
        {
            Uri serverUri = new Uri(Configuration["WebSocketServer"]); // замените на ваш URL

            await webSocket.ConnectAsync(serverUri, new CancellationToken());

            logger.LogInformation($"WebSocket client connected. Server URL {serverUri}");
            while (true)
            {
                string runCommand = await Receive(webSocket); // Запуск метода для приема сообщений
                if (string.IsNullOrEmpty(runCommand))
                {
                    logger.LogInformation("Close message. Agent finished.");
                    return;
                }
                logger.LogInformation($"Получено: {runCommand}");
                RunTest(JToken.Parse(runCommand), config);
                await Send(webSocket, "Run Finished"); // Отправка сообщения
            }
        }
    }

    static async Task Send(ClientWebSocket webSocket, string message)
    {
        ILogger logger = loggerFactory.CreateLogger<WebSocketClient>();
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);

        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, new CancellationToken());
        logger.LogInformation($"Send to server: {message}");
    }

    static async Task<string> Receive(ClientWebSocket webSocket)
    {
        byte[] buffer = new byte[1024];
        ArraySegment<byte> segment = new ArraySegment<byte>(buffer);

        while (webSocket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(segment, new CancellationToken());
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                return message;
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the client", CancellationToken.None);
            }
        }
        return null;
    }
    static void RunTest(JToken message, RunnerConfig RunnerConfig)
    {
        ILogger logger = loggerFactory.CreateLogger<WebSocketClient>();
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Development")
        {
            logger.LogInformation($"Recived run autotest message: {message}");
            string autotestId = message.SelectToken("AutoTestGlobalId").ToString();
            Guid testRunId = Guid.Parse(message.SelectToken("TestRunId").ToString());
            Guid testRunReportFolder = Guid.NewGuid();
            logger.LogInformation($"Test result for test {autotestId} will be saved in directory {testRunReportFolder}");
            long manualTestId;
            try
            {
                manualTestId = TestItApi.AutoTestsApi.GetWorkItemsLinkedToAutoTest(autotestId).First().GlobalId;
            }catch(TestIT.ApiClient.Client.ApiException ex)
            {
                logger.LogWarning($"Error while get manual test Id\n{ex.Message}");
                return;
            }
            logger.LogInformation($"Starting test: {manualTestId} .....");
            CmdHelper.ExecuteCommand($"dotnet test {Path.GetFileName(RunnerConfig.DllPath)} -v n --filter \"Name~{manualTestId}\" -- TestRunParameters.Parameter(name=\\\"TestRunId\\\", value=\\\"{testRunReportFolder}\\\")", workingDirectory: Path.GetDirectoryName(RunnerConfig.DllPath));
            logger.LogInformation($"Test execution finished");
            CmdHelper.ExecuteCommand(new ImportCommand(RunnerConfig) { TestResultDirectory = @$"{Path.GetDirectoryName(RunnerConfig.DllPath)}\allure-results\{testRunReportFolder}", TestRunId = testRunId }.ProcessInfo);
            logger.LogInformation($"Test results send in TMS {DateTime.Now}\n");
        }
        else
        {
            Thread.Sleep(TimeSpan.FromSeconds(new Random().NextDouble()*59));
        }
    }
}