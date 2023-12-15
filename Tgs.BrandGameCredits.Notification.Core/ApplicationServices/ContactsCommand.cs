using Tgs.BrandGameCredits.Common;

namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

public record ContactsCommand(
    Guid ResellerId,
    string ResellerCode,
    Guid LicenseeId,
    string LicenseeCode,
    Guid BrandId,
    string BrandCode,
    BrandGameCreditRiskTypes RiskType);
