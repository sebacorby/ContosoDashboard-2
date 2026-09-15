using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentShare : IValidatableObject
{
    [Key]
    public int DocumentShareId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int SharedByUserId { get; set; }

    public int? SharedWithUserId { get; set; }

    [MaxLength(100)]
    public string? SharedWithDepartment { get; set; }

    public DateTime SharedDate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;

    [ForeignKey(nameof(SharedByUserId))]
    public virtual User SharedByUser { get; set; } = null!;

    [ForeignKey(nameof(SharedWithUserId))]
    public virtual User? SharedWithUser { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var hasUser = SharedWithUserId.HasValue;
        var hasDepartment = !string.IsNullOrWhiteSpace(SharedWithDepartment);
        if (hasUser == hasDepartment)
        {
            yield return new ValidationResult(
                "Exactly one share target must be provided.",
                new[] { nameof(SharedWithUserId), nameof(SharedWithDepartment) });
        }
    }

}
