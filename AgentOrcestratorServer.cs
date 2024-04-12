using CommonLibrary;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
namespace TestRunnerKafkaConsumer
{
    public class AgentOrcestratorServer : Hub
    {
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly ILogger<AgentOrcestratorServer> _logger;
        WebSocketBehavior eh;
        RunnerConfig RunnerConfig = new();
        public AgentOrcestratorServer(IConfiguration configuration, ILogger<AgentOrcestratorServer> logger)
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
            _consumer.Subscribe("AutoTestTopic");
            _logger.LogInformation("SignalR Hub подключен к топику AutoTestTopic!");
            ProcessKafkaMessage();
        }
        public async void ProcessKafkaMessage()
        {
            try
            {
                _logger.LogInformation("Before staring to consume!");
                while (true)
                {
                    var consumeResult = _consumer.Consume();
                    var message = consumeResult.Message.Value;
                    _logger.LogInformation($"Получено сообщение из топика kafka о запуске автотеста: {message}");
                    this.RunTest(JToken.Parse(message)).Start();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка обработки сообщения из kafka: {ex.Message}");
            }
        }
        public List<string> ConnectionIdList { get; set; } = new();
        private List<string> InProcessConnecions { get; set; } = new();
        public async Task Send(string message)
        {
            await this.Clients.All.SendAsync("Send", message);
        }
        public override Task OnConnectedAsync()
        {
            var connectionId = this.Context.ConnectionId;
            ConnectionIdList.Add(connectionId);
            _logger.LogInformation($"Подключен тест-раннер {connectionId}");
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = this.Context.ConnectionId;
            ConnectionIdList.Remove(connectionId);
            _logger.LogWarning($"Отключен тест-раннер {connectionId}");
            return base.OnDisconnectedAsync(exception);
        }
        public async Task RunTest(JToken runCommand)
        {
            var connectionId = ConnectionIdList.FirstOrDefault(c => !InProcessConnecions.Contains(c));
            while(connectionId == null)
            {
                connectionId = ConnectionIdList.FirstOrDefault(c => !InProcessConnecions.Contains(c));
            }
            InProcessConnecions.Add(connectionId);
            _logger.LogInformation($"Запуск теста на раннере {connectionId}");
            await this.Clients.Client(connectionId).SendAsync("RunTest", runCommand);
        }
        public async Task TestFinished(string message)
        {
            _logger.LogInformation($"Test Finished from {Context.ConnectionId} {DateTime.Now}, {message}");
        }
    }
}
