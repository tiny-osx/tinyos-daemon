namespace TinyOS.Daemon.Endpoints;

internal partial class Endpoints
{
    //CancellationToken cancellationToken = new CancellationToken();
    
    internal static void MapDebug(WebApplication app)
    {
        // app.MapGet("/debug/args", (HttpRequest request, IConfiguration configuration) =>
        // {
        //     var arguments = configuration["debugger:arguments"] ?? "--interpreter=vscode";
        //     string[] args = arguments.Split(' ');

        //     return Results.Ok(args);
        // });

        // app.MapPut("/debug/args/set", async (HttpRequest request, IConfiguration configuration) =>
        // {
        //     var arguments = await request.ReadFromJsonAsync<string[]>();
        //     if (arguments is not null)
        //     {
        //         if (arguments.Count() > 0)
        //         {
        //             configuration["debugger:arguments"] = string.Join(" ", arguments);
        //         }

        //         return Results.Ok();
        //     }

        //     return Results.BadRequest();
        // });

        app.MapPut("/debug", async (HttpRequest request, DebuggerService debugger) =>
        {
            var arguments = await request.ReadFromJsonAsync<string[]>();
            if (arguments is not null)
            {
                var ipEndPoint = debugger.Start(arguments); 
                
                return Results.Ok(ipEndPoint.Port);
            }

            return Results.BadRequest();
        });
    }
}
