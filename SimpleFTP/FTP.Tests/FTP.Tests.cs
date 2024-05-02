using FTPClient;
using FTPServer;
using NUnit.Framework;

namespace FTP.Tests;

public class SimpleFtpTests
{
    private const int port = 8888;

    private const string host = "localhost";
    
    private readonly Server _server = new(port);

    private readonly Client _client = new(port, host);

    [SetUp]
    public void Setup()
        =>Task.Run(() => _server.Start());
    
    [Test]
    public async Task ListRequest_ShouldReturnExpectedValue()
    {
        const string path = "../../../Files";
        var response1 = new ListElement("../../../Files\\EmptyFile.txt", false);
        var response2 = new ListElement("../../../Files\\SomeDirectory", true);
        var expectedResult = $"{response1} {response2}";

        var actualResponse = await _client.ListAsync(path);
        var result = string.Join(' ', actualResponse.Select(element => element.ToString()));
        
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public async Task GetRequest_ShouldReturnExpectedValue()
    {
        const string path = "../../../Files\\SomeDirectory\\File.txt";
        var expectedResult = await File.ReadAllBytesAsync(path);
        
        using var stream = new MemoryStream();
        await _client.GetAsync(path, stream);
        var result = stream.ToArray();
        
        Assert.That(result, Is.EqualTo(expectedResult));
    }
    
    [Test]
    public void NonExistentDirectoryForListRequest_ShouldReturnException()
    {
        const string path = "../../../Files\\InvalidPath";

        Assert.ThrowsAsync<DirectoryNotFoundException>(async () => await _client.ListAsync(path));
    }
    
    [Test]
    public void NonExistentDirectoryForGetRequest_ShouldReturnException()
    {
        const string path = "../../../Files\\InvalidPath";
        using var stream = new MemoryStream();
        
        Assert.ThrowsAsync<FileNotFoundException>(async () => await _client.GetAsync(path, stream));
    }
}