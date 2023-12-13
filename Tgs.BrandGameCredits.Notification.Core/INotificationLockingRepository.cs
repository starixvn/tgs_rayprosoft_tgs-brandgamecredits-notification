namespace Tgs.BrandGameCredits.Notification.Core;

public interface INotificationLockingRepository
{
    public ValueTask<bool> LockedAsync(string code, CancellationToken cancellationToken = default);

    public ValueTask UnlockAsync(string code, CancellationToken cancellationToken = default);
}
