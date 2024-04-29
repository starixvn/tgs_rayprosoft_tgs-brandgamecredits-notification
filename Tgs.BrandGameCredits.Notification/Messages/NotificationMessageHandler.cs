using Tgs.BrandGameCredits.Handler.Common.Protos;
using Tgs.BrandGameCredits.Notification.Core;
using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

using Valhalla.MessageQueue;

namespace Tgs.BrandGameCredits.Handler.Messages;

public class NotificationMessageHandler(
	ILogger<NotificationMessageHandler> logger,
	INotificationDispatcher dispatcher) : IMessageHandler<BrandGameCreditUpdatePublish>
{
	public ValueTask HandleAsync(string subject, BrandGameCreditUpdatePublish data, IEnumerable<MessageHeaderValue>? headerValues, CancellationToken cancellationToken = default)
	{
		logger.LogInformation("LogOn:{logOn} Subject:{subject} Message:This receiving message data info.({data})",
			DateTime.Now.ToString("o"),
			subject,
			data);

		return dispatcher.PushCommandAsync(
		command: new NotificationCommand(
				ResellerId: data.ResellerId.ToGuid(),
				ResellerCode: data.ResellerCode,
				LicenseeId: data.LicenseeId.ToGuid(),
				LicenseeCode: data.LicenseeCode,
				BrandId: data.BrandId.ToGuid(),
				BrandCode: data.BrandCode),
			cancellationToken: cancellationToken);
	}
}
