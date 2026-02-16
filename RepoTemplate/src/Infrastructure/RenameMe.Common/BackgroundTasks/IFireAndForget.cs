namespace RenameMe.Common.BackgroundTasks;

/// <summary>
/// Defines a combined interface for both producing and consuming fire-and-forget tasks, allowing for scheduling and processing of background tasks without awaiting their completion.
/// </summary>
public interface IFireAndForget : IFireAndForgetProducer, IFireAndForgetConsumer
{
}