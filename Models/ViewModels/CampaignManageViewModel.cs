using System.ComponentModel.DataAnnotations;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

public class CampaignManageViewModel
{
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0.01, 1000000)]
    public decimal DiscountValue { get; set; } = 10;

    public bool IsPercentage { get; set; } = true;

    [Required]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    [Required]
    public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);

    public bool IsActive { get; set; } = true;
}
