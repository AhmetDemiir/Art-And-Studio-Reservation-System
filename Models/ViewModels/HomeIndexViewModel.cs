namespace Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

public class HomeIndexViewModel
{
    public List<Artwork> LatestArtworks { get; set; } = new();
    public List<WorkshopHighlightViewModel> UpcomingWorkshops { get; set; } = new();
    public List<Coupon> ActiveCoupons { get; set; } = new();
}

public class WorkshopHighlightViewModel
{
    public WorkshopEvent Workshop { get; set; } = null!;
    public WorkshopSchedule? NextSchedule { get; set; }
}
