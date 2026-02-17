namespace Template.IntegrationTests;

/// <summary>
/// Shared fixture that packs the template once and installs it for all tests.
/// Uninstalls the template on disposal.
/// </summary>
public sealed class TemplateFixture : IAsyncLifetime
{
    private string? _packagePath;

    /// <summary>
    /// Gets the path to the packed .nupkg file.
    /// </summary>
    public string PackagePath => _packagePath ?? throw new InvalidOperationException("Fixture not initialized.");

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        var repoRoot = FindRepoRoot();
        var templateProjectPath = Path.Combine(repoRoot, "Template", "RepoTemplate.csproj");
        var artifactsDir = Path.Combine(repoRoot, "test-artifacts");

        // Pack the template.
        var packResult = await DotnetCli.RunAsync(
            $"pack \"{templateProjectPath}\" --configuration Release --output \"{artifactsDir}\"");
        packResult.AssertSuccess("Failed to pack template");

        // Find the nupkg.
        var nupkgFiles = Directory.GetFiles(artifactsDir, "*.nupkg");
        Assert.True(nupkgFiles.Length > 0, "No .nupkg files found after packing.");
        _packagePath = nupkgFiles[0];

        // Uninstall any previously installed version (ignore failure).
        await DotnetCli.RunAsync("new uninstall DotnetRepo-olstakh");

        // Install from the local package.
        var installResult = await DotnetCli.RunAsync($"new install \"{_packagePath}\"");
        installResult.AssertSuccess("Failed to install template from local package");
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        // Uninstall the template to leave the machine clean.
        await DotnetCli.RunAsync("new uninstall DotnetRepo-olstakh");

        // Clean up the test-artifacts directory.
        var repoRoot = FindRepoRoot();
        var artifactsDir = Path.Combine(repoRoot, "test-artifacts");
        if (Directory.Exists(artifactsDir))
        {
            Directory.Delete(artifactsDir, recursive: true);
        }
    }

    private static string FindRepoRoot()
    {
        // Walk up from the test assembly location to find the repo root (contains global.json).
        var dir = AppContext.BaseDirectory;
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir, "global.json")))
            {
                return dir;
            }

            dir = Path.GetDirectoryName(dir);
        }

        throw new InvalidOperationException(
            "Could not find repo root. Ensure global.json exists at the repository root.");
    }
}
