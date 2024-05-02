namespace FTPClient;

/// <summary>
/// Record to initialize new instance of file or directory.
/// </summary>
/// <param name="FilePath">The path to file or directory.</param>
/// <param name="IsDirectory">Shows whether the instance is file or directory.</param>
public record ListElement(string FilePath, bool IsDirectory)
{
    public override string ToString()
    {
        return $"{FilePath} {IsDirectory}";
    }
}