using System.ComponentModel.DataAnnotations;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

public class SupportTicketCreateViewModel
{
    [Required]
    [StringLength(180)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [StringLength(3000)]
    public string Message { get; set; } = string.Empty;
}

public class SupportTicketDetailsViewModel
{
    public SupportTicket Ticket { get; set; } = null!;
    public List<SupportMessage> Messages { get; set; } = new();
    public SupportMessageCreateViewModel NewMessage { get; set; } = new();
}

public class SupportMessageCreateViewModel
{
    public int SupportTicketId { get; set; }

    [Required]
    [StringLength(3000)]
    public string Message { get; set; } = string.Empty;
}
