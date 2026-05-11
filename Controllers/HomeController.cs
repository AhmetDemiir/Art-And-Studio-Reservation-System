using System.Diagnostics;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;
using Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var latestArtworks = await _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.ArtworkCategory)
            .Include(a => a.Images)
            .Include(a => a.Campaign)
            .OrderByDescending(a => a.CreatedAt)
            .Take(6)
            .ToListAsync();

        var workshops = await _context.WorkshopEvents
            .Include(w => w.WorkshopCategory)
            .Include(w => w.Campaign)
            .Include(w => w.Schedules)
            .Where(w => w.IsActive)
            .OrderBy(w => w.Title)
            .Take(6)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var couponsQuery = _context.Coupons
            .Where(c => c.IsActive && c.ValidFrom <= now && c.ValidTo >= now);
        if (userId is null)
        {
            couponsQuery = couponsQuery.Where(c => c.RestrictedUserId == null);
        }
        else
        {
            couponsQuery = couponsQuery.Where(c => c.RestrictedUserId == null || c.RestrictedUserId == userId);
        }

        var activeCoupons = await couponsQuery
            .OrderBy(c => c.ValidTo)
            .Take(6)
            .ToListAsync();

        var activeCampaigns = await _context.Campaigns
            .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
            .OrderBy(c => c.Title)
            .ToListAsync();

        var campaignIds = activeCampaigns.Select(c => c.CampaignId).ToList();

        var campaignArtworks = campaignIds.Count == 0
            ? new List<Artwork>()
            : await _context.Artworks
                .Include(a => a.Artist)
                .Include(a => a.ArtworkCategory)
                .Include(a => a.Images)
                .Include(a => a.Campaign)
                .Where(a => a.CampaignId != null && campaignIds.Contains(a.CampaignId.Value))
                .OrderByDescending(a => a.CreatedAt)
                .Take(6)
                .ToListAsync();

        var campaignWorkshopEvents = campaignIds.Count == 0
            ? new List<WorkshopEvent>()
            : await _context.WorkshopEvents
                .Include(w => w.WorkshopCategory)
                .Include(w => w.Schedules)
                .Include(w => w.Campaign)
                .Where(w => w.IsActive && w.CampaignId != null && campaignIds.Contains(w.CampaignId.Value))
                .OrderBy(w => w.Title)
                .Take(6)
                .ToListAsync();

        var campaignWorkshops = campaignWorkshopEvents
            .Select(w => new WorkshopHighlightViewModel
            {
                Workshop = w,
                NextSchedule = w.Schedules
                    .Where(s => !s.IsCancelled && s.StartDateTime >= DateTime.UtcNow)
                    .OrderBy(s => s.StartDateTime)
                    .FirstOrDefault()
            })
            .OrderBy(x => x.NextSchedule?.StartDateTime ?? DateTime.MaxValue)
            .ToList();

        var model = new HomeIndexViewModel
        {
            LatestArtworks = latestArtworks,
            UpcomingWorkshops = workshops
                .Select(w => new WorkshopHighlightViewModel
                {
                    Workshop = w,
                    NextSchedule = w.Schedules
                        .Where(s => !s.IsCancelled && s.StartDateTime >= DateTime.UtcNow)
                        .OrderBy(s => s.StartDateTime)
                        .FirstOrDefault()
                })
                .OrderBy(x => x.NextSchedule?.StartDateTime ?? DateTime.MaxValue)
                .ToList(),
            ActiveCoupons = activeCoupons,
            ActiveCampaigns = activeCampaigns,
            CampaignArtworks = campaignArtworks,
            CampaignWorkshops = campaignWorkshops
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
