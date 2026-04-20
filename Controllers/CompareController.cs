using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;
using Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

[Authorize]
public class CompareController : Controller
{
    private readonly ApplicationDbContext _context;

    public CompareController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var artworkSession = await GetOrCreateSessionAsync(userId, ComparisonType.Artwork);
        var workshopSession = await GetOrCreateSessionAsync(userId, ComparisonType.Workshop);

        var artworkIds = await _context.ArtworkComparisonItems
            .Where(i => i.ComparisonSessionId == artworkSession.ComparisonSessionId)
            .Select(i => i.ArtworkId)
            .ToListAsync();

        var workshopIds = await _context.WorkshopComparisonItems
            .Where(i => i.ComparisonSessionId == workshopSession.ComparisonSessionId)
            .Select(i => i.WorkshopEventId)
            .ToListAsync();

        var artworks = await _context.Artworks
            .Where(a => artworkIds.Contains(a.ArtworkId))
            .Include(a => a.Artist)
            .Include(a => a.ArtworkCategory)
            .Include(a => a.Images)
            .ToListAsync();

        var workshops = await _context.WorkshopEvents
            .Where(w => workshopIds.Contains(w.WorkshopEventId))
            .Include(w => w.WorkshopCategory)
            .Include(w => w.Schedules)
            .ToListAsync();

        var model = new CompareViewModel
        {
            Artworks = artworks,
            Workshops = workshops
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddArtwork(int artworkId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var exists = await _context.Artworks.AnyAsync(a => a.ArtworkId == artworkId);
        if (!exists)
        {
            return NotFound();
        }

        var session = await GetOrCreateSessionAsync(userId, ComparisonType.Artwork);
        var hasItem = await _context.ArtworkComparisonItems
            .AnyAsync(i => i.ComparisonSessionId == session.ComparisonSessionId && i.ArtworkId == artworkId);
        if (!hasItem)
        {
            _context.ArtworkComparisonItems.Add(new ArtworkComparisonItem
            {
                ComparisonSessionId = session.ComparisonSessionId,
                ArtworkId = artworkId
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Eser karsilastirmaya eklendi.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWorkshop(int workshopEventId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var exists = await _context.WorkshopEvents.AnyAsync(w => w.WorkshopEventId == workshopEventId);
        if (!exists)
        {
            return NotFound();
        }

        var session = await GetOrCreateSessionAsync(userId, ComparisonType.Workshop);
        var hasItem = await _context.WorkshopComparisonItems
            .AnyAsync(i => i.ComparisonSessionId == session.ComparisonSessionId && i.WorkshopEventId == workshopEventId);
        if (!hasItem)
        {
            _context.WorkshopComparisonItems.Add(new WorkshopComparisonItem
            {
                ComparisonSessionId = session.ComparisonSessionId,
                WorkshopEventId = workshopEventId
            });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Etkinlik karsilastirmaya eklendi.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveArtwork(int artworkId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var session = await GetOrCreateSessionAsync(userId, ComparisonType.Artwork);
        var item = await _context.ArtworkComparisonItems
            .FirstOrDefaultAsync(i => i.ComparisonSessionId == session.ComparisonSessionId && i.ArtworkId == artworkId);
        if (item is not null)
        {
            _context.ArtworkComparisonItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Eser karsilastirmadan cikarildi.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveWorkshop(int workshopEventId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var session = await GetOrCreateSessionAsync(userId, ComparisonType.Workshop);
        var item = await _context.WorkshopComparisonItems
            .FirstOrDefaultAsync(i => i.ComparisonSessionId == session.ComparisonSessionId && i.WorkshopEventId == workshopEventId);
        if (item is not null)
        {
            _context.WorkshopComparisonItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Etkinlik karsilastirmadan cikarildi.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<ComparisonSession> GetOrCreateSessionAsync(string userId, ComparisonType type)
    {
        var session = await _context.ComparisonSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.ComparisonType == type && !s.IsSaved);

        if (session is not null)
        {
            return session;
        }

        session = new ComparisonSession
        {
            UserId = userId,
            ComparisonType = type,
            Title = type == ComparisonType.Artwork ? "Eser Karşılaştırma" : "Atölye Karşılaştırma",
            IsSaved = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.ComparisonSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }
}
