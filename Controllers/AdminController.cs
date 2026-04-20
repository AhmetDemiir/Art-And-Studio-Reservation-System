using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;
using Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var topFavorited = await _context.Artworks
            .Select(a => new ArtworkStatItemViewModel
            {
                ArtworkId = a.ArtworkId,
                Title = a.Title,
                ArtistName = a.Artist.FullName,
                Count = a.Favorites.Count
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Title)
            .Take(5)
            .ToListAsync();

        var topReviewed = await _context.Artworks
            .Select(a => new ArtworkStatItemViewModel
            {
                ArtworkId = a.ArtworkId,
                Title = a.Title,
                ArtistName = a.Artist.FullName,
                Count = a.Reviews.Count(r => r.IsApproved)
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Title)
            .Take(5)
            .ToListAsync();

        var workshopStats = await _context.WorkshopEvents
            .Select(w => new WorkshopStatItemViewModel
            {
                WorkshopEventId = w.WorkshopEventId,
                Title = w.Title,
                TotalCapacity = w.Schedules.Sum(s => s.Capacity),
                ReservedCount = w.Schedules
                    .SelectMany(s => s.Reservations)
                    .Where(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
                    .Sum(r => r.ParticipantCount),
                AverageRating = w.Reviews.Where(r => r.IsApproved).Select(r => (double?)r.Rating).Average() ?? 0,
                TotalReservations = w.Schedules.SelectMany(s => s.Reservations).Count()
            })
            .OrderByDescending(x => x.TotalReservations)
            .ToListAsync();

        foreach (var stat in workshopStats)
        {
            stat.OccupancyRate = stat.TotalCapacity > 0
                ? Math.Round((double)stat.ReservedCount / stat.TotalCapacity * 100, 1)
                : 0;
        }

        var recentOrders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Artwork)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .SelectMany(o => o.OrderItems.Select(oi => new AdminOrderItemViewModel
            {
                OrderNumber = o.OrderNumber,
                BuyerName = o.User.FirstName + " " + o.User.LastName,
                ArtworkTitle = oi.Artwork.Title,
                TotalAmount = oi.TotalPrice,
                CreatedAt = o.CreatedAt
            }))
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            TopFavoritedArtworks = topFavorited,
            TopReviewedArtworks = topReviewed,
            WorkshopStats = workshopStats,
            RecentOrders = recentOrders
        };

        return View(model);
    }

    public async Task<IActionResult> Orders()
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Artwork)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }
}
