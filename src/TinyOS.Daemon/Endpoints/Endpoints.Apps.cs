using System.IO;
using System.Security.Cryptography;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

using Microsoft.Extensions.Primitives;

namespace TinyOS.Daemon.Endpoints;

[UnsupportedOSPlatform("windows")]
internal partial class Endpoints
{
    internal static void MapApps(WebApplication app)
    {
        app.MapGet("/apps/clean", () =>
        {
            DirectoryInfo directory = new DirectoryInfo(app.Environment.ContentRootPath);

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }
        });

        app.MapGet("/apps/clean/{applicationId:guid}", (Guid applicationId) =>
        {
            try
            {
                var path = Path.Combine(app.Environment.ContentRootPath, applicationId.ToString());
                Directory.Delete(path, true);
                return Results.Ok();
            }
            catch
            {
                return Results.NotFound();
            }
        });

        app.MapGet("/apps/delete/{applicationId:guid}/file/{remotePath}", (Guid applicationId, string remotePath) =>
        {
            var path = Path.Combine(app.Environment.ContentRootPath, applicationId.ToString(), remotePath);
            File.Delete(path);
        });

        app.MapGet("/apps/download/{applicationId:guid}", (Guid applicationId) =>
        {
            var filePath = Path.Combine(app.Environment.ContentRootPath, applicationId.ToString(), "filecache.bin");

            if (File.Exists(filePath))
            {
                return Results.File(filePath, "application/octet-stream");
            }

            return Results.NotFound();
        });

        _ = app.MapPost("/apps/upload/{applicationId:guid}", async (Guid applicationId, HttpRequest request) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest();
            }

            var files = request.Form.Files.OfType<IFormFile?>().ToList();
            if (files == null || files.Count <= 0)
            {
                return Results.BadRequest();
            }

            var form = await request.ReadFormAsync();

            var file = form.Files["file"];
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest();
            }

            var filePath = Path.Combine(app.Environment.ContentRootPath, applicationId.ToString(), file.FileName);
            var directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath is null)
            {
                return Results.BadRequest();
            }

            Directory.CreateDirectory(directoryPath);

            using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            if (request.Form.Keys.Count != 0)
            {
                var parameters = new Dictionary<string, string>();

                foreach (var key in request.Form.Keys)
                {
                    request.Form.TryGetValue(key, out StringValues formData);
                    parameters.Add(key, formData.ToString());
                }

                if (parameters.TryGetValue("filehash", out string? fileHash)
                     || !string.IsNullOrEmpty(fileHash))
                {
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = new BufferedStream(File.OpenRead(filePath)))
                        {
                            if (!fileHash.Equals(md5.ComputeHash(stream).ToHex()))
                            {
                                return Results.BadRequest();
                            }
                        }
                    }
                }

                if (file.FileName == "TinyOS.VScode")
                {
                    File.SetUnixFileMode(
                        filePath, // 0755
                        UnixFileMode.UserRead
                        | UnixFileMode.UserWrite
                        | UnixFileMode.UserExecute
                        | UnixFileMode.GroupRead
                        | UnixFileMode.GroupExecute
                        | UnixFileMode.OtherRead
                        | UnixFileMode.OtherExecute
                    );
                }
            }

            return Results.Ok();

        }).Accepts<IFormFile>("multipart/form-data");   
    }
}
