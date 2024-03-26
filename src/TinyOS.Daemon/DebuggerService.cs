using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace TinyOS.Daemon;

public class DebuggerService
{
    private readonly Process _process;
    private readonly TcpListener _listener;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DebuggerService> _logger;
    private readonly CancellationTokenSource _cancellationSource;
    private static char[] ARGUMENT_SEPARATORS = new char[] { ' ', '\t' };

    public DebuggerService(ILogger<DebuggerService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _listener = TcpListener.Create(0);
        _cancellationSource = new CancellationTokenSource();
        _process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _configuration["debugger:filename"] ?? "/usr/share/vsdbg/vsdbg",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
    }

    public IPEndPoint Start(string[] args)
    {        
        _cancellationSource.TryReset();        
        if (args.Count() == 0)
        {
            _process.StartInfo.Arguments = _configuration["debugger:arguments"] ?? "--interpreter=vscode";
        }
        else
        {
            _process.StartInfo.Arguments = ConcatArgs(args);
        }

        _listener.Start();
        _logger.LogTrace($"TinyOS daemon started listening on {_listener.LocalEndpoint}");

        Task.Run(ExecuteAsync);

        return (IPEndPoint)_listener.LocalEndpoint;
    }

    public void Stop()
    {   
        _cancellationSource.Cancel();
        
        _process.Dispose();
        _listener.Dispose();
        
        _logger.LogTrace($"TinyOS daemon stopped listening on {_listener.LocalEndpoint}");
    }

    protected async Task ExecuteAsync()
    {
        _logger.LogInformation($"Now listening on: pipelink://{_listener.LocalEndpoint}");
        
        var cancellationToken = _cancellationSource.Token;

        await AcceptConnectionAsync(cancellationToken);
    }

    private async Task AcceptConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            using (TcpClient remoteClient = await _listener.AcceptTcpClientAsync(cancellationToken))
            {
                _logger.LogTrace($"Accepted remote client on {_listener.LocalEndpoint}:{remoteClient.Client.RemoteEndPoint}");

                using (NetworkStream clientStream = remoteClient.GetStream())
                {
                    _process.Start();

                    Task.WhenAny(
                        clientStream.CopyToAsync(_process.StandardInput.BaseStream, cancellationToken),
                        _process.StandardOutput.BaseStream.CopyToAsync(clientStream, cancellationToken),
                        _process.StandardError.BaseStream.CopyToAsync(clientStream, cancellationToken)
                    ).Wait(cancellationToken);
                }
            }
        }
        catch (Exception exception)
            when (exception is SocketException
                || exception is OperationCanceledException
                || exception is ObjectDisposedException
                || exception is InvalidOperationException)
        {
            _logger.LogTrace($"Rejected remote client on {_listener.LocalEndpoint}");
        }
    }

    public static string ConcatArgs (string [] args, bool quote = true)
    {
        var arg = string.Empty;
        if (args != null) {
            foreach (var r in args) 
            {
                if (arg.StartsWith("--"))
                {
                    if (arg.Length > 0) 
                    {
                        arg += " ";
                    }
                    arg += quote ? Quote (r) : r;
                }
            }
        }
        return arg;
    }

    public static string Quote(string arg)
    {
        if (arg.IndexOfAny(ARGUMENT_SEPARATORS) >= 0) {
            return '"' + arg + '"';
        }
        return arg;
    }
}