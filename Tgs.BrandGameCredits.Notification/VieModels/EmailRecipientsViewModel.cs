using System.ComponentModel.DataAnnotations;

namespace Tgs.BrandGameCredits.Notification.VieModels;

public record EmailRecipientsViewModel
{
	[Required]
	public IEnumerable<string> To { get; init; } = default!;
	[Required]
	public string Subject { get; init; } = default!;
	[Required]
	public string Content { get; init; } = default!;
}
