using System.ComponentModel.DataAnnotations;

namespace ProjectHub.Data.Models;

public enum ProjectStatus
{
    Active = 1,
    [Display(Name = "On Hold")]
    OnHold = 2,
    Completed = 3
}