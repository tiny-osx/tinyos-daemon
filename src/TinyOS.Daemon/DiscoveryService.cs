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
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscoveryService> _logger;
    private readonly UdpClient _listener;

    public DiscoveryService(ILogger<DiscoveryService> logger, IConfiguration configuration)
    {
        var port = int.Parse(configuration["discovery:port"] ?? "8920");
        var endpoint = new IPEndPoint(IPAddress.Any, port);

        _listener = new UdpClient(endpoint)
        {
            MulticastLoopback = false
        };

        _logger = logger;
        _configuration = configuration;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _listener.Dispose();
        
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await AcceptConnectionAsync(_listener, stoppingToken);
        }
    }

    private async Task AcceptConnectionAsync(UdpClient _listener, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = await _listener.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                    var text = Encoding.UTF8.GetString(result.Buffer);
                    if (text.Contains("aa832bc6", StringComparison.OrdinalIgnoreCase))
                    {
                        await ResponseMessage(result.RemoteEndPoint, cancellationToken).ConfigureAwait(false);
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
            // Exception in this function will prevent the background service from restarting in-process.
            //_logger.LogError(ex, "Unable to bind to {Address}:{Port}", .BeginReceive.Address, endpoint.Port);
        }
    }
    private async Task ResponseMessage(IPEndPoint endpoint, CancellationToken cancellationToken)
    {
        using var udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
                
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
        
        try
        {
            var json = JsonSerializer.Serialize(hostInterface, JsonContext.Default.HostInterface);
            await udpClient.SendAsync(Encoding.UTF8.GetBytes(json), endpoint, cancellationToken).ConfigureAwait(false);
            
            _logger.LogDebug("Sending discovery response");
        }
        catch (SocketException ex)
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

    public static string? GetEnvironmentVariable(string variable)
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