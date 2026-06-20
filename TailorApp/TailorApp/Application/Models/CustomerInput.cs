using System.ComponentModel.DataAnnotations;

namespace TailorApp.Application.Models;

/// <summary>
/// Input model for creating/editing a customer. Decouples the UI form from
/// the persisted <see cref="Domain.Entities.Customer"/> entity, and carries
/// the validation rules so the same annotations drive both the Blazor
/// EditForm and the service-side <see cref="Common.ModelValidator"/>.
/// </summary>
public sealed class CustomerInput
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(120, ErrorMessage = "Name must be 120 characters or fewer.")]
    public string Name { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Phone number is not valid.")]
    [StringLength(30, ErrorMessage = "Phone number must be 30 characters or fewer.")]
    public string? PhoneNumber { get; set; }

    [StringLength(250, ErrorMessage = "Address must be 250 characters or fewer.")]
    public string? Address { get; set; }

    [StringLength(500, ErrorMessage = "Notes must be 500 characters or fewer.")]
    public string? Notes { get; set; }
}
