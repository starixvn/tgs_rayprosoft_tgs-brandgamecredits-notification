using Microsoft.AspNetCore.Mvc;

using Tgs.BrandGameCredits.Notification.Core.ApplicationServices;
using Tgs.BrandGameCredits.Notification.VieModels;
using Tgs.ServiceUtility;

namespace Tgs.BrandGameCredits.Notification.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrnadController : ControllerBase
{
    [HttpPost]
    public ValueTask PostA(
        [FromServices] ICommandService<BrnadManualCommand> service,
        BrnadViewModel source)
        => service.ExecuteAsync(
            command: new BrnadManualCommand(
                source.BrandCode),
            cancellationToken: HttpContext.RequestAborted);
}
