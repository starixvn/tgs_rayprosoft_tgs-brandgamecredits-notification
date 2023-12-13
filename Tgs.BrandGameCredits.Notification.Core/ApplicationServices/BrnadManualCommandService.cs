using Google.Protobuf;
using Microsoft.Extensions.Logging;

using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Handler.Common.Protos;
using Tgs.ServiceUtility;
using Valhalla.MessageQueue;

namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

internal class BrnadManualCommandService(
    ILogger<BrnadManualCommandService> logger,
    IBrandGameCreditsClient client,
    IMessageSender messageSender) : ICommandService<BrnadManualCommand>
{
    public async ValueTask ExecuteAsync(BrnadManualCommand command, CancellationToken cancellationToken = default)
    {
        var gameCredit = await client.GameCreditGetCurrentAsync(
            brandCode: command.BrandCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NullReferenceException($"This brand does not exist.{command.BrandCode}");

        var publish = new BrandGameCreditUpdatePublish
        {
            ResellerId = gameCredit.ResellerId,
            ResellerCode = gameCredit.ResellerCode,
            LicenseeId = gameCredit.LicenseeId,
            LicenseeCode = gameCredit.LicenseeCode,
            BrandId = gameCredit.BrandId,
            BrandCode = gameCredit.BrandCode,
        };

        await messageSender.PublishAsync(
            subject: "brand.game.credits.notification.manual",
            data: publish.ToByteArray(),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        logger.LogInformation("LogOn:{logOn} Subject:{subject} Message:The content of this publish.({publish})",
            DateTime.Now.ToString("u"),
            "brand.game.credits.notification.manual",
            publish);
    }
}
