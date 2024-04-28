using CommonLibrary;
using Confluent.Kafka;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using TestRunnerKafkaConsumer;

namespace KafkaWebSocketService
{
    public class KafkaWebSocketService
    {
        
        //private static Dictionary<string, WebSocketHandler> connectedClients = new Dictionary<string, WebSocketHandler>();
        public static async Task Main(string[] args)
        {
            object queueLock = new object();
            IConfiguration Configuration = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile("config.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables()
              .AddCommandLine(args)
              .Build();
            TestRunnerKafkaConsumer.WebSocketHandler server = new(Configuration["SocketEndpoint"]);

            Console.WriteLine("Starting init TestItApi");
            TestItApi.Init(GetConfig(Configuration));
            Console.WriteLine("TestItApi init sucess");
            using var consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                BootstrapServers = Configuration["Kafka:BootstrapServers"],
                GroupId = "AutoTestConsumer",
                AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();

            consumer.Subscribe("AutoTestTopic");

            var cancellationToken = new CancellationToken();

            while (true)
            {
                try
                {
                    var message = consumer.Consume(cancellationToken);
                    Console.WriteLine($"Прочитано сообщение из kafka {message.Value}");
                    // Пересылка сообещния по WebSocket всем подключенным клиентам
                    lock (queueLock)
                    {
                        WebSocket freeClient = null;
                        KeyValuePair<string, WebSocket> firstKeyValue = new();
                        while (freeClient == null)
                        {

                            if (WebSocketHandler.connectedClients.Count > 0)
                            {
                                firstKeyValue = WebSocketHandler.connectedClients.First();
                                freeClient = firstKeyValue.Value;
                                WebSocketHandler.connectedClients.TryRemove(firstKeyValue.Key, out _);
                            }
                        }
                        server.Send(freeClient, message.Value);
                    }
                }
                catch (ConsumeException e)
                {
                    Console.WriteLine($"Ошибка при чтении сообщения: {e.Error.Reason}");
                }
            }
        }

        static RunnerConfig GetConfig(IConfiguration appConfig)
        {
            RunnerConfig config = new();
            appConfig.GetSection("RunnerConfig").Bind(config);
            return config;
        }
    }

    /*public class WebSocketHandler : WebSocketBehavior
    {
        public static Dictionary<string, WebSocketHandler> connectedClients = new Dictionary<string, WebSocketHandler>();
        protected override void OnMessage(MessageEventArgs e)
        {
            Console.WriteLine($"Получено сообщение от клиента: {e.Data}");
            if(e.Data == "Run Finished")
            {
                connectedClients.Add(Guid.NewGuid().ToString(), this);
            }
            // Обработка сообщения от клиента
        }
        protected override void OnOpen()
        {
            string clientId = Guid.NewGuid().ToString(); // Генерируем уникальный Id для нового клиента
            connectedClients.Add(clientId, this); // Добавляем клиента в словарь
            Console.WriteLine($"Новый клиент подключен. Id: {clientId}");
        }
        public void RunTest(string runMessage)
        {
            this.Send(runMessage.ToString());
        }
    }*/
}