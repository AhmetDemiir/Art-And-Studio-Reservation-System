using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;
using Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

[Authorize]
public class SupportTicketController : Controller
{
    private readonly ApplicationDbContext _context;

    public SupportTicketController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        return View(new SupportTicketCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupportTicketCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var ticket = new SupportTicket
        {
            UserId = userId,
            Subject = model.Subject.Trim(),
            Description = model.Message.Trim(),
            Status = SupportTicketStatus.Open,
            Priority = SupportTicketPriority.Medium,
            CreatedAt = DateTime.UtcNow
        };
        _context.SupportTickets.Add(ticket);
        await _context.SaveChangesAsync();

        _context.SupportMessages.Add(new SupportMessage
        {
            SupportTicketId = ticket.SupportTicketId,
            SenderUserId = userId,
            Message = model.Message.Trim(),
            IsFromAdmin = false,
            SentAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Destek talebiniz oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = ticket.SupportTicketId });
    }

    public async Task<IActionResult> MyTickets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var tickets = await _context.SupportTickets
            .Where(t => t.UserId == userId)
            .Include(t => t.Messages)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tickets);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var ticket = await _context.SupportTickets
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.SupportTicketId == id);

        if (ticket is null)
        {
            return NotFound();
        }

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && ticket.UserId != userId)
        {
            return Forbid();
        }

        var messages = await _context.SupportMessages
            .Where(m => m.SupportTicketId == id)
            .Include(m => m.Sender)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        var vm = new SupportTicketDetailsViewModel
        {
            Ticket = ticket,
            Messages = messages,
            NewMessage = new SupportMessageCreateViewModel { SupportTicketId = id }
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMessage(SupportMessageCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Mesaj boş bırakılamaz.";
            return RedirectToAction(nameof(Details), new { id = model.SupportTicketId });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var ticket = await _context.SupportTickets
            .FirstOrDefaultAsync(t => t.SupportTicketId == model.SupportTicketId);

        if (ticket is null)
        {
            return NotFound();
        }

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && ticket.UserId != userId)
        {
            return Forbid();
        }

        _context.SupportMessages.Add(new SupportMessage
        {
            SupportTicketId = ticket.SupportTicketId,
            SenderUserId = userId,
            Message = model.Message.Trim(),
            IsFromAdmin = isAdmin,
            SentAt = DateTime.UtcNow
        });

        ticket.Status = isAdmin ? SupportTicketStatus.Resolved : SupportTicketStatus.WaitingForCustomer;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Mesajınız gönderildi.";
        return RedirectToAction(nameof(Details), new { id = ticket.SupportTicketId });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminTickets()
    {
        var tickets = await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.Messages)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tickets);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int ticketId, SupportTicketStatus status)
    {
        var ticket = await _context.SupportTickets.FirstOrDefaultAsync(t => t.SupportTicketId == ticketId);
        if (ticket is null)
        {
            return NotFound();
        }

        ticket.Status = status;
        ticket.ClosedAt = status == SupportTicketStatus.Closed ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Talep durumu güncellendi.";
        return RedirectToAction(nameof(AdminTickets));
    }
}
