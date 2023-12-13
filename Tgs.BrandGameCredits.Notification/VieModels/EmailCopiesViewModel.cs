using System.ComponentModel.DataAnnotations;

namespace Tgs.BrandGameCredits.Notification.VieModels;

public record EmailCopiesViewModel
{
    [Required]
    public IEnumerable<string> To { get; init; } = default!;
    [Required]
    public IEnumerable<string> Cc { get; init; } = default!;
    [Required]
    public string Subject { get; init; } = default!;
    [Required]
    public string Content { get; init; } = default!;
}
