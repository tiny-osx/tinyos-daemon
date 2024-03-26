using System.Runtime.Versioning;
namespace TinyOS.Daemon.Endpoints;

/// <summary>
/// Contains extension methods for <see cref="WebApplication"/>.
/// </summary>
[UnsupportedOSPlatform("windows")]
public static class EndpointsExtensions
{
    public static WebApplication AddEndpoints(this WebApplication app)
    {
        Endpoints.MapApps(app);
        Endpoints.MapClock(app);
        Endpoints.MapDebug(app);
        Endpoints.MapStatus(app);
        return app;
    }
}
