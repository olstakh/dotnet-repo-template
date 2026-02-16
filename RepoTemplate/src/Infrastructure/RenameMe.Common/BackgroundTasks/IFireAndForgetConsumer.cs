namespace RenameMe.Common.BackgroundTasks;

/// <summary>
/// Defines an interface for consuming scheduled fire-and-forget tasks, allowing for the processing of pending tasks in the background.
/// </summary>
public interface IFireAndForgetConsumer : IAsyncDisposable
{
    /// <summary>
    /// Starts the consumer to process scheduled fire-and-forget tasks.
    /// </summary>
    /// <param name="cancellationToken">A token for scheduling cancellation.</param>
    Task CompletePendingTasksAsync(CancellationToken cancellationToken);
}