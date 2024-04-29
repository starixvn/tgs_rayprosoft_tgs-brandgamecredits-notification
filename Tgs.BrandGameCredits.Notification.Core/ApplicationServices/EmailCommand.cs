using Tgs.BrandGameCredits.Common;

namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

public record EmailCommand(
	string BrandCode,
	BrandGameCreditRiskTypes RiskType,
	IEnumerable<string> Recipients,
	string Subject,
	string Content);
