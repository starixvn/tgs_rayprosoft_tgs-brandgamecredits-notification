using System.Reflection;

using Tgs.BrandGameCredits.Notification.Core;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollecitonExtensions
{
    public static IServiceCollection AddBrandGameCreditsCore(
        this IServiceCollection services)
        => services
        .RegisterAllServiceUtilityTypes(
            Assembly.GetExecutingAssembly(),
            ServiceLifetime.Transient)
        .AddSingleton<INotificationDispatcher, NotificationDispatcher>();
}
