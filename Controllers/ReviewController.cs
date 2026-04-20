using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

[Authorize]
public class ReviewController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReviewController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReview(
        int? artworkId,
        int? workshopEventId,
        int rating,
        string comment,
        string? sort = "newest")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(comment))
        {
            TempData["ErrorMessage"] = "Yorum eklenemedi. Puan ve yorum alani zorunludur.";
            return RedirectToTarget(artworkId, workshopEventId, sort);
        }

        var isArtworkReview = artworkId.HasValue && !workshopEventId.HasValue;
        var isWorkshopReview = workshopEventId.HasValue && !artworkId.HasValue;
        if (!isArtworkReview && !isWorkshopReview)
        {
            TempData["ErrorMessage"] = "Yorum hedefi geçersiz.";
            return RedirectToAction("Index", "Home");
        }

        bool verified = false;
        ReviewTargetType targetType;

        if (isArtworkReview)
        {
            var targetArtworkId = artworkId!.Value;
            var artworkExists = await _context.Artworks.AnyAsync(a => a.ArtworkId == targetArtworkId);
            if (!artworkExists)
            {
                return NotFound();
            }

            verified = await _context.OrderItems
                .Include(oi => oi.Order)
                .AnyAsync(oi =>
                    oi.ArtworkId == targetArtworkId &&
                    oi.Order.UserId == userId &&
                    oi.Order.Status != OrderStatus.Cancelled &&
                    oi.Order.Status != OrderStatus.Refunded);
            targetType = ReviewTargetType.Artwork;
        }
        else
        {
            var targetWorkshopId = workshopEventId!.Value;
            var workshopExists = await _context.WorkshopEvents.AnyAsync(w => w.WorkshopEventId == targetWorkshopId);
            if (!workshopExists)
            {
                return NotFound();
            }

            verified = await _context.Reservations
                .Include(r => r.WorkshopSchedule)
                .AnyAsync(r =>
                    r.UserId == userId &&
                    r.WorkshopSchedule.WorkshopEventId == targetWorkshopId &&
                    r.Status != ReservationStatus.Cancelled);
            targetType = ReviewTargetType.Workshop;
        }

        if (!verified)
        {
            TempData["ErrorMessage"] = "Yorum eklemek icin dogrulanmis satin alma veya rezervasyon gerekli.";
            return RedirectToTarget(artworkId, workshopEventId, sort);
        }

        var alreadyReviewed = await _context.Reviews.AnyAsync(r =>
            r.UserId == userId &&
            r.ArtworkId == artworkId &&
            r.WorkshopEventId == workshopEventId);
        if (alreadyReviewed)
        {
            TempData["ErrorMessage"] = "Bu icerik icin daha once yorum yaptiniz.";
            return RedirectToTarget(artworkId, workshopEventId, sort);
        }

        var review = new Review
        {
            UserId = userId,
            TargetType = targetType,
            ArtworkId = artworkId,
            WorkshopEventId = workshopEventId,
            Rating = rating,
            Comment = comment.Trim(),
            IsVerifiedPurchaseOrParticipation = true,
            CreatedAt = DateTime.UtcNow,
            IsApproved = true
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Yorumunuz başarıyla eklendi.";
        return RedirectToTarget(artworkId, workshopEventId, sort);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VoteHelpful(int reviewId, string targetType, int targetId, string? sort = "newest")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        if (review is null)
        {
            return NotFound();
        }

        if (review.UserId == userId)
        {
            TempData["ErrorMessage"] = "Kendi yorumunu faydalı olarak oylayamazsın.";
            return RedirectToTargetByType(targetType, targetId, sort);
        }

        var existingVote = await _context.ReviewHelpfulVotes
            .FirstOrDefaultAsync(v => v.ReviewId == reviewId && v.UserId == userId);

        if (existingVote is not null)
        {
            TempData["ErrorMessage"] = "Bu yorumu zaten oyladin.";
            return RedirectToTargetByType(targetType, targetId, sort);
        }

        _context.ReviewHelpfulVotes.Add(new ReviewHelpfulVote
        {
            ReviewId = reviewId,
            UserId = userId,
            IsHelpful = true,
            CreatedAt = DateTime.UtcNow
        });

        review.HelpfulCount += 1;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Geri bildirimin icin tesekkurler.";
        return RedirectToTargetByType(targetType, targetId, sort);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminReply(int reviewId, string responseText, string targetType, int targetId, string? sort = "newest")
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            return Challenge();
        }

        if (string.IsNullOrWhiteSpace(responseText))
        {
            TempData["ErrorMessage"] = "Yanıt metni boş bırakılamaz.";
            return RedirectToTargetByType(targetType, targetId, sort);
        }

        var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        if (review is null)
        {
            return NotFound();
        }

        _context.ReviewResponses.Add(new ReviewResponse
        {
            ReviewId = reviewId,
            AdminUserId = adminUserId,
            ResponseText = responseText.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Yönetici yanıtı eklendi.";
        return RedirectToTargetByType(targetType, targetId, sort);
    }

    private IActionResult RedirectToTarget(int? artworkId, int? workshopEventId, string? sort)
    {
        if (artworkId.HasValue)
        {
            return RedirectToAction("Details", "Artwork", new { id = artworkId.Value, sort });
        }

        if (workshopEventId.HasValue)
        {
            return RedirectToAction("Details", "Workshop", new { id = workshopEventId.Value, sort });
        }

        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToTargetByType(string targetType, int targetId, string? sort)
    {
        if (string.Equals(targetType, "artwork", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction("Details", "Artwork", new { id = targetId, sort });
        }

        return RedirectToAction("Details", "Workshop", new { id = targetId, sort });
    }
}
