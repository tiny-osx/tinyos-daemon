using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Collections;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace TinyOS.Daemon;

public class DiscoveryService : BackgroundService
{
    private readonly UdpClient _udpListener;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscoveryService> _logger;

    public DiscoveryService(ILogger<DiscoveryService> logger, IConfiguration configuration)
    {
        var port = int.Parse(configuration["discovery:port"] ?? "8920");
        var endpoint = new IPEndPoint(IPAddress.Any, port);

        _udpListener = new UdpClient(endpoint)
        {
            MulticastLoopback = false
        };

        _logger = logger;
        _configuration = configuration;
    }
    
    public override void Dispose()
    {
        _udpListener?.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await AcceptConnectionAsync(stoppingToken);
        }
    }

    private async Task AcceptConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpListener.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                    var text = Encoding.UTF8.GetString(result.Buffer);
                    if (text.Contains("aa832bc6", StringComparison.OrdinalIgnoreCase))
                    {
                        await ResponseMessageAsync(result.RemoteEndPoint, cancellationToken).ConfigureAwait(false);
                    }
                }
                catch (SocketException ex)
                {
                    _logger.LogError(ex, "Failed to receive data from socket");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Broadcast socket operation cancelled");
        }
        catch (Exception ex)
        {
            if (_udpListener.Client.LocalEndPoint is IPEndPoint)
            {
                _logger.LogError(ex, $"Unable to bind to {_udpListener.Client.LocalEndPoint}");
            };
        }
    }

    private async Task ResponseMessageAsync(IPEndPoint endpoint, CancellationToken cancellationToken)
    {
        try
        {
            var hostInterface = new HostInterface()
            {
                Host = Dns.GetHostName(),
                BoardType = GetEnvironmentVariable("BOARD") ?? "unknown",
                AdaptorInterfaces = GetAvailableInterfaces()
            };

            if (hostInterface.AdaptorInterfaces.Count == 0)
            {
                _logger.LogWarning("Unable to respond to server discovery request because the local ip address could not be determined");
                return;
            }

            var json = JsonSerializer.Serialize(hostInterface, JsonContext.Default.HostInterface);
            try
            {
                using (var udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
                {
                    await udpClient.SendAsync(Encoding.UTF8.GetBytes(json), endpoint, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (SocketException se)
            {
                _logger.LogError(se, "Socket error sending response message");
            }

            _logger.LogDebug("Sending discovery response");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending response message");
        }
    }

    private static List<AdaptorInterface> GetAvailableInterfaces()
    {
        var interfaces = new List<AdaptorInterface>();

        foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Unknown) continue;
            if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
            if (networkInterface.OperationalStatus != OperationalStatus.Up) continue;

            var v4Address = new List<string>();
            var v6Address = new List<string>();

            var ipProperties = networkInterface.GetIPProperties();
            foreach (UnicastIPAddressInformation unicast in ipProperties.UnicastAddresses)
            {
                if (unicast.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    v4Address.Add(unicast.Address.ToString());
                }
                if (unicast.Address.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    v6Address.Add(unicast.Address.ToString());
                }
            }

            var priorities = new string[] { "eth0", "eth1", "wlan0", "wlan0", "usb0", "usb1" };

            var deviceInterface = new AdaptorInterface()
            {
                Name = networkInterface.Name,
                IPv4Address = v4Address,
                IPv6Address = v6Address,
                Priority = Array.IndexOf(priorities, networkInterface.Name)
            };

            interfaces.Add(deviceInterface);
        }

        return interfaces;
    }

    private static string? GetEnvironmentVariable(string variable)
    {
        foreach (var item in Environment.GetEnvironmentVariables())
        {
            if (item is DictionaryEntry entry)
            {
                if (entry.Key is not string key) return null;

                if (key.Equals(variable))
                {
                    return entry.Value as string;
                }
            }
        }

        return null;
    }
}