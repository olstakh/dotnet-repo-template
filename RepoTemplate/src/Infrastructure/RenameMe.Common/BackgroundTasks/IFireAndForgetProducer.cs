namespace RenameMe.Common.BackgroundTasks;

/// <summary>
/// Defines an interface for scheduling and consuming fire-and-forget tasks, allowing for background execution of tasks without awaiting their completion.
/// </summary>
public interface IFireAndForgetProducer
{
    /// <summary>
    /// Schedules a fire-and-forget task to be executed in the background, with support for cancellation.
    /// </summary>
    /// <param name="task">The task to be executed. The task receives a <see cref="CancellationToken"/> that can be used to observe cancellation requests.</param>
    /// <param name="cancellationToken">A token for observing cancellation requests.</param>
    /// <remarks>
    /// Cancellation of the scheduled task is cooperative, and the task should respect the provided <see cref="CancellationToken"/> to allow for graceful shutdown.
    /// Cancellation token is used for scheduling the task, as well as for canceling the task if needed. The task should be designed to handle cancellation appropriately to avoid resource leaks or unresponsive behavior.
    /// </remarks>
    void Schedule(Func<CancellationToken, Task> task, CancellationToken cancellationToken);
}
