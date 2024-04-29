using System.ComponentModel.DataAnnotations;

namespace Tgs.BrandGameCredits.Notification.VieModels;

public record BrnadViewModel
{
	[Required]
	public string BrandCode { get; init; } = default!;
}
