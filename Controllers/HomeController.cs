using System.Diagnostics;
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
            .OrderByDescending(a => a.CreatedAt)
            .Take(6)
            .ToListAsync();

        var workshops = await _context.WorkshopEvents
            .Include(w => w.WorkshopCategory)
            .Include(w => w.Schedules)
            .Where(w => w.IsActive)
            .OrderBy(w => w.Title)
            .Take(6)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var activeCoupons = await _context.Coupons
            .Where(c => c.IsActive && c.ValidFrom <= now && c.ValidTo >= now)
            .OrderBy(c => c.ValidTo)
            .Take(3)
            .ToListAsync();

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
            ActiveCoupons = activeCoupons
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
