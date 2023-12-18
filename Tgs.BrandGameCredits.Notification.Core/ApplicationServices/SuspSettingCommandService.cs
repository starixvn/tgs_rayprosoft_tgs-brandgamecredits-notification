using Microsoft.Extensions.Logging;

using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common;
using Tgs.ServiceUtility;

namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

internal class SuspSettingCommandService(
    ILogger<SuspSettingCommandService> logger,
    IBrandGameCreditsClient client,
    ICommandService<EmailCommand> service) : ICommandService<SuspSettingCommand>
{
    public async ValueTask ExecuteAsync(SuspSettingCommand command, CancellationToken cancellationToken = default)
    {
        var suspSetting = await client.GameCreditSuspSettingGetAsync(
            brandCode: command.BrandCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var notification = command.RiskType == BrandGameCreditRiskTypes.LowRisk
        ? suspSetting.LowNotification
            : command.RiskType == BrandGameCreditRiskTypes.ModerateRisk
            ? suspSetting.ModerateNotification
            : suspSetting.HighNotification;

        logger.LogInformation("LogOn:{logOn}  Message:This ({brand}) notification content ({types}).",
            DateTime.Now.ToString("o"),
            command.BrandCode,
            notification);

        await service.ExecuteAsync(
            command: new EmailCommand(
                BrandCode: command.BrandCode,
                RiskType: command.RiskType,
                Recipients: command.Recipients,
                Subject: notification.EmailSubject,
                Content: notification.EmailContent),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
