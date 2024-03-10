using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Text;
using System.Text.Json;
using TestItAdapter;

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
    Console.WriteLine($"testRunned {DateTime.Now}\n");
    return;
});
app.MapGet("/", (IConfiguration appConfig) =>
{
    RunnerConfig config = new();
    appConfig.GetSection("RunnerConfig").Bind(config);
    return JsonSerializer.Serialize<RunnerConfig>(config);
});
app.Run();