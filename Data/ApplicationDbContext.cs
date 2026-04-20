using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<ArtworkCategory> ArtworkCategories => Set<ArtworkCategory>();
    public DbSet<Artwork> Artworks => Set<Artwork>();
    public DbSet<ArtworkImage> ArtworkImages => Set<ArtworkImage>();
    public DbSet<ArtworkFavorite> ArtworkFavorites => Set<ArtworkFavorite>();
    public DbSet<ArtworkViewLog> ArtworkViewLogs => Set<ArtworkViewLog>();
    public DbSet<WorkshopCategory> WorkshopCategories => Set<WorkshopCategory>();
    public DbSet<WorkshopEvent> WorkshopEvents => Set<WorkshopEvent>();
    public DbSet<WorkshopSchedule> WorkshopSchedules => Set<WorkshopSchedule>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CouponRedemption> CouponRedemptions => Set<CouponRedemption>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();
    public DbSet<ComparisonSession> ComparisonSessions => Set<ComparisonSession>();
    public DbSet<ArtworkComparisonItem> ArtworkComparisonItems => Set<ArtworkComparisonItem>();
    public DbSet<WorkshopComparisonItem> WorkshopComparisonItems => Set<WorkshopComparisonItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewHelpfulVote> ReviewHelpfulVotes => Set<ReviewHelpfulVote>();
    public DbSet<ReviewResponse> ReviewResponses => Set<ReviewResponse>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ArtworkFavorite>()
            .HasIndex(x => new { x.UserId, x.ArtworkId })
            .IsUnique();

        builder.Entity<ArtworkViewLog>()
            .HasIndex(x => new { x.ArtworkId, x.ViewedAt });

        builder.Entity<WorkshopSchedule>()
            .HasIndex(x => new { x.WorkshopEventId, x.StartDateTime, x.EndDateTime });

        builder.Entity<Reservation>()
            .HasIndex(x => new { x.UserId, x.WorkshopScheduleId, x.Status });

        builder.Entity<Coupon>()
            .HasIndex(x => x.Code)
            .IsUnique();

        builder.Entity<CouponRedemption>()
            .HasIndex(x => new { x.CouponId, x.UserId, x.OrderId });

        builder.Entity<Order>()
            .HasIndex(x => x.OrderNumber)
            .IsUnique();

        builder.Entity<OrderItem>()
            .HasIndex(x => new { x.OrderId, x.ArtworkId });

        builder.Entity<SupportTicket>()
            .HasIndex(x => new { x.UserId, x.Status, x.CreatedAt });

        builder.Entity<ArtworkComparisonItem>()
            .HasIndex(x => new { x.ComparisonSessionId, x.ArtworkId })
            .IsUnique();

        builder.Entity<WorkshopComparisonItem>()
            .HasIndex(x => new { x.ComparisonSessionId, x.WorkshopEventId })
            .IsUnique();

        builder.Entity<Review>()
            .HasIndex(x => new { x.UserId, x.TargetType, x.ArtworkId, x.WorkshopEventId });

        builder.Entity<ReviewHelpfulVote>()
            .HasIndex(x => new { x.ReviewId, x.UserId })
            .IsUnique();

        builder.Entity<ReviewHelpfulVote>()
            .HasOne(v => v.Review)
            .WithMany(r => r.HelpfulVotes)
            .HasForeignKey(v => v.ReviewId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Review>()
            .ToTable(t =>
            {
                t.HasCheckConstraint("CK_Review_Target_NotNull", "([ArtworkId] IS NOT NULL AND [WorkshopEventId] IS NULL) OR ([ArtworkId] IS NULL AND [WorkshopEventId] IS NOT NULL)");
                t.HasCheckConstraint("CK_Review_Rating_Between_1_5", "[Rating] >= 1 AND [Rating] <= 5");
            });

        builder.Entity<SupportMessage>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SupportMessages)
            .HasForeignKey(m => m.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ReviewResponse>()
            .HasOne(r => r.AdminUser)
            .WithMany(u => u.ReviewResponses)
            .HasForeignKey(r => r.AdminUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
