using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Tgs.Email.Client;
using Tgs.ServiceUtility;

namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

internal class EmailCommandService(
    ILogger<EmailCommandService> logger,
    IOptions<EmailSettingsOptions> emailSettingsOptions,
    IEmailLockingRepository lockingRepository,
    INotificationEmailClient emailClient) : ICommandService<EmailCommand>
{
    private readonly EmailSettingsOptions _emailSettingsOptions = emailSettingsOptions.Value;

    public async ValueTask ExecuteAsync(EmailCommand command, CancellationToken cancellationToken = default)
    {
        if (await lockingRepository.LockedAsync(
            code: command.BrandCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false))
        {
            try
            {
                await emailClient.PublishTextAsync(
                    recipients: command.Recipients.ToArray(),
                    subject: command.Subject,
                    content: command.Content,
                    carbonCopies: Enumerable.Empty<string>(),
                    blindCarbonCopies: _emailSettingsOptions.BlindCarbonCopies,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "LogOn:{logOn}  Message:This {code} email failed to be sent.",
                    DateTime.Now.ToString("o"),
                    command.BrandCode);

                await lockingRepository.UnlockAsync(
                    code: command.BrandCode,
                    cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
