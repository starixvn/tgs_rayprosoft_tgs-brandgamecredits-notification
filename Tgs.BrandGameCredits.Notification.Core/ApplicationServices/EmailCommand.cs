namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

public record EmailCommand(
    string BrandCode,
    IEnumerable<string> Recipients,
    string Subject,
    string Content);
