using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.BrandGameCredits.Common;
using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;
using Tgs.Notification.Contacts.Client;
using Tgs.Notification.Contacts.Common.Models;

using Tgs.ServiceUtility;

namespace Tgs.BrandGameCredits.Notification.Core.UnitTest.ApplicationServices;

public class ContactsCommandSerivceUnitTest
{
    [Fact]
    public async void Normal_Process()
    {
        var fakeLogger = NullLogger<ContactsCommandSerivce>.Instance;
        var fakeNotificationContactsClient = Substitute.For<INotificationContactsClient>();
        var fakeSuspSettingService = Substitute.For<ICommandService<SuspSettingCommand>>();

        var command = new ContactsCommand(
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
            LicenseeCode: "TGS",
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            BrandCode: "GPI",
            BrandGameCreditRiskTypes.LowRisk);

        var brandContact = new EmailContactGet(
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
            LicenseeCode: "TGS",
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            BrandCode: "GPI",
            Contacts: new EmailContact[]
            {
                new() {
                    Name="tst",
                    Email="tst@rayprosoft.com",
                    Remark="tst"
                }
            });
        _ = fakeNotificationContactsClient.EmailBrandGetAsync(
            code: Arg.Is(command.BrandCode),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(brandContact);

        var licenseeContact = new EmailContactGet(
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
            LicenseeCode: "TGS",
            BrandId: Guid.Empty,
            BrandCode: string.Empty,
            Contacts: new EmailContact[]
            {
                new() {
                    Name="tst",
                    Email="tst@rayprosoft.com",
                    Remark="tst"
                }
            });
        _ = fakeNotificationContactsClient.EmailLicenseeGetAsync(
            code: Arg.Is(command.LicenseeCode),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(licenseeContact);

        var resellerContact = new EmailContactGet(
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Empty,
            LicenseeCode: string.Empty,
            BrandId: Guid.Empty,
            BrandCode: string.Empty,
            Contacts: new EmailContact[]
            {
                new() {
                    Name="tst",
                    Email="tst@rayprosoft.com",
                    Remark="tst"
                }
            });
        _ = fakeNotificationContactsClient.EmailResellerGetAsync(
            code: Arg.Is(command.ResellerCode),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(resellerContact);

        var service = new ContactsCommandSerivce(
            fakeLogger,
            fakeNotificationContactsClient,
            fakeSuspSettingService);

        await service.ExecuteAsync(command);

        _ = fakeSuspSettingService
            .Received(1)
            .ExecuteAsync(
            command: Arg.Any<SuspSettingCommand>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async void No_Contact()
    {
        var fakeLogger = NullLogger<ContactsCommandSerivce>.Instance;
        var fakeNotificationContactsClient = Substitute.For<INotificationContactsClient>();
        var fakeSuspSettingService = Substitute.For<ICommandService<SuspSettingCommand>>();

        var command = new ContactsCommand(
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
            LicenseeCode: "TGS",
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            BrandCode: "GPI",
            BrandGameCreditRiskTypes.LowRisk);

        _ = fakeNotificationContactsClient.EmailBrandGetAsync(
            code: Arg.Is(command.BrandCode),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns((EmailContactGet?)null);

        _ = fakeNotificationContactsClient.EmailLicenseeGetAsync(
            code: Arg.Is(command.LicenseeCode),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns((EmailContactGet?)null);

        _ = fakeNotificationContactsClient.EmailResellerGetAsync(
            code: Arg.Is(command.ResellerCode),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns((EmailContactGet?)null);

        var service = new ContactsCommandSerivce(
            fakeLogger,
            fakeNotificationContactsClient,
            fakeSuspSettingService);

        await service.ExecuteAsync(command);

        _ = fakeSuspSettingService
            .Received(0)
            .ExecuteAsync(
            command: Arg.Any<SuspSettingCommand>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
