using TestIT.ApiClient.Client;

namespace CommonLibrary;

public class TestItApi
{
    private static TestItApi _instance;
    Configuration config;
    HttpClient httpClient;
    HttpClientHandler httpClientHandler;
    public static TestItApi Init(RunnerConfig config)
    {
        if(_instance != null) return _instance;
        _instance = new();
        _instance.config = new();
        _instance.config.BasePath = config.TmsUrl;
        _instance.config.AddApiKey("Authorization", config.TmsApiKey);
        _instance.config.AddApiKeyPrefix("Authorization", "PrivateToken");
        _instance.httpClient = new();
        _instance.httpClientHandler = new HttpClientHandler();
        return _instance;
    }
    /// <summary>
    /// AutoTestsApi
    /// </summary>
    public static TestIT.ApiClient.Api.AutoTestsApi AutoTestsApi => new(_instance.httpClient, _instance.config, _instance.httpClientHandler);
}
