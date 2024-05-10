using CommonLibrary;
using Confluent.Kafka;

namespace TestRunnerKafkaConsumer;

public class ConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;

    private readonly ILogger<ConsumerService> _logger;
    RunnerConfig RunnerConfig = new();
    private readonly AgentOrcestratorServer _agentOrcestratorServer;
    public ConsumerService(IConfiguration configuration, ILogger<ConsumerService> logger, AgentOrcestratorServer agentOrcestratorServer)
    {
        _logger = logger;

        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "AutoTestConsumer",
            AutoOffsetReset = AutoOffsetReset.Earliest,
        };
        configuration.GetSection("RunnerConfig").Bind(RunnerConfig);
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _agentOrcestratorServer = agentOrcestratorServer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe("AutoTestTopic");
        _logger.LogInformation("Instance Subscribed!");
        while (!stoppingToken.IsCancellationRequested)
        {
            ProcessKafkaMessage(stoppingToken);

            Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }

        _consumer.Close();
    }

    public async void ProcessKafkaMessage(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Before staring to consume!");
            _logger.LogInformation($"Connected agents: {_agentOrcestratorServer.ConnectionIdList.Count}: {string.Join(',', _agentOrcestratorServer.ConnectionIdList)}");
            var consumeResult = _consumer.Consume(stoppingToken);
            var message = consumeResult.Message.Value;
            //await _agentOrcestratorServer.RunTest(JToken.Parse(message));
            _logger.LogInformation($"Recived run autotest message: {message}");
            /*string autotestId = JObject.Parse(message).SelectToken("AutoTestGlobalId").ToString();
            Guid testRunId = Guid.Parse(JObject.Parse(message).SelectToken("TestRunId").ToString());
            Guid testRunReportFolder = Guid.NewGuid();
            _logger.LogInformation($"Saving test result for test {autotestId} in {testRunReportFolder}");
            var manualTestId = TestItApi.AutoTestsApi.GetWorkItemsLinkedToAutoTest(autotestId).First().GlobalId;
            _logger.LogInformation($"Starting Test {manualTestId}");
            CmdHelper.ExecuteCommand($"dotnet test {Path.GetFileName(RunnerConfig.DllPath)} -v n --filter \"Name~{manualTestId}\" -- TestRunParameters.Parameter(name=\\\"TestRunId\\\", value=\\\"{testRunReportFolder}\\\")", workingDirectory: Path.GetDirectoryName(RunnerConfig.DllPath));
            _logger.LogInformation($"TestExecute Completed");
            CmdHelper.ExecuteCommand(new ImportCommand(RunnerConfig) { TestResultDirectory = @$"{Path.GetDirectoryName(RunnerConfig.DllPath)}\allure-results\{testRunReportFolder}", TestRunId = testRunId }.ProcessInfo);
            _logger.LogInformation($"testRunned {DateTime.Now}\n");*/
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing Kafka message: {ex.Message}");
        }
    }
}
