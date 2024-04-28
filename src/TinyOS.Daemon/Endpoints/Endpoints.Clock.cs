using TinyOS.Daemon.Processes;

namespace TinyOS.Daemon.Endpoints;

internal partial class Endpoints
{
    internal static void MapClock(WebApplication app)
    {
        app.MapGet("/clock", () =>
        {
            return Results.Ok(DateTime.UtcNow);
        });

        app.MapPut("/clock/set", async (HttpContext context) =>
        {
            try
            {
                var date = await context.Request.ReadFromJsonAsync<DateTime>();
                long unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var result = new ProcessRunner("date", true,
                    new ProcessArgumentBuilder()
                    .Append($"--set @{unixSeconds}"))
                    .WaitForExit();

                if (File.Exists("/dev/rtc0"))
                {
                    result = new ProcessRunner("hwclock", true,
                        new ProcessArgumentBuilder()
                        .Append($"--systohc"))
                        .WaitForExit();
                }

                if (!result.Success)
                {
                    return Results.BadRequest();
                }
            }
            catch
            {
                return Results.BadRequest();
            }

            return Results.Ok(DateTime.UtcNow);
        });
    }
}
