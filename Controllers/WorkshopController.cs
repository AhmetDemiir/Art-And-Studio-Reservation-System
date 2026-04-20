using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

public class WorkshopController : Controller
{
    private readonly ApplicationDbContext _context;

    public WorkshopController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var workshops = await _context.WorkshopEvents
            .Include(w => w.WorkshopCategory)
            .Include(w => w.Schedules)
            .Where(w => w.IsActive)
            .OrderBy(w => w.Title)
            .ToListAsync();

        return View(workshops);
    }

    public async Task<IActionResult> Details(int id, string sort = "newest")
    {
        var workshop = await _context.WorkshopEvents
            .Include(w => w.WorkshopCategory)
            .Include(w => w.Schedules)
                .ThenInclude(s => s.Reservations)
            .FirstOrDefaultAsync(w => w.WorkshopEventId == id);

        if (workshop is null)
        {
            return NotFound();
        }

        workshop.Schedules = workshop.Schedules
            .OrderBy(s => s.StartDateTime)
            .ToList();

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.UserHelpfulVotes = new HashSet<int>(await _context.ReviewHelpfulVotes
                .Where(v => v.UserId == userId)
                .Select(v => v.ReviewId)
                .ToListAsync());
        }
        else
        {
            ViewBag.UserHelpfulVotes = new HashSet<int>();
        }

        var reviewsQuery = _context.Reviews
            .Where(r => r.WorkshopEventId == id && r.IsApproved)
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

        return View(workshop);
    }

    [Authorize]
    public async Task<IActionResult> MyReservations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var reservations = await _context.Reservations
            .Where(r => r.UserId == userId)
            .Include(r => r.WorkshopSchedule)
                .ThenInclude(s => s.WorkshopEvent)
                    .ThenInclude(w => w.Schedules)
            .OrderByDescending(r => r.ReservedAt)
            .ToListAsync();

        return View(reservations);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int eventId, int participantCount, int workshopScheduleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        if (participantCount < 1)
        {
            TempData["ErrorMessage"] = "Katılımcı sayısı en az 1 olmalıdır.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        var workshop = await _context.WorkshopEvents.FirstOrDefaultAsync(w => w.WorkshopEventId == eventId && w.IsActive);
        if (workshop is null)
        {
            return NotFound();
        }

        var selectedSchedule = await _context.WorkshopSchedules
            .Include(s => s.Reservations)
            .FirstOrDefaultAsync(s => s.WorkshopScheduleId == workshopScheduleId && s.WorkshopEventId == eventId);
        if (selectedSchedule is null)
        {
            TempData["ErrorMessage"] = "Seçilen seans bulunamadı.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        if (selectedSchedule.IsCancelled || selectedSchedule.StartDateTime <= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Seçilen seans artık rezervasyona uygun değil.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        var reservedCount = selectedSchedule.Reservations
            .Where(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
            .Sum(r => r.ParticipantCount);

        var remainingCapacity = selectedSchedule.Capacity - reservedCount;
        if (participantCount > remainingCapacity)
        {
            TempData["ErrorMessage"] = $"Yetersiz kontenjan. Kalan kontenjan: {remainingCapacity}.";
            return RedirectToAction(nameof(Details), new { id = eventId });
        }

        var reservation = new Reservation
        {
            UserId = userId,
            WorkshopScheduleId = selectedSchedule.WorkshopScheduleId,
            ParticipantCount = participantCount,
            Status = ReservationStatus.Confirmed,
            TotalPrice = selectedSchedule.Fee * participantCount,
            ReservedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Rezervasyonunuz başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = eventId });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateReservationParticipants(int reservationId, int participantCount, int workshopScheduleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var reservation = await _context.Reservations
            .Include(r => r.WorkshopSchedule)
                .ThenInclude(s => s.Reservations)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.UserId == userId);

        if (reservation is null)
        {
            return NotFound();
        }

        if (reservation.WorkshopSchedule.StartDateTime <= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Geçmiş rezervasyonlar güncellenemez.";
            return RedirectToAction(nameof(MyReservations));
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            TempData["ErrorMessage"] = "İptal edilmiş rezervasyon güncellenemez.";
            return RedirectToAction(nameof(MyReservations));
        }

        if (participantCount < 1)
        {
            TempData["ErrorMessage"] = "Katılımcı sayısı en az 1 olmalıdır.";
            return RedirectToAction(nameof(MyReservations));
        }

        var schedules = await _context.WorkshopSchedules
            .Include(s => s.Reservations)
            .Where(s => s.WorkshopEventId == reservation.WorkshopSchedule.WorkshopEventId && !s.IsCancelled)
            .ToListAsync();
        var targetSchedule = schedules.FirstOrDefault(s => s.WorkshopScheduleId == workshopScheduleId);
        if (targetSchedule is null || targetSchedule.StartDateTime <= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Seçilen yeni seans geçerli değil.";
            return RedirectToAction(nameof(MyReservations));
        }

        var currentParticipants = reservation.ParticipantCount;
        var isScheduleChanged = reservation.WorkshopScheduleId != targetSchedule.WorkshopScheduleId;
        var requiredParticipants = participantCount;
        var activeReservationsInTarget = targetSchedule.Reservations
            .Where(r => r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed)
            .Sum(r => r.ParticipantCount);

        if (!isScheduleChanged)
        {
            var usedByOthers = activeReservationsInTarget - currentParticipants;
            var remainingCapacity = targetSchedule.Capacity - usedByOthers;
            if (requiredParticipants > remainingCapacity)
            {
                TempData["ErrorMessage"] = $"Katılımcı sayısı artırılamadı. Kalan kontenjan: {Math.Max(0, remainingCapacity)}.";
                return RedirectToAction(nameof(MyReservations));
            }
        }
        else
        {
            var remainingCapacity = targetSchedule.Capacity - activeReservationsInTarget;
            if (requiredParticipants > remainingCapacity)
            {
                TempData["ErrorMessage"] = $"Seans değiştirilemedi. Seçilen seansta kalan kontenjan: {Math.Max(0, remainingCapacity)}.";
                return RedirectToAction(nameof(MyReservations));
            }
        }

        reservation.WorkshopScheduleId = targetSchedule.WorkshopScheduleId;
        reservation.ParticipantCount = participantCount;
        reservation.TotalPrice = targetSchedule.Fee * participantCount;
        reservation.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var directionMessage = isScheduleChanged
            ? "Rezervasyon seansı ve kişi sayısı güncellendi."
            : participantCount < currentParticipants
                ? "Katılımcı sayısı azaltıldı, kontenjan iade edildi."
                : "Rezervasyon katılımcı sayısı güncellendi.";
        TempData["SuccessMessage"] = directionMessage;
        return RedirectToAction(nameof(MyReservations));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelReservation(int reservationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var reservation = await _context.Reservations
            .Include(r => r.WorkshopSchedule)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId && r.UserId == userId);

        if (reservation is null)
        {
            return NotFound();
        }

        if (reservation.WorkshopSchedule.StartDateTime <= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Geçmiş rezervasyonlar iptal edilemez.";
            return RedirectToAction(nameof(MyReservations));
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            TempData["ErrorMessage"] = "Rezervasyon zaten iptal edilmiş.";
            return RedirectToAction(nameof(MyReservations));
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.LastUpdatedAt = DateTime.UtcNow;

        // Capacity is effectively returned by excluding cancelled reservations from active counts.
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Rezervasyon iptal edildi ve kontenjan iade edildi.";
        return RedirectToAction(nameof(MyReservations));
    }
}
