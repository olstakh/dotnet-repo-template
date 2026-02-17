using System.Diagnostics;

namespace Template.IntegrationTests;

/// <summary>
/// Helper for running dotnet CLI commands and capturing output.
/// </summary>
internal static class DotnetCli
{
    /// <summary>
    /// Runs a dotnet CLI command and returns the result.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the dotnet CLI.</param>
    /// <param name="workingDirectory">The working directory for the command. If null, uses the current directory.</param>
    /// <param name="timeout">Timeout for the command. Defaults to 120 seconds.</param>
    /// <returns>A <see cref="DotnetCliResult"/> containing exit code, stdout, and stderr.</returns>
    public static async Task<DotnetCliResult> RunAsync(
        string arguments,
        string? workingDirectory = null,
        TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(120);

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.Start();

        // Read stdout and stderr concurrently to avoid deadlocks.
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        var exited = await process.WaitForExitAsync()
            .WaitAsync(timeout.Value)
            .ContinueWith(static t => !t.IsFaulted);

        if (!exited)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException(
                $"dotnet {arguments} timed out after {timeout.Value.TotalSeconds}s");
        }

        return new DotnetCliResult(
            process.ExitCode,
            await stdoutTask,
            await stderrTask);
    }
}

/// <summary>
/// Represents the result of a dotnet CLI command execution.
/// </summary>
/// <param name="ExitCode">The process exit code.</param>
/// <param name="StandardOutput">The captured standard output.</param>
/// <param name="StandardError">The captured standard error.</param>
internal sealed record DotnetCliResult(int ExitCode, string StandardOutput, string StandardError)
{
    /// <summary>
    /// Asserts that the command succeeded (exit code 0).
    /// </summary>
    /// <param name="context">Optional context message for the assertion.</param>
    public void AssertSuccess(string? context = null)
    {
        if (ExitCode != 0)
        {
            var message = $"dotnet command failed with exit code {ExitCode}.";
            if (context is not null)
            {
                message = $"{context}: {message}";
            }

            message += $"\n--- stdout ---\n{StandardOutput}\n--- stderr ---\n{StandardError}";
            Assert.Fail(message);
        }
    }
}
