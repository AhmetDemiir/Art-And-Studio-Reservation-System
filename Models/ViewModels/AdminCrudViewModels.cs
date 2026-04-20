using System.ComponentModel.DataAnnotations;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

public class ArtworkManageViewModel
{
    public int? ArtworkId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(3000)]
    public string? Description { get; set; }

    [Range(0.01, 100000000)]
    public decimal Price { get; set; }

    public bool IsForSale { get; set; } = true;

    [Range(0, 10000)]
    public int StockQuantity { get; set; } = 1;

    [Required]
    public int ArtistId { get; set; }

    [Required]
    public int ArtworkCategoryId { get; set; }

    [Url]
    public string? PrimaryImageUrl { get; set; }

    public string? PrimaryImageAltText { get; set; }
}

public class WorkshopManageViewModel
{
    public int? WorkshopEventId { get; set; }

    [Required]
    [StringLength(180)]
    public string Title { get; set; } = string.Empty;

    [StringLength(3000)]
    public string? Description { get; set; }

    [Url]
    public string? CoverImageUrl { get; set; }

    [StringLength(250)]
    public string? Location { get; set; }

    [Range(0.01, 100000000)]
    public decimal BasePrice { get; set; }

    [Required]
    public int WorkshopCategoryId { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime ScheduleStart { get; set; } = DateTime.UtcNow.AddDays(3);

    [Required]
    public DateTime ScheduleEnd { get; set; } = DateTime.UtcNow.AddDays(3).AddHours(2);

    [Range(1, 1000)]
    public int ScheduleCapacity { get; set; } = 20;

    [Range(0.01, 100000000)]
    public decimal ScheduleFee { get; set; } = 1000;
}
