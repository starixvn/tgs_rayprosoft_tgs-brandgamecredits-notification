using Tgs.BrandGameCredits.Common;

namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

public record SuspSettingCommand(
    string BrandCode,
    GameCreditRiskTypes RiskType,
    IEnumerable<string> Recipients);
