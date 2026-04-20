using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

public class ArtworkController : Controller
{
    private readonly ApplicationDbContext _context;

    public ArtworkController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var artworks = await _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.ArtworkCategory)
            .Include(a => a.Images)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return View(artworks);
    }

    public async Task<IActionResult> Details(int id, string sort = "newest")
    {
        var artwork = await _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.ArtworkCategory)
            .Include(a => a.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(a => a.ArtworkId == id);

        if (artwork is null)
        {
            return NotFound();
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.IsFavorite = await _context.ArtworkFavorites
                .AnyAsync(f => f.UserId == userId && f.ArtworkId == id);

            ViewBag.UserHelpfulVotes = new HashSet<int>(await _context.ReviewHelpfulVotes
                .Where(v => v.UserId == userId)
                .Select(v => v.ReviewId)
                .ToListAsync());
        }
        else
        {
            ViewBag.IsFavorite = false;
            ViewBag.UserHelpfulVotes = new HashSet<int>();
        }

        var reviewsQuery = _context.Reviews
            .Where(r => r.ArtworkId == id && r.IsApproved)
            .Include(r => r.User)
            .Include(r => r.Responses)
                .ThenInclude(resp => resp.AdminUser)
            .AsQueryable();

        reviewsQuery = sort switch
        {
            "highest" => reviewsQuery.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            "helpful" => reviewsQuery.OrderByDescending(r => r.HelpfulCount).ThenByDescending(r => r.CreatedAt),
            _ => reviewsQuery.OrderByDescending(r => r.CreatedAt)
        };

        var reviews = await reviewsQuery.ToListAsync();
        ViewBag.Reviews = reviews;
        ViewBag.ReviewSort = sort;
        ViewBag.ReviewAverage = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0;
        ViewBag.ReviewCount = reviews.Count;

        return View(artwork);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int artworkId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var artworkExists = await _context.Artworks.AnyAsync(a => a.ArtworkId == artworkId);
        if (!artworkExists)
        {
            return NotFound();
        }

        var existingFavorite = await _context.ArtworkFavorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ArtworkId == artworkId);

        if (existingFavorite is null)
        {
            _context.ArtworkFavorites.Add(new ArtworkFavorite
            {
                UserId = userId,
                ArtworkId = artworkId,
                CreatedAt = DateTime.UtcNow
            });
            TempData["SuccessMessage"] = "Eser favorilere eklendi.";
        }
        else
        {
            _context.ArtworkFavorites.Remove(existingFavorite);
            TempData["SuccessMessage"] = "Eser favorilerden cikarildi.";
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = artworkId });
    }

    [Authorize]
    public async Task<IActionResult> Favorites()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var favorites = await _context.ArtworkFavorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Artwork)
                .ThenInclude(a => a.Artist)
            .Include(f => f.Artwork)
                .ThenInclude(a => a.ArtworkCategory)
            .Include(f => f.Artwork)
                .ThenInclude(a => a.Images)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return View(favorites);
    }
}
