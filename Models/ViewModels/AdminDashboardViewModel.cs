namespace Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

public class AdminDashboardViewModel
{
    public List<ArtworkStatItemViewModel> TopFavoritedArtworks { get; set; } = new();
    public List<ArtworkStatItemViewModel> TopReviewedArtworks { get; set; } = new();
    public List<WorkshopStatItemViewModel> WorkshopStats { get; set; } = new();
    public List<AdminOrderItemViewModel> RecentOrders { get; set; } = new();
}

public class ArtworkStatItemViewModel
{
    public int ArtworkId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class WorkshopStatItemViewModel
{
    public int WorkshopEventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int ReservedCount { get; set; }
    public double OccupancyRate { get; set; }
    public double AverageRating { get; set; }
    public int TotalReservations { get; set; }
}

public class AdminOrderItemViewModel
{
    public string OrderNumber { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string ArtworkTitle { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
