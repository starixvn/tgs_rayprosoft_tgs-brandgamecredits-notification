using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Tgs.BrandGameCredits.Notification.Core;
using Tgs.BrandGameCredits.Notification.Repositories;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollecitonExtensions
{
    public static IServiceCollection AddBrandGameCreditsRepositories(
        this IServiceCollection services,
        Func<IServiceProvider, IDatabase> redisDatabaseResolver)
        => services
        .AddTransient<IEmailLockingRepository>((sp) => ActivatorUtilities.CreateInstance<EmailLockingRepository>(sp, redisDatabaseResolver(sp), TimeSpan.FromHours(sp.GetRequiredService<IOptions<EmailSettingsOptions>>().Value.EmailLockingHours)))
        .AddTransient<INotificationLockingRepository>((sp) => ActivatorUtilities.CreateInstance<NotificationLockingRepository>(sp, redisDatabaseResolver(sp), TimeSpan.FromSeconds(sp.GetRequiredService<IOptions<EmailSettingsOptions>>().Value.NotificationLockingSecs)));
}
