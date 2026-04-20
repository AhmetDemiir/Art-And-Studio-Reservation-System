using System.ComponentModel.DataAnnotations;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

public class CouponCreateViewModel
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsPercentage { get; set; } = true;

    [Range(0.01, 1000000)]
    public decimal DiscountValue { get; set; }

    public decimal? MaxDiscountAmount { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public int? MaxUsageCount { get; set; }

    [Required]
    public DateTime ValidTo { get; set; } = DateTime.UtcNow.AddMonths(1);
}
