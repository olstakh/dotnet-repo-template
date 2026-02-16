using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace RenameMe.Common.BackgroundTasks;

internal sealed partial class DefaultFireAndForget(
    ILogger<DefaultFireAndForget> logger,
    TimeProvider timeProvider,
    TimeSpan? completionTimeout) : IFireAndForget
{
    private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    private readonly TimeSpan _completionTimeout = completionTimeout ?? TimeSpan.FromSeconds(30);
    private readonly ILogger<DefaultFireAndForget> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ConcurrentDictionary<TaskHolder, Task> _tasks = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private sealed class TaskHolder;

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> task, CancellationToken cancellationToken)
    {
        var heldTask = new TaskHolder();
        var taskAdded = 0;

        var taskWrapper = Task.Run(async () =>
        {
            try
            {
                await task(_cancellationTokenSource.Token);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            // We don't know what exceptions the task might throw, and we don't want them to crash the application, so we catch all exceptions here and log them.
            catch (Exception ex)
            {
                Log_BackgroundTaskError(_logger, ex);
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                // Spin briefly if TryAdd hasn't executed yet
                SpinWait.SpinUntil(() => Volatile.Read(ref taskAdded) == 1);

                if (!_tasks.TryRemove(heldTask, out _))
                {
                    Log_BackgroundTaskNotRemoved(_logger);
                }
            }
        }, cancellationToken);

        _tasks.TryAdd(heldTask, taskWrapper);
        Volatile.Write(ref taskAdded, 1);
    }

    /// <inheritdoc />
    public Task CompletePendingTasksAsync(CancellationToken cancellationToken)
    {
        foreach (var kvp in _tasks)
        {
            if (kvp.Value.IsCompleted)
            {
                _ = _tasks.TryRemove(kvp.Key, out _);
                // Failure to remove a completed task is not critical, so we can safely ignore the return value of TryRemove here.
            }
        }

        return Task
            .WhenAll(_tasks.Values.ToArray())
            .WaitAsync(_completionTimeout, _timeProvider, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync();

        try
        {
            await CompletePendingTasksAsync(CancellationToken.None);
        }
        catch (OperationCanceledException)
        {
            // We expect this exception to be thrown when the cancellation token is canceled, so we can safely ignore it.
        }

        _cancellationTokenSource.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "An error occurred while executing a fire-and-forget task.")]
    private static partial void Log_BackgroundTaskError(ILogger<DefaultFireAndForget> logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Fire-and-forget task was not removed from the tracking collection. This may indicate a potential functional bug.")]
    private static partial void Log_BackgroundTaskNotRemoved(ILogger<DefaultFireAndForget> logger);
}