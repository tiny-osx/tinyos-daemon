
using TinyOS.Daemon.Endpoints;

namespace TinyOS.Daemon;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(
            new WebApplicationOptions() 
            {
                Args = args, 
                //ContentRootPath = "/apps/"
            });
            
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