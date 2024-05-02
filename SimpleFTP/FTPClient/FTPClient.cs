using System.Text;

namespace FTPClient;

using System.Net.Sockets;

/// <summary>
/// Represents an FTP client that can send requests to an FTP server.
/// </summary>
public class Client
{
    private readonly int _port;
    private readonly string _address;

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="port">Port for connection.</param>
    /// <param name="address">Name of host.</param>
    public Client(int port, string address)
    {
        _port = port;
        _address = address;
    }

    /// <summary>
    /// Get list of all files and subdirectories in the path.
    /// </summary>
    /// <param name="path">Path to directory.</param>
    /// <returns>List of directory elements.</returns>
    public async Task<List<ListElement>> ListAsync(string path)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_address, _port);

        var request = $"1 {path}\n";

        var stream = client.GetStream();

        await stream.WriteAsync(Encoding.UTF8.GetBytes(request));
        await stream.FlushAsync();

        return await HandleListResponse(stream);
    }

    /// <summary>
    /// Get file content by its path.
    /// </summary>
    /// <param name="path">Path to the file.</param>
    /// <param name="outStream">Stream for writing the result.</param>
    /// <returns>File content.</returns>
    public async Task GetAsync(string path, Stream outStream)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_address, _port);

        var request = $"2 {path}\n";

        var stream = client.GetStream();

        await using var writer = new StreamWriter(stream);

        await writer.WriteAsync(request);
        await writer.FlushAsync();

        await HandleGetResponse(stream, outStream);
    }
    
    private static async Task<List<ListElement>> HandleListResponse(Stream stream)
    {
        using var reader = new StreamReader(stream);
        
        var response = await reader.ReadLineAsync();
        if (response is null)
        {
            throw new InvalidDataException();
        }
        
        var parts = response.Split();
        
        var size = int.Parse(parts[0]);

        if (size == -1)
        {
            throw new DirectoryNotFoundException();
        }

        var result = new List<ListElement>();

        for (var i = 1; i <= size * 2; i += 2)
        {
            var fileName = parts[i];
            var isDirectory = bool.Parse(parts[i + 1]);

            result.Add(new ListElement(fileName, isDirectory));
        }
        
        return result.ToList();
    }

    private static async Task HandleGetResponse(Stream stream, Stream outStream)
    {
        var sizeList = new List<byte>();

        int readByte;
        while ((readByte = stream.ReadByte()) != ' ')
        {
            sizeList.Add((byte)readByte);
        }

        var contentSize = int.Parse(Encoding.UTF8.GetString(sizeList.ToArray()));

        if (contentSize == -1)
        {
            throw new FileNotFoundException();
        }

        const int bodyBufferSize = 1024;
        var bodyBuffer = new byte[bodyBufferSize];

        var downloadedBytes = 0;

        while (downloadedBytes < contentSize)
        {
            var charsRead = await stream.ReadAsync(bodyBuffer);

            downloadedBytes += charsRead;
            await outStream.WriteAsync(bodyBuffer.Take(charsRead).ToArray());
        }
    }
}