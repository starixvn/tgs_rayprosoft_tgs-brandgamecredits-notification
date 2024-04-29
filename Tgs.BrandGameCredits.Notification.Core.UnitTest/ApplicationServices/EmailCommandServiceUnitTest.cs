using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.BrandGameCredits.Common;
using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;
using Tgs.Email.Client;

namespace Tgs.BrandGameCredits.Notification.Core.UnitTest.ApplicationServices;

public class EmailCommandServiceUnitTest
{
	private static readonly string[] _BlindCarbonCopies = ["hank.wu@rayprosoft.com"];
	private static readonly string[] _Recipients = ["tst@rayprosoft.com"];

	[Fact]
	public async Task Locked_Return_True()
	{
		var fakeLogger = NullLogger<EmailCommandService>.Instance;
		var fakeOptions = Options.Create<EmailSettingsOptions>(new EmailSettingsOptions
		{
			NotificationLockingSecs = 30,
			EmailLockingHours = 3,
			BlindCarbonCopies = _BlindCarbonCopies
		});
		var fakeEmailLockingRepository = Substitute.For<IEmailLockingRepository>();
		var fakeNotificationEmailClient = Substitute.For<INotificationEmailClient>();

		var command = new EmailCommand(
			BrandCode: "GPI",
			RiskType: BrandGameCreditRiskTypes.LowRisk,
			Recipients: _Recipients,
			Subject: "TestSubject",
			Content: "TestContent");

		_ = fakeEmailLockingRepository.LockedAsync(
			code: Arg.Is(command.BrandCode),
			riskType: Arg.Is(command.RiskType),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(true);

		var service = new EmailCommandService(
			fakeLogger,
			fakeOptions,
			fakeEmailLockingRepository,
			fakeNotificationEmailClient);

		await service.ExecuteAsync(command);

		_ = fakeNotificationEmailClient
			.Received(1)
			.PublishTextAsync(
			recipients: Arg.Any<IEnumerable<string>>(),
			subject: Arg.Is(command.Subject),
			content: Arg.Is(command.Content),
			carbonCopies: Arg.Any<IEnumerable<string>>(),
			blindCarbonCopies: Arg.Any<IEnumerable<string>>(),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Locked_Return_False()
	{
		var fakeLogger = NullLogger<EmailCommandService>.Instance;
		var fakeOptions = Options.Create<EmailSettingsOptions>(new EmailSettingsOptions
		{
			NotificationLockingSecs = 30,
			EmailLockingHours = 3,
			BlindCarbonCopies = _BlindCarbonCopies
		});
		var fakeEmailLockingRepository = Substitute.For<IEmailLockingRepository>();
		var fakeNotificationEmailClient = Substitute.For<INotificationEmailClient>();

		var command = new EmailCommand(
			BrandCode: "GPI",
			RiskType: BrandGameCreditRiskTypes.LowRisk,
			Recipients: _Recipients,
			Subject: "TestSubject",
			Content: "TestContent");

		_ = fakeEmailLockingRepository.LockedAsync(
			code: Arg.Is(command.BrandCode),
			riskType: Arg.Is(command.RiskType),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(false);

		var service = new EmailCommandService(
			fakeLogger,
			fakeOptions,
			fakeEmailLockingRepository,
			fakeNotificationEmailClient);

		await service.ExecuteAsync(command);

		_ = fakeNotificationEmailClient
			.Received(0)
			.PublishTextAsync(
			recipients: Arg.Any<IEnumerable<string>>(),
			subject: Arg.Is(command.Subject),
			content: Arg.Is(command.Content),
			carbonCopies: Arg.Any<IEnumerable<string>>(),
			blindCarbonCopies: Arg.Any<IEnumerable<string>>(),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
