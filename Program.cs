using CommonLibrary;
using TestRunnerKafkaConsumer;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("config.json");
builder.Services.AddSignalR().AddHubOptions<AgentOrcestratorServer>(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(5);
});
var app = builder.Build();

app.UseRouting();
app.UseDefaultFiles();
app.MapHub<AgentOrcestratorServer>("/chat");



Console.WriteLine("Starting init TestItApi");
TestItApi.Init(GetConfig(builder.Configuration));
Console.WriteLine("TestItApi init sucess");


app.Run();


RunnerConfig GetConfig(IConfiguration appConfig)
{
    RunnerConfig config = new();
    appConfig.GetSection("RunnerConfig").Bind(config);
    return config;
}