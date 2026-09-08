using ProjectHub.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Application.WorkItems;

public sealed class CreateWorkItemRequest
{
    [Required]
    [Display(Name = "Project")]
    public Guid ProjectId { get; set; }
    [Required]
    [StringLength(300)]
    [Display(Name = "Title")]
    public string Title { get; set; } = null!;
    [StringLength(4000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }
    [Display(Name = "Priority")]
    public WorkItemPriority Priority { get; set; }
    [Display(Name = "Due Date")]
    public DateTime? DueDateUtc { get; set; }
}