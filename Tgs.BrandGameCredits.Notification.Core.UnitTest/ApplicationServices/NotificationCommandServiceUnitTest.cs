using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common;
using Tgs.BrandGameCredits.Common.Models;
using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.BrandGameCredits.Notification.Core.UnitTest.ApplicationServices;

public class NotificationCommandServiceUnitTest
{
	[Fact]
	public async Task NormalRisk()
	{
		var fakeLogger = NullLogger<NotificationCommandService>.Instance;
		var fakeNotificationLockingRepository = Substitute.For<INotificationLockingRepository>();
		var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();
		var fakeContactsService = Substitute.For<ICommandService<ContactsCommand>>();

		var command = new NotificationCommand(
			ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
			ResellerCode: "TGS",
			LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
			LicenseeCode: "TGS",
			BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
			BrandCode: "GPI");

		_ = fakeNotificationLockingRepository.LockedAsync(
			code: Arg.Is(command.BrandCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(true);

		var gameCredit = new BrandGameCredit(
			ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
			ResellerCode: "TGS",
			LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
			LicenseeCode: "TGS",
			BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
			BrandCode: "GPI",
			Month: DateOnly.Parse("2023-12-01"),
			InitialAmount: 2m,
			CreditAmount: 100m,
			DebitAmount: 1m,
			ProfitAmount: -33m,
			CreditLimit: 101m,
			CreditUsed: 33m,
			CreditUsedPer: 32.6733m,
			CreditAvailable: 68m,
			CreditAvailablePer: 67.3267m,
			BrandGameCreditRiskTypes.NormalRisk,
			CreatedOn: DateTimeOffset.Parse("2023-12-06T09:19:50.010516+00:00"),
			UpdatedOn: DateTimeOffset.Parse("2023-12-07T10:38:10.350982+00:00"));
		_ = fakeBrandGameCreditsClient.GameCreditGetCurrentAsync(
			brandCode: Arg.Is(command.BrandCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(gameCredit);

		var service = new NotificationCommandService(
			fakeLogger,
			fakeNotificationLockingRepository,
			fakeBrandGameCreditsClient,
			fakeContactsService);

		await service.ExecuteAsync(command);

		var contactsCommand = new ContactsCommand(
			ResellerId: gameCredit.ResellerId,
			ResellerCode: gameCredit.ResellerCode,
			LicenseeId: gameCredit.LicenseeId,
			LicenseeCode: gameCredit.LicenseeCode,
			BrandId: gameCredit.BrandId,
			BrandCode: gameCredit.BrandCode,
			RiskType: gameCredit.CreditRiskType);

		_ = fakeContactsService
			.Received(0)
			.ExecuteAsync(
			command: Arg.Is(contactsCommand),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task LowRisk()
	{
		var fakeLogger = NullLogger<NotificationCommandService>.Instance;
		var fakeNotificationLockingRepository = Substitute.For<INotificationLockingRepository>();
		var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();
		var fakeContactsService = Substitute.For<ICommandService<ContactsCommand>>();

		var command = new NotificationCommand(
			ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
			ResellerCode: "TGS",
			LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
			LicenseeCode: "TGS",
			BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
			BrandCode: "GPI");

		_ = fakeNotificationLockingRepository.LockedAsync(
			code: Arg.Is(command.BrandCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(true);

		var gameCredit = new BrandGameCredit(
			ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
			ResellerCode: "TGS",
			LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
			LicenseeCode: "TGS",
			BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
			BrandCode: "GPI",
			Month: DateOnly.Parse("2023-12-01"),
			InitialAmount: 0m,
			CreditAmount: 1000m,
			DebitAmount: 0m,
			ProfitAmount: -600m,
			CreditLimit: 1000m,
			CreditUsed: 600m,
			CreditUsedPer: 60m,
			CreditAvailable: 400m,
			CreditAvailablePer: 40m,
			BrandGameCreditRiskTypes.LowRisk,
			CreatedOn: DateTimeOffset.Parse("2023-12-06T09:19:50.010516+00:00"),
			UpdatedOn: DateTimeOffset.Parse("2023-12-07T10:38:10.350982+00:00"));
		_ = fakeBrandGameCreditsClient.GameCreditGetCurrentAsync(
			brandCode: Arg.Is(command.BrandCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(gameCredit);

		var service = new NotificationCommandService(
			fakeLogger,
			fakeNotificationLockingRepository,
			fakeBrandGameCreditsClient,
			fakeContactsService);

		await service.ExecuteAsync(command);

		var contactsCommand = new ContactsCommand(
			ResellerId: gameCredit.ResellerId,
			ResellerCode: gameCredit.ResellerCode,
			LicenseeId: gameCredit.LicenseeId,
			LicenseeCode: gameCredit.LicenseeCode,
			BrandId: gameCredit.BrandId,
			BrandCode: gameCredit.BrandCode,
			RiskType: gameCredit.CreditRiskType);

		_ = fakeContactsService
			.Received(1)
			.ExecuteAsync(
			command: Arg.Is(contactsCommand),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task ModerateRisk()
	{
		var fakeLogger = NullLogger<NotificationCommandService>.Instance;
		var fakeNotificationLockingRepository = Substitute.For<INotificationLockingRepository>();
		var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();
		var fakeContactsService = Substitute.For<ICommandService<ContactsCommand>>();

		var command = new NotificationCommand(
			ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
			ResellerCode: "TGS",
			LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
			LicenseeCode: "TGS",
			BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
			BrandCode: "GPI");

		_ = fakeNotificationLockingRepository.LockedAsync(
			code: Arg.Is(command.BrandCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(true);

		var gameCredit = new BrandGameCredit(
			ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
			ResellerCode: "TGS",
			LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
			LicenseeCode: "TGS",
			BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
			BrandCode: "GPI",
			Month: DateOnly.Parse("2023-12-01"),
			InitialAmount: 0m,
			CreditAmount: 1000m,
			DebitAmount: 0m,
			ProfitAmount: -800m,
			CreditLimit: 1000m,
			CreditUsed: 800m,
			CreditUsedPer: 80m,
			CreditAvailable: 200m,
			CreditAvailablePer: 20m,
			BrandGameCreditRiskTypes.ModerateRisk,
			CreatedOn: DateTimeOffset.Parse("2023-12-06T09:19:50.010516+00:00"),
			UpdatedOn: DateTimeOffset.Parse("2023-12-07T10:38:10.350982+00:00"));
		_ = fakeBrandGameCreditsClient.GameCreditGetCurrentAsync(
			brandCode: Arg.Is(command.BrandCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(gameCredit);

		var service = new NotificationCommandService(
			fakeLogger,
			fakeNotificationLockingRepository,
			fakeBrandGameCreditsClient,
			fakeContactsService);

		await service.ExecuteAsync(command);

		var contactsCommand = new ContactsCommand(
			ResellerId: gameCredit.ResellerId,
			ResellerCode: gameCredit.ResellerCode,
			LicenseeId: gameCredit.LicenseeId,
			LicenseeCode: gameCredit.LicenseeCode,
			BrandId: gameCredit.BrandId,
			BrandCode: gameCredit.BrandCode,
			RiskType: gameCredit.CreditRiskType);

		_ = fakeContactsService
			.Received(1)
			.ExecuteAsync(
			command: Arg.Is(contactsCommand),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task HighRisk()
	{
		var fakeLogger = NullLogger<NotificationCommandService>.Instance;
		var fakeNotificationLockingRepository = Substitute.For<INotificationLockingRepository>();
		var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();
		var fakeContactsService = Substitute.For<ICommandService<ContactsCommand>>();

		var command = new NotificationCommand(
			ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
			ResellerCode: "TGS",
			LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
			LicenseeCode: "TGS",
			BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
			BrandCode: "GPI");

		_ = fakeNotificationLockingRepository.LockedAsync(
			code: Arg.Is(command.BrandCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(true);

		var gameCredit = new BrandGameCredit(
			ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
			ResellerCode: "TGS",
			LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
			LicenseeCode: "TGS",
			BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
			BrandCode: "GPI",
			Month: DateOnly.Parse("2023-12-01"),
			InitialAmount: 0m,
			CreditAmount: 1000m,
			DebitAmount: 0m,
			ProfitAmount: -950m,
			CreditLimit: 1000m,
			CreditUsed: 950m,
			CreditUsedPer: 95m,
			CreditAvailable: 50m,
			CreditAvailablePer: 5m,
			BrandGameCreditRiskTypes.HighRisk,
			CreatedOn: DateTimeOffset.Parse("2023-12-06T09:19:50.010516+00:00"),
			UpdatedOn: DateTimeOffset.Parse("2023-12-07T10:38:10.350982+00:00"));
		_ = fakeBrandGameCreditsClient.GameCreditGetCurrentAsync(
			brandCode: Arg.Is(command.BrandCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(gameCredit);

		var service = new NotificationCommandService(
			fakeLogger,
			fakeNotificationLockingRepository,
			fakeBrandGameCreditsClient,
			fakeContactsService);

		await service.ExecuteAsync(command);

		var contactsCommand = new ContactsCommand(
			ResellerId: gameCredit.ResellerId,
			ResellerCode: gameCredit.ResellerCode,
			LicenseeId: gameCredit.LicenseeId,
			LicenseeCode: gameCredit.LicenseeCode,
			BrandId: gameCredit.BrandId,
			BrandCode: gameCredit.BrandCode,
			RiskType: gameCredit.CreditRiskType);

		_ = fakeContactsService
			.Received(1)
			.ExecuteAsync(
			command: Arg.Is(contactsCommand),
			cancellationToken: Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Locked_Return_False()
	{
		var fakeLogger = NullLogger<NotificationCommandService>.Instance;
		var fakeNotificationLockingRepository = Substitute.For<INotificationLockingRepository>();
		var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();
		var fakeContactsService = Substitute.For<ICommandService<ContactsCommand>>();

		var command = new NotificationCommand(
			ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
			ResellerCode: "TGS",
			LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
			LicenseeCode: "TGS",
			BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
			BrandCode: "GPI");

		_ = fakeNotificationLockingRepository.LockedAsync(
			code: Arg.Is(command.BrandCode),
			cancellationToken: Arg.Any<CancellationToken>())
			.Returns(false);

		var service = new NotificationCommandService(
			fakeLogger,
			fakeNotificationLockingRepository,
			fakeBrandGameCreditsClient,
			fakeContactsService);

		await service.ExecuteAsync(command);

		_ = fakeContactsService
			.Received(0)
			.ExecuteAsync(
			command: Arg.Any<ContactsCommand>(),
			cancellationToken: Arg.Any<CancellationToken>());
	}
}
