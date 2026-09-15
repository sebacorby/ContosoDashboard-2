using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentTask
{
    [Key]
    public int DocumentTaskId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int TaskId { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public virtual Document Document { get; set; } = null!;

    [ForeignKey(nameof(TaskId))]
    public virtual TaskItem Task { get; set; } = null!;
}

