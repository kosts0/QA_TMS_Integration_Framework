using System.Net.WebSockets;
using System.Text;

public class WebSocketClient
{
    static async Task Main(string[] args)
    {
        IConfiguration Configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables()
              .AddCommandLine(args)
              .Build();
        using (ClientWebSocket webSocket = new ClientWebSocket())
        {
            Uri serverUri = new Uri(Configuration["WebSocketServer"]); // замените на ваш URL

            await webSocket.ConnectAsync(serverUri, new CancellationToken());

            Console.WriteLine($"WebSocket client connected. Server URL {serverUri}");
            while (true)
            {
                string runCommand = await Receive(webSocket); // Запуск метода для приема сообщений
                Console.WriteLine($"Получено: {runCommand}");
                Thread.Sleep(TimeSpan.FromSeconds(new Random().NextDouble()*59));
                await Send(webSocket, "Run Finished"); // Отправка сообщения
            }
        }
    }

    static async Task Send(ClientWebSocket webSocket, string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);

        await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, new CancellationToken());
        Console.WriteLine($"Отправлено: {message}");
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
}
/*connection.On("RunTest", (JToken message) =>
{
    Debug.WriteLine($"Recived run autotest message: {message}");
    string autotestId = message.SelectToken("AutoTestGlobalId").ToString();
    Guid testRunId = Guid.Parse(message.SelectToken("TestRunId").ToString());
    Guid testRunReportFolder = Guid.NewGuid();
    Debug.WriteLine($"Сохранеяем результаты теста {autotestId} в папке {testRunReportFolder}");
    var manualTestId = TestItApi.AutoTestsApi.GetWorkItemsLinkedToAutoTest(autotestId).First().GlobalId;
    Debug.WriteLine($"Запускаем тест {manualTestId}");
    CmdHelper.ExecuteCommand($"dotnet test {Path.GetFileName(RunnerConfig.DllPath)} -v n --filter \"Name~{manualTestId}\" -- TestRunParameters.Parameter(name=\\\"TestRunId\\\", value=\\\"{testRunReportFolder}\\\")", workingDirectory: Path.GetDirectoryName(RunnerConfig.DllPath));
    Debug.WriteLine($"Тест завершен");
    CmdHelper.ExecuteCommand(new ImportCommand(RunnerConfig) { TestResultDirectory = @$"{Path.GetDirectoryName(RunnerConfig.DllPath)}\allure-results\{testRunReportFolder}", TestRunId = testRunId }.ProcessInfo);
    Debug.WriteLine($"Результаты теста отправлены в TMS. {DateTime.Now}\n");
});
await connection.StartAsync();


Console.ReadLine();
*/