namespace Template.IntegrationTests;

/// <summary>
/// Integration tests that validate the dotnet new template produces
/// a valid, buildable project for each combination of parameters.
/// </summary>
public sealed class TemplateInstantiationTests(TemplateFixture fixture) : IClassFixture<TemplateFixture>, IDisposable
{
    private readonly TemplateFixture _fixture = fixture;
    private readonly TempDirectory _tempDir = new();

    /// <summary>
    /// Default template (with analyzers) should produce a buildable solution.
    /// </summary>
    [Fact]
    public async Task DefaultTemplate_WithAnalyzers_Builds()
    {
        var projectName = "TestWithAnalyzers";
        var outputDir = Path.Combine(_tempDir.Path, projectName);

        // Instantiate the template.
        var newResult = await DotnetCli.RunAsync(
            $"new repo-template -n {projectName} -o \"{outputDir}\"");
        newResult.AssertSuccess("dotnet new failed");

        // Verify CodeAnalysis folder exists.
        Assert.True(
            Directory.Exists(Path.Combine(outputDir, "CodeAnalysis")),
            "CodeAnalysis folder should exist when includeAnalyzers is true (default).");

        // Verify the slnx references the analyzer project.
        var slnxContent = await File.ReadAllTextAsync(
            Path.Combine(outputDir, $"{projectName}.slnx"),
            TestContext.Current.CancellationToken);
        Assert.Contains("CodeAnalysis", slnxContent);

        // Init git so SourceLink/NBGV don't complain.
        await InitGitRepo(outputDir);

        // Build.
        var buildResult = await DotnetCli.RunAsync(
            $"build \"{outputDir}\" --configuration Release",
            workingDirectory: outputDir);
        buildResult.AssertSuccess("dotnet build failed for default template");
    }

    /// <summary>
    /// Template without analyzers should produce a buildable solution
    /// with no CodeAnalysis folder.
    /// </summary>
    [Fact]
    public async Task Template_WithoutAnalyzers_Builds()
    {
        var projectName = "TestNoAnalyzers";
        var outputDir = Path.Combine(_tempDir.Path, projectName);

        // Instantiate the template without analyzers.
        var newResult = await DotnetCli.RunAsync(
            $"new repo-template -n {projectName} -o \"{outputDir}\" --include-analyzers false");
        newResult.AssertSuccess("dotnet new failed");

        // Verify CodeAnalysis folder does NOT exist.
        Assert.False(
            Directory.Exists(Path.Combine(outputDir, "CodeAnalysis")),
            "CodeAnalysis folder should NOT exist when includeAnalyzers is false.");

        // Verify the slnx does NOT reference the analyzer project.
        var slnxContent = await File.ReadAllTextAsync(
            Path.Combine(outputDir, $"{projectName}.slnx"),
            TestContext.Current.CancellationToken);
        Assert.DoesNotContain("CodeAnalysis", slnxContent);

        // Verify Directory.Build.props does NOT contain the analyzer ProjectReference.
        var buildPropsContent = await File.ReadAllTextAsync(
            Path.Combine(outputDir, "Directory.Build.props"),
            TestContext.Current.CancellationToken);
        Assert.DoesNotContain("CodeAnalysis", buildPropsContent);

        // Init git so SourceLink/NBGV don't complain.
        await InitGitRepo(outputDir);

        // Build.
        var buildResult = await DotnetCli.RunAsync(
            $"build \"{outputDir}\" --configuration Release",
            workingDirectory: outputDir);
        buildResult.AssertSuccess("dotnet build failed for template without analyzers");
    }

    /// <summary>
    /// The generated project's name should replace all occurrences of "RenameMe".
    /// </summary>
    [Fact]
    public async Task Template_ProjectName_IsReplaced()
    {
        var projectName = "MyAwesomeService";
        var outputDir = Path.Combine(_tempDir.Path, projectName);

        var newResult = await DotnetCli.RunAsync(
            $"new repo-template -n {projectName} -o \"{outputDir}\"");
        newResult.AssertSuccess("dotnet new failed");

        // Solution file should be renamed.
        Assert.True(
            File.Exists(Path.Combine(outputDir, $"{projectName}.slnx")),
            $"Expected {projectName}.slnx to exist.");

        // Main project should be renamed.
        Assert.True(
            File.Exists(Path.Combine(outputDir, "src", projectName, $"{projectName}.csproj")),
            $"Expected src/{projectName}/{projectName}.csproj to exist.");

        // "RenameMe" should not appear in the solution file.
        var slnxContent = await File.ReadAllTextAsync(
            Path.Combine(outputDir, $"{projectName}.slnx"),
            TestContext.Current.CancellationToken);
        Assert.DoesNotContain("RenameMe", slnxContent);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _tempDir.Dispose();
    }

    private static async Task InitGitRepo(string directory)
    {
        // git init
        using var initProcess = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = "init",
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        initProcess.Start();
        await initProcess.WaitForExitAsync();

        // Also do an initial commit so NBGV can compute version.
        using var addProcess = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = "add -A",
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        addProcess.Start();
        await addProcess.WaitForExitAsync();

        using var commitProcess = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "git",
                Arguments = "commit -m \"Initial commit\" --allow-empty",
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment =
                {
                    ["GIT_AUTHOR_NAME"] = "Test",
                    ["GIT_AUTHOR_EMAIL"] = "test@test.com",
                    ["GIT_COMMITTER_NAME"] = "Test",
                    ["GIT_COMMITTER_EMAIL"] = "test@test.com",
                },
            },
        };

        commitProcess.Start();
        await commitProcess.WaitForExitAsync();
    }
}
