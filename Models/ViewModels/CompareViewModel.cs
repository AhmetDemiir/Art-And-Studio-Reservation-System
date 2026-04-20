namespace Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

public class CompareViewModel
{
    public List<Artwork> Artworks { get; set; } = new();
    public List<WorkshopEvent> Workshops { get; set; } = new();
}
