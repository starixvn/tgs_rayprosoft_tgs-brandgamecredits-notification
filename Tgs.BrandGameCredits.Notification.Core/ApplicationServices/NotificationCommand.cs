namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

public record NotificationCommand(
	Guid ResellerId,
	string ResellerCode,
	Guid LicenseeId,
	string LicenseeCode,
	Guid BrandId,
	string BrandCode);
