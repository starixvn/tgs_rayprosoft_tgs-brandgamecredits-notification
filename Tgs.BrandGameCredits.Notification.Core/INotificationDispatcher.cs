using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

namespace Tgs.BrandGameCredits.Notification.Core;

public interface INotificationDispatcher
{
	ValueTask PushCommandAsync(NotificationCommand command, CancellationToken cancellationToken = default);
}
