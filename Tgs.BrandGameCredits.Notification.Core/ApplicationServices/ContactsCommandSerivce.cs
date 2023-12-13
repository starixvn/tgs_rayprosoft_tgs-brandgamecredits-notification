using Microsoft.Extensions.Logging;
using Tgs.Notification.Contacts.Client;
using Tgs.ServiceUtility;

namespace Tgs.BrandGameCredits.Notification.Core.ApplicationServices;

internal class ContactsCommandSerivce(
    ILogger<ContactsCommandSerivce> logger,
    INotificationContactsClient client,
    ICommandService<SuspSettingCommand> service) : ICommandService<ContactsCommand>
{
    public async ValueTask ExecuteAsync(ContactsCommand command, CancellationToken cancellationToken = default)
    {
        var recipients = new HashSet<string>();

        var brandContact = await client.EmailBrandGetAsync(
            code: command.BrandCode,
            cancellationToken: cancellationToken)
             .ConfigureAwait(false);
        if (brandContact is not null)
        {
            foreach (var contact in brandContact.Contacts)
            {
                if (!recipients.Add(contact.Email))
                {
                    logger.LogInformation("LogOn:{logOn}  Message:This mailbox is added repeatedly.({email})", DateTime.Now.ToString("o"), contact.Email);
                }
            }
        }

        var licenseeContact = await client.EmailLicenseeGetAsync(
            code: command.LicenseeCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (licenseeContact is not null)
        {
            foreach (var contact in licenseeContact.Contacts)
            {
                if (!recipients.Add(contact.Email))
                {
                    logger.LogInformation("LogOn:{logOn}  Message:This mailbox is added repeatedly.({email})", DateTime.Now.ToString("o"), contact.Email);
                }
            }
        }

        var resellerContact = await client.EmailResellerGetAsync(
            code: command.ResellerCode,
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (resellerContact is not null)
        {
            foreach (var contact in resellerContact.Contacts)
            {
                if (!recipients.Add(contact.Email))
                {
                    logger.LogInformation("LogOn:{logOn}  Message:This mailbox is added repeatedly.({email})", DateTime.Now.ToString("o"), contact.Email);
                }
            }
        }

        if (recipients.Count == 0)
        {
            logger.LogWarning("LogOn:{logOn}  Message:This {code} has no contact notifications set up", DateTime.Now.ToString("o"), command.BrandCode);
            return;
        }

        await service.ExecuteAsync(
            command: new SuspSettingCommand(
                BrandCode: command.BrandCode,
                RiskType: command.RiskType,
                Recipients: recipients),
            cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
