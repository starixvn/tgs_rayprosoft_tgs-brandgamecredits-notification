using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Notification;
using Tgs.BrandGameCredits.Notification.Core;
using Tgs.Email.Client;
using Tgs.Notification.Contacts.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollecitonExtensions
{
	public static IServiceCollection AddBrandGameCredits(
		this IServiceCollection services,
		Action<RedisOptions, IServiceProvider> redisOptions,
		Action<BrandGameCreditsClientOptions, IServiceProvider> brandGameCreditsClientOptions,
		Action<NotificationContactsClientOptions, IServiceProvider> notificationContactsClientOptions,
		Action<EmailSettingsOptions, IServiceProvider> emailSettingsOptions)
		=> services
		.AddOptions<RedisOptions>()
		.Configure(redisOptions)
		.Services
		.AddOptions<EmailSettingsOptions>()
		.Configure(emailSettingsOptions)
		.Services
		.AddSingleton<RedisConnectionManager>()
		.AddBrandGameCreditsClient(brandGameCreditsClientOptions)
		.AddNotificationContactsClient(notificationContactsClientOptions)
		.AddBrandGameCreditsCore()
		.AddBrandGameCreditsRepositories(redisDatabaseResolver: (sp) => sp.GetRequiredService<RedisConnectionManager>().Database);
}
