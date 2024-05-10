using Confluent.Kafka;

namespace HookKafkaProducer
{
    public class ProducerService
    {
        private readonly IConfiguration _configuration;

        private readonly IProducer<string, string> _producer;
        private readonly ILogger<ProducerService> _logger;

        public ProducerService(IConfiguration configuration, ILogger<ProducerService> logger)
        {
            _logger = logger;
            _configuration = configuration;
            
            string bootstrapServers = _configuration["Kafka:BootstrapServers"];
            _logger.LogInformation($"Starting connect to Kafka Bootstrap servers {bootstrapServers}");
            var producerconfig = new ProducerConfig
            {
                BootstrapServers = bootstrapServers
            };
            _producer = new ProducerBuilder<string, string>(producerconfig).Build();
            _logger.LogInformation("Connection to Bootstrap servers finished");
        }

        public async Task ProduceAsync(string topic, string message, string key = null)
        {
            var kafkamessage = new Message<string, string> { Value = message, Key = Guid.NewGuid().ToString() };

            await _producer.ProduceAsync(topic, kafkamessage);
        }
    }
}
