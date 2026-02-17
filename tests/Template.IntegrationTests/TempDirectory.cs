namespace Template.IntegrationTests;

/// <summary>
/// Manages a temporary directory for test isolation and cleans it up on disposal.
/// </summary>
internal sealed class TempDirectory : IDisposable
{
    public TempDirectory()
    {
        Path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "dotnet-repo-template-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(Path);
    }

    /// <summary>
    /// Gets the full path to the temporary directory.
    /// </summary>
    public string Path { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
#pragma warning disable CA1031 // Cleanup is best-effort; don't fail the test if temp dir removal fails
        catch
        {
            // Best-effort cleanup.
        }
#pragma warning restore CA1031
    }
}
