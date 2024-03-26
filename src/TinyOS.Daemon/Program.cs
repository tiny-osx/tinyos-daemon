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
                ContentRootPath = "/apps/",
            });

        builder.WebHost.UseUrls("http://*:8920");

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, EndpointsJsonContext.Default);
        });
        
        builder.Services.AddScoped<DebuggerService>();

        var app = builder.Build();
        
        app.MapGet("/status", () => Results.Ok("{}"));
        app.AddEndpoints();
        
        app.Run();
    }
}