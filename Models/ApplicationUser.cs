using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(250)]
    public string? ProfileImageUrl { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public ICollection<ArtworkFavorite> FavoriteArtworks { get; set; } = new HashSet<ArtworkFavorite>();
    public ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    public ICollection<ReviewHelpfulVote> ReviewHelpfulVotes { get; set; } = new HashSet<ReviewHelpfulVote>();
    public ICollection<SupportTicket> SupportTickets { get; set; } = new HashSet<SupportTicket>();
    public ICollection<SupportMessage> SupportMessages { get; set; } = new HashSet<SupportMessage>();
    public ICollection<ComparisonSession> ComparisonSessions { get; set; } = new HashSet<ComparisonSession>();
    public ICollection<CouponRedemption> CouponRedemptions { get; set; } = new HashSet<CouponRedemption>();
    public ICollection<ReviewResponse> ReviewResponses { get; set; } = new HashSet<ReviewResponse>();
}
