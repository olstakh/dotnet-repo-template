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
        services.AddSingleton<DefaultFireAndForget>();
        services.AddSingleton<IFireAndForgetProducer>(sp => sp.GetRequiredService<DefaultFireAndForget>());
        services.AddSingleton<IFireAndForgetConsumer>(sp => sp.GetRequiredService<DefaultFireAndForget>());
        return services.AddTimeProvider();
    }

    /// <summary>
    /// Registers a singleton TimeProvider service in the dependency injection container, which can be used for scheduling and timing operations throughout the application.
    /// </summary>
    /// <param name="services">The service collection to which the TimeProvider service will be added.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddTimeProvider(this IServiceCollection services) =>
        services.AddSingleton(TimeProvider.System);
}
