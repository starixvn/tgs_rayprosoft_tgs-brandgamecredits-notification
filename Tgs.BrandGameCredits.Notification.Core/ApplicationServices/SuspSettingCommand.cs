using Tgs.BrandGameCredits.Common;

namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

public record SuspSettingCommand(
	string BrandCode,
	BrandGameCreditRiskTypes RiskType,
	IEnumerable<string> Recipients);
