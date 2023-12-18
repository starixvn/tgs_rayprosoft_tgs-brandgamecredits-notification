using Tgs.BrandGameCredits.Common;

namespace Tgs.BrandGameCredits.Notification.Core;

public interface IEmailLockingRepository
{
    public ValueTask<bool> LockedAsync(string code, BrandGameCreditRiskTypes riskType, CancellationToken cancellationToken = default);

    public ValueTask UnlockAsync(string code, BrandGameCreditRiskTypes riskType, CancellationToken cancellationToken = default);
}
