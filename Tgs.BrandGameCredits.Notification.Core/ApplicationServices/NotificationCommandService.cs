using Microsoft.Extensions.Logging;

using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common;
using Tgs.ServiceUtility;

namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

internal class NotificationCommandService(
	ILogger<NotificationCommandService> logger,
	INotificationLockingRepository lockingRepository,
	IBrandGameCreditsClient client,
	ICommandService<ContactsCommand> service) : ICommandService<NotificationCommand>
{
	public async ValueTask ExecuteAsync(NotificationCommand command, CancellationToken cancellationToken = default)
	{
		if (!await lockingRepository.LockedAsync(
			command.BrandCode,
			cancellationToken)
			.ConfigureAwait(false))
		{
			logger.LogWarning("LogOn:{logOn}  Message:This {brand} notification frequency is too frequent.",
				DateTime.Now.ToString("o"),
				command.BrandCode);

			return;
		}

		var gameCredit = await client.GameCreditGetCurrentAsync(
			 brandCode: command.BrandCode,
			 cancellationToken: cancellationToken)
			 .ConfigureAwait(false)
			 ?? throw new NullReferenceException($"This brand does not exist.{command.BrandCode}");

		logger.LogInformation("LogOn:{logOn}  Message:This {brand} risk is {types}.",
			DateTime.Now.ToString("o"),
			command.BrandCode,
			gameCredit.CreditRiskType.ToString());

		if (gameCredit.CreditRiskType == BrandGameCreditRiskTypes.NormalRisk)
		{
			return;
		}

		await service.ExecuteAsync(
			command: new ContactsCommand(
				ResellerId: gameCredit.ResellerId,
				ResellerCode: gameCredit.ResellerCode,
				LicenseeId: gameCredit.LicenseeId,
				LicenseeCode: gameCredit.LicenseeCode,
				BrandId: gameCredit.BrandId,
				BrandCode: gameCredit.BrandCode,
				RiskType: gameCredit.CreditRiskType),
			cancellationToken: cancellationToken)
			.ConfigureAwait(false);
	}
}
