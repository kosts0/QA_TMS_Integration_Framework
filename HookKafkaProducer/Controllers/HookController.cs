using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using TestIT.ApiClient.Model;

namespace HookKafkaProducer
{
    [Route("api/[controller]")]
    [ApiController]
    public class HookController : ControllerBase
    {
        private readonly ProducerService _producerService;
        private readonly ILogger<HookController> _logger;
        public HookController(ProducerService producerService, ILogger<HookController> logger)
        {
            _producerService = producerService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateInventory([FromBody] JToken request)
        {
            _logger.LogInformation("POST TestRun started");
            string testRunId = request.SelectToken("TestRunId").ToString();
            foreach (var autotest in request.SelectTokens("AutoTests[*].GlobalId").Select(t => t.ToString()).ToList())
            {
                string json = $"{{\"TestRunId\": \"{testRunId}\",\n \"AutoTestGlobalId\": {autotest}}}";
                await _producerService.ProduceAsync("AutoTestTopic", json, testRunId);
            }
            _logger.LogInformation("POST TestRun finished");
            return Ok("Test Run Hooked Successfully...");
        }
        [HttpGet]
        public string Test()
        {
            _logger.LogInformation("Get Hooks API");
            return "hook_kafka_producer" + DateTime.Now;
        }
    }
}
