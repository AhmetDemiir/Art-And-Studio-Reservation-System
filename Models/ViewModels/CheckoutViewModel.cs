using System.ComponentModel.DataAnnotations;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

public class CheckoutViewModel
{
    public int ArtworkId { get; set; }
    public string ArtworkTitle { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal UnitPrice { get; set; }
    public int AvailableStock { get; set; }
    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }
    public bool IsCouponApplied { get; set; }
    public DateTime? CouponValidTo { get; set; }

    [Required]
    public PaymentMethod SelectedPaymentMethod { get; set; } = PaymentMethod.CreditCard;
}
