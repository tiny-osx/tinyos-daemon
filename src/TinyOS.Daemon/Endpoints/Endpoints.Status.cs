namespace TinyOS.Daemon.Endpoints;

internal partial class Endpoints
{
    internal static void MapStatus(WebApplication app)
    {
        app.MapGet("/", () =>
        {            
            return Results.Ok();
        });
    }
}
