using System.Runtime.Versioning;

using TinyOS.Daemon.Endpoints;

namespace TinyOS.Daemon;

internal class Program
{
    [UnsupportedOSPlatform("windows")]
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions() 
            {
                Args = args, 
                ContentRootPath = "/data/",
            });

        var port = int.Parse(builder.Configuration["debugger:port"] ?? "8920");
        builder.WebHost.UseUrls($"http://*:{port}");

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, EndpointsJsonContext.Default);
        });
        
        builder.Services.AddScoped<DebuggerService>();
        builder.Services.AddHostedService<DiscoveryService>();

        var app = builder.Build();
        
        app.MapGet("/status", () => Results.Ok("{}"));
        app.AddEndpoints();
        
        app.Run();
    }
}
