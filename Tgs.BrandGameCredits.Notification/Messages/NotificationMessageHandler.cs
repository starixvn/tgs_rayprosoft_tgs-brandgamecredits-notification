using Tgs.BrandGameCredits.Handler.Common.Protos;
using Tgs.BrandGameCredits.Notification.Core;
using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

using Valhalla.MessageQueue;

namespace Tgs.BrandGameCredits.Handler.Messages;

public class NotificationMessageHandler(
    ILogger<NotificationMessageHandler> logger,
    INotificationDispatcher dispatcher) : IMessageHandler
{
    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var publish = BrandGameCreditUpdatePublish.Parser.ParseFrom(data.Span)
            ?? throw new NullReferenceException("The request data resolves to a null value");

        logger.LogInformation("LogOn:{logOn} Information publish:{publish}", DateTime.Now.ToString("o"), publish);

        return dispatcher.PushCommandAsync(
            command: new NotificationCommand(
                ResellerId: publish.ResellerId.ToGuid(),
                ResellerCode: publish.ResellerCode,
                LicenseeId: publish.LicenseeId.ToGuid(),
                LicenseeCode: publish.LicenseeCode,
                BrandId: publish.BrandId.ToGuid(),
                BrandCode: publish.BrandCode),
            cancellationToken: cancellationToken);
    }
}
