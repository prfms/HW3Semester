namespace FTPServer;
using System.Text;
using System.Net;
using System.Net.Sockets;

/// <summary>
/// Server which can handle simple FTP requests.
/// </summary>
public class Server
{
    private readonly CancellationTokenSource _tokenSource;

    private readonly TcpListener _listener;

    /// <summary>
    /// Initializes a new instance of the <see cref="Server"/> class.
    /// </summary>
    /// <param name="port">port number.</param>
    public Server(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
      
        _tokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Starts the server.  
    /// </summary>
    public async Task Start()
    {
        _listener.Start();

        while (!_tokenSource.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(_tokenSource.Token);
            await Task.Run(async () =>
            {
                await using var stream = client.GetStream();
                using var reader = new StreamReader(stream);

                while (await reader.ReadLineAsync() is { } request)
                {
                    if (request.StartsWith("1 "))
                    {
                        await ListHandleAsync(request[2..], stream);
                    }

                    if (request.StartsWith("2 "))
                    {
                        await GetHandleAsync(request[2..], stream);
                    }
                }
                client.Close();
            });
        }
    }

    /// <summary>
    /// Stops the server.
    /// </summary>
    public void Stop()
    {
        _tokenSource.Cancel();
        _listener.Stop();
    } 
    
    private static async Task ListHandleAsync(string path, Stream stream)
    {
        if (!Directory.Exists(path))
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes("-1\n").ToArray());
            await stream.FlushAsync();
        }

        var result = new StringBuilder();

        var directories = Directory.GetFileSystemEntries(path);
        var size = directories.Length;

        foreach (var directory in directories)
        {
            var isDirectory = Directory.Exists(directory);
            result.Append($" {directory} {isDirectory}");
        }

        result.Insert(0, $"{size}");
        result.Append('\n');

        await stream.WriteAsync(Encoding.UTF8.GetBytes(result.ToString()));
        await stream.FlushAsync();
    }
    
    private static async Task GetHandleAsync(string path, Stream stream)
    {
        if (!File.Exists(path))
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes("-1 "));
            await stream.FlushAsync();
        }
        
        var content = await File.ReadAllBytesAsync(path);
        await stream.WriteAsync(Encoding.UTF8.GetBytes($"{new FileInfo(path).Length} "));
        await stream.WriteAsync(content);
        
        await stream.FlushAsync();
    }
}

