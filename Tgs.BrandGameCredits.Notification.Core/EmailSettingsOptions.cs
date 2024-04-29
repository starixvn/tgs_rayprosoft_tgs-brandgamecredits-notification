namespace Tgs.BrandGameCredits.Notification.Core;

public record EmailSettingsOptions
{
	public double NotificationLockingSecs { get; set; } = default!;

	public double EmailLockingHours { get; set; } = default!;

	public IEnumerable<string> BlindCarbonCopies { get; set; } = default!;
}
