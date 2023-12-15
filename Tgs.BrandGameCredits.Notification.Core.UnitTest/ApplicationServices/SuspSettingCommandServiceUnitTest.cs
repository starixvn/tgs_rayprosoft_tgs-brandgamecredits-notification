using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.BrandGameCredits.Client;
using Tgs.BrandGameCredits.Common;
using Tgs.BrandGameCredits.Common.Models;
using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;
using Tgs.Notification.Contacts.Client;
using Tgs.Notification.Contacts.Common.Models;
using Tgs.ServiceUtility;

namespace Tgs.BrandGameCredits.Notification.Core.UnitTest.ApplicationServices;

public class SuspSettingCommandServiceUnitTest
{
    private static readonly string[] _Recipients = ["tst@rayprosoft.com"];

    [Fact]
    public async void Normal_Process()
    {
        var fakeLogger = NullLogger<SuspSettingCommandService>.Instance;
        var fakeBrandGameCreditsClient = Substitute.For<IBrandGameCreditsClient>();
        var fakeEmailService = Substitute.For<ICommandService<EmailCommand>>();

        var command = new SuspSettingCommand(
            BrandCode: "GPI",
            RiskType: BrandGameCreditRiskTypes.LowRisk,
            Recipients: _Recipients);

        var suspSetting = new BrandGameCreditSuspSetting(
            ResellerId: Guid.Parse("b5a2c708-ab70-4316-9f4d-5d2ad0e77460"),
            ResellerCode: "TGS",
            LicenseeId: Guid.Parse("66c5fef5-217b-4929-9a08-72c2479a8f4e"),
            LicenseeCode: "TGS",
            BrandId: Guid.Parse("4e86719f-243f-46f6-b010-5c758b7285af"),
            BrandCode: "GPI",
            LowRisk: 60m,
            LowNotification: new BrandGameCreditSuspSettingNotificationMsg
            {
                EmailContent = "敬愛的夥伴您好，\r\n您的平台目前剩餘額度不足，為避免影響會員遊戲體驗，請盡速聯繫貴司對應商務進行加值，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site currently has reached a high balance usage. To maintain the stability and quality of your gaming experience, we kindly recommend reaching out to your designated business contact for a top-up. Thank you.",
                EmailSubject = "[Licensee][Brand] 加值通知 Warning: Balance low"
            },
            ModerateRisk: 85m,
            ModerateNotification: new BrandGameCreditSuspSettingNotificationMsg
            {
                EmailContent = "敬愛的夥伴您好，\r\n您的平台目前剩餘額度嚴重不足，所有遊戲暫時不可進入；為避免影響平台正常運營，請立即聯繫貴司對應商務進行加值，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site currently has reached a higher balance usage, and the access to all games will be temporarily suspended. To maintain the stability of your site, we kindly recommend reaching out to your designated business contact for a top-up immediately. Thank you.",
                EmailSubject = "[Licensee][Brand] 緊急加值通知 Warning: Balance too low"
            },
            HighRisk: 95m,
            HighNotification: new BrandGameCreditSuspSettingNotificationMsg
            {
                EmailContent = "貴司您好，\r\n由於您的平台剩餘額度已低於限制，平台及所有遊戲暫時不可進入；請立即聯繫貴司對應商務進行加值，並由專人協助您恢復平台運行，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site has been temporarily suspended due to a balance usage that has exceeded the limit. To recover the full access of platform and all games, we kindly recommend reaching out to your designated business contact for a top-up immediately. Thank you.",
                EmailSubject = "[Licensee][Brand] 平台已停用，請立即加值 Notice of Suspension"
            },
            ManualNotification: new BrandGameCreditSuspSettingNotificationMsg
            {
                EmailContent = "敬愛的夥伴您好，\r\n您的平台目前剩餘額度不足，為避免影響會員遊戲體驗，請盡速聯繫貴司對應商務進行加值，謝謝您。\r\nDear Partner,\r\nWe wanted to inform you that your site currently has reached a high balance usage. To maintain the stability and quality of your gaming experience, we kindly recommend reaching out to your designated business contact for a top-up. Thank you.",
                EmailSubject = "[Licensee][Brand] 加值通知 Warning: Balance low"
            },
            DateTimeOffset.Now,
            DateTimeOffset.Now);
        _ = fakeBrandGameCreditsClient.GameCreditSuspSettingGetAsync(
            brandCode: Arg.Is(command.BrandCode),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(suspSetting);

        var service = new SuspSettingCommandService(
            fakeLogger,
            fakeBrandGameCreditsClient,
            fakeEmailService);

        await service.ExecuteAsync(command);

        _ = fakeEmailService
            .Received(1)
            .ExecuteAsync(
            command: Arg.Any<EmailCommand>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
