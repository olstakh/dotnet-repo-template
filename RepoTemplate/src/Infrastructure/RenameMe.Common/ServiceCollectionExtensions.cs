using Microsoft.Extensions.DependencyInjection;
using RenameMe.Common.BackgroundTasks;

namespace RepoTemplate.Infrastructure;

/// <summary>
/// Provides extension methods for registering services related to background tasks in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the necessary services for scheduling and consuming fire-and-forget tasks in the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to which the background task services will be added.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBackgroundTasks(this IServiceCollection services)
    {
        services.AddSingleton<IFireAndForgetProducer, DefaultFireAndForget>();
        services.AddSingleton<IFireAndForgetConsumer, DefaultFireAndForget>();

        return services;
    }
}
