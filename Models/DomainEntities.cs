using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Models;

public class Artist
{
    [Key]
    public int ArtistId { get; set; }

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(300)]
    public string? ProfileImageUrl { get; set; }

    [StringLength(2000)]
    public string? Biography { get; set; }

    [StringLength(100)]
    public string? Nationality { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Artwork> Artworks { get; set; } = new HashSet<Artwork>();
}

public class ArtworkCategory
{
    [Key]
    public int ArtworkCategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<Artwork> Artworks { get; set; } = new HashSet<Artwork>();
}

public class Artwork
{
    [Key]
    public int ArtworkId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(3000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    public bool IsForSale { get; set; } = true;

    public int StockQuantity { get; set; } = 1;

    public int ViewCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(Artist))]
    public int ArtistId { get; set; }
    public Artist Artist { get; set; } = null!;

    [ForeignKey(nameof(ArtworkCategory))]
    public int ArtworkCategoryId { get; set; }
    public ArtworkCategory ArtworkCategory { get; set; } = null!;

    public ICollection<ArtworkImage> Images { get; set; } = new HashSet<ArtworkImage>();
    public ICollection<ArtworkFavorite> Favorites { get; set; } = new HashSet<ArtworkFavorite>();
    public ICollection<ArtworkViewLog> ViewLogs { get; set; } = new HashSet<ArtworkViewLog>();
    public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    public ICollection<ArtworkComparisonItem> ComparisonItems { get; set; } = new HashSet<ArtworkComparisonItem>();
}

public class ArtworkImage
{
    [Key]
    public int ArtworkImageId { get; set; }

    [Required]
    [StringLength(400)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(200)]
    public string? AltText { get; set; }

    public bool IsPrimary { get; set; } = false;

    public int DisplayOrder { get; set; } = 0;

    [ForeignKey(nameof(Artwork))]
    public int ArtworkId { get; set; }
    public Artwork Artwork { get; set; } = null!;
}

public class ArtworkFavorite
{
    [Key]
    public int ArtworkFavoriteId { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(Artwork))]
    public int ArtworkId { get; set; }
    public Artwork Artwork { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ArtworkViewLog
{
    [Key]
    public int ArtworkViewLogId { get; set; }

    [ForeignKey(nameof(Artwork))]
    public int ArtworkId { get; set; }
    public Artwork Artwork { get; set; } = null!;

    [ForeignKey(nameof(Viewer))]
    public string? ViewerId { get; set; }
    public ApplicationUser? Viewer { get; set; }

    [StringLength(45)]
    public string? IpAddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
}

public class WorkshopCategory
{
    [Key]
    public int WorkshopCategoryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public ICollection<WorkshopEvent> WorkshopEvents { get; set; } = new HashSet<WorkshopEvent>();
}

public class WorkshopEvent
{
    [Key]
    public int WorkshopEventId { get; set; }

    [Required]
    [StringLength(180)]
    public string Title { get; set; } = string.Empty;

    [StringLength(3000)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? CoverImageUrl { get; set; }

    [StringLength(250)]
    public string? Location { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(WorkshopCategory))]
    public int WorkshopCategoryId { get; set; }
    public WorkshopCategory WorkshopCategory { get; set; } = null!;

    public ICollection<WorkshopSchedule> Schedules { get; set; } = new HashSet<WorkshopSchedule>();
    public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
    public ICollection<WorkshopComparisonItem> ComparisonItems { get; set; } = new HashSet<WorkshopComparisonItem>();
}

public class WorkshopSchedule
{
    [Key]
    public int WorkshopScheduleId { get; set; }

    [ForeignKey(nameof(WorkshopEvent))]
    public int WorkshopEventId { get; set; }
    public WorkshopEvent WorkshopEvent { get; set; } = null!;

    [DataType(DataType.DateTime)]
    public DateTime StartDateTime { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime EndDateTime { get; set; }

    public int Capacity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Fee { get; set; }

    public bool IsCancelled { get; set; } = false;

    public ICollection<Reservation> Reservations { get; set; } = new HashSet<Reservation>();
}

public class Reservation
{
    [Key]
    public int ReservationId { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(WorkshopSchedule))]
    public int WorkshopScheduleId { get; set; }
    public WorkshopSchedule WorkshopSchedule { get; set; } = null!;

    public int ParticipantCount { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public DateTime ReservedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }
}

public class Coupon
{
    [Key]
    public int CouponId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsPercentage { get; set; } = true;

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxDiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinimumOrderAmount { get; set; }

    public int? MaxUsageCount { get; set; }

    public int CurrentUsageCount { get; set; } = 0;

    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Order> Orders { get; set; } = new HashSet<Order>();
    public ICollection<CouponRedemption> Redemptions { get; set; } = new HashSet<CouponRedemption>();
}

public class Campaign
{
    [Key]
    public int CampaignId { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountValue { get; set; }

    public bool IsPercentage { get; set; } = true;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}

public class CouponRedemption
{
    [Key]
    public int CouponRedemptionId { get; set; }

    [ForeignKey(nameof(Coupon))]
    public int CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;

    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(Order))]
    public int? OrderId { get; set; }
    public Order? Order { get; set; }

    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;
}

public class Order
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(Coupon))]
    public int? CouponId { get; set; }
    public Coupon? Coupon { get; set; }

    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal DiscountAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new HashSet<Payment>();
}

public class OrderItem
{
    [Key]
    public int OrderItemId { get; set; }

    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(Artwork))]
    public int ArtworkId { get; set; }
    public Artwork Artwork { get; set; } = null!;

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }
}

public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    [ForeignKey(nameof(Order))]
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [StringLength(100)]
    public string? TransactionId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SupportTicket
{
    [Key]
    public int SupportTicketId { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    [StringLength(180)]
    public string Subject { get; set; } = string.Empty;

    [StringLength(3000)]
    public string? Description { get; set; }

    public SupportTicketStatus Status { get; set; } = SupportTicketStatus.Open;

    public SupportTicketPriority Priority { get; set; } = SupportTicketPriority.Medium;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ClosedAt { get; set; }

    public ICollection<SupportMessage> Messages { get; set; } = new HashSet<SupportMessage>();
}

public class SupportMessage
{
    [Key]
    public int SupportMessageId { get; set; }

    [ForeignKey(nameof(SupportTicket))]
    public int SupportTicketId { get; set; }
    public SupportTicket SupportTicket { get; set; } = null!;

    [ForeignKey(nameof(Sender))]
    public string SenderUserId { get; set; } = string.Empty;
    public ApplicationUser Sender { get; set; } = null!;

    [Required]
    [StringLength(3000)]
    public string Message { get; set; } = string.Empty;

    public bool IsFromAdmin { get; set; } = false;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public class ComparisonSession
{
    [Key]
    public int ComparisonSessionId { get; set; }

    [ForeignKey(nameof(User))]
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ComparisonType ComparisonType { get; set; }

    [StringLength(150)]
    public string? Title { get; set; }

    [StringLength(100)]
    public string? SavedName { get; set; }

    public bool IsSaved { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ArtworkComparisonItem> ArtworkItems { get; set; } = new HashSet<ArtworkComparisonItem>();
    public ICollection<WorkshopComparisonItem> WorkshopItems { get; set; } = new HashSet<WorkshopComparisonItem>();
}

public class ArtworkComparisonItem
{
    [Key]
    public int ArtworkComparisonItemId { get; set; }

    [ForeignKey(nameof(ComparisonSession))]
    public int ComparisonSessionId { get; set; }
    public ComparisonSession ComparisonSession { get; set; } = null!;

    [ForeignKey(nameof(Artwork))]
    public int ArtworkId { get; set; }
    public Artwork Artwork { get; set; } = null!;
}

public class WorkshopComparisonItem
{
    [Key]
    public int WorkshopComparisonItemId { get; set; }

    [ForeignKey(nameof(ComparisonSession))]
    public int ComparisonSessionId { get; set; }
    public ComparisonSession ComparisonSession { get; set; } = null!;

    [ForeignKey(nameof(WorkshopEvent))]
    public int WorkshopEventId { get; set; }
    public WorkshopEvent WorkshopEvent { get; set; } = null!;
}

public class Review
{
    [Key]
    public int ReviewId { get; set; }

    [Required]
    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public ReviewTargetType TargetType { get; set; }

    [ForeignKey(nameof(Artwork))]
    public int? ArtworkId { get; set; }
    public Artwork? Artwork { get; set; }

    [ForeignKey(nameof(WorkshopEvent))]
    public int? WorkshopEventId { get; set; }
    public WorkshopEvent? WorkshopEvent { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(2500)]
    public string Comment { get; set; } = string.Empty;

    public int HelpfulCount { get; set; } = 0;

    public bool IsVerifiedPurchaseOrParticipation { get; set; } = false;

    public bool IsApproved { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ReviewHelpfulVote> HelpfulVotes { get; set; } = new HashSet<ReviewHelpfulVote>();
    public ICollection<ReviewResponse> Responses { get; set; } = new HashSet<ReviewResponse>();
}

public class ReviewHelpfulVote
{
    [Key]
    public int ReviewHelpfulVoteId { get; set; }

    [ForeignKey(nameof(Review))]
    public int ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public bool IsHelpful { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ReviewResponse
{
    [Key]
    public int ReviewResponseId { get; set; }

    [ForeignKey(nameof(Review))]
    public int ReviewId { get; set; }
    public Review Review { get; set; } = null!;

    [Required]
    [ForeignKey(nameof(AdminUser))]
    public string AdminUserId { get; set; } = string.Empty;
    public ApplicationUser AdminUser { get; set; } = null!;

    [Required]
    [StringLength(2000)]
    public string ResponseText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
