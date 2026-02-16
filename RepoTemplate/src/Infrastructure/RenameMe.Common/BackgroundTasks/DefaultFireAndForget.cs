using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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
    private readonly ConcurrentQueue<Task> _tasks = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Lock _lock = new();

    /// <inheritdoc />
    public void Schedule(Func<CancellationToken, Task> task, CancellationToken cancellationToken)
    {
        _tasks.Enqueue(Task.Run(async () =>
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
        }, cancellationToken));
    }

    /// <inheritdoc />
    public Task CompletePendingTasksAsync(CancellationToken cancellationToken)
    {
        var scheduledTasks = GetScheduledTasks();

        return Task
            .WhenAll(scheduledTasks)
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

     private Task[] GetScheduledTasks()
    {
        lock (_lock)
        {
            var scheduledTasks = _tasks.ToArray();
            _tasks.Clear();
            return scheduledTasks;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "An error occurred while executing a fire-and-forget task.")]
    private static partial void Log_BackgroundTaskError(ILogger<DefaultFireAndForget> logger, Exception ex);
}