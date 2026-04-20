using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;
using Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

[Authorize(Roles = "Admin")]
public class AdminWorkshopController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminWorkshopController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var workshops = await _context.WorkshopEvents
            .Include(w => w.WorkshopCategory)
            .Include(w => w.Schedules)
            .OrderByDescending(w => w.WorkshopEventId)
            .ToListAsync();
        return View(workshops);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSelectsAsync();
        return View(new WorkshopManageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkshopManageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectsAsync();
            return View(model);
        }

        var workshop = new WorkshopEvent
        {
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            CoverImageUrl = model.CoverImageUrl?.Trim(),
            Location = model.Location?.Trim(),
            BasePrice = model.BasePrice,
            WorkshopCategoryId = model.WorkshopCategoryId,
            IsActive = model.IsActive
        };
        _context.WorkshopEvents.Add(workshop);
        await _context.SaveChangesAsync();

        _context.WorkshopSchedules.Add(new WorkshopSchedule
        {
            WorkshopEventId = workshop.WorkshopEventId,
            StartDateTime = model.ScheduleStart.ToUniversalTime(),
            EndDateTime = model.ScheduleEnd.ToUniversalTime(),
            Capacity = model.ScheduleCapacity,
            Fee = model.ScheduleFee
        });
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Etkinlik başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var workshop = await _context.WorkshopEvents
            .Include(w => w.Schedules)
            .FirstOrDefaultAsync(w => w.WorkshopEventId == id);
        if (workshop is null)
        {
            return NotFound();
        }

        var schedule = workshop.Schedules.OrderBy(s => s.StartDateTime).FirstOrDefault();
        var vm = new WorkshopManageViewModel
        {
            WorkshopEventId = workshop.WorkshopEventId,
            Title = workshop.Title,
            Description = workshop.Description,
            CoverImageUrl = workshop.CoverImageUrl,
            Location = workshop.Location,
            BasePrice = workshop.BasePrice,
            WorkshopCategoryId = workshop.WorkshopCategoryId,
            IsActive = workshop.IsActive,
            ScheduleStart = schedule?.StartDateTime.ToLocalTime() ?? DateTime.Now,
            ScheduleEnd = schedule?.EndDateTime.ToLocalTime() ?? DateTime.Now.AddHours(2),
            ScheduleCapacity = schedule?.Capacity ?? 20,
            ScheduleFee = schedule?.Fee ?? workshop.BasePrice
        };

        await LoadSelectsAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(WorkshopManageViewModel model)
    {
        if (!ModelState.IsValid || !model.WorkshopEventId.HasValue)
        {
            await LoadSelectsAsync();
            return View(model);
        }

        var workshop = await _context.WorkshopEvents
            .Include(w => w.Schedules)
            .FirstOrDefaultAsync(w => w.WorkshopEventId == model.WorkshopEventId.Value);
        if (workshop is null)
        {
            return NotFound();
        }

        workshop.Title = model.Title.Trim();
        workshop.Description = model.Description?.Trim();
        workshop.CoverImageUrl = model.CoverImageUrl?.Trim();
        workshop.Location = model.Location?.Trim();
        workshop.BasePrice = model.BasePrice;
        workshop.WorkshopCategoryId = model.WorkshopCategoryId;
        workshop.IsActive = model.IsActive;

        var schedule = workshop.Schedules.OrderBy(s => s.StartDateTime).FirstOrDefault();
        if (schedule is null)
        {
            workshop.Schedules.Add(new WorkshopSchedule
            {
                StartDateTime = model.ScheduleStart.ToUniversalTime(),
                EndDateTime = model.ScheduleEnd.ToUniversalTime(),
                Capacity = model.ScheduleCapacity,
                Fee = model.ScheduleFee
            });
        }
        else
        {
            schedule.StartDateTime = model.ScheduleStart.ToUniversalTime();
            schedule.EndDateTime = model.ScheduleEnd.ToUniversalTime();
            schedule.Capacity = model.ScheduleCapacity;
            schedule.Fee = model.ScheduleFee;
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Etkinlik başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var workshop = await _context.WorkshopEvents.FirstOrDefaultAsync(w => w.WorkshopEventId == id);
        if (workshop is null)
        {
            return NotFound();
        }

        _context.WorkshopEvents.Remove(workshop);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Etkinlik silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectsAsync()
    {
        ViewBag.Categories = new SelectList(await _context.WorkshopCategories.OrderBy(c => c.Name).ToListAsync(), nameof(WorkshopCategory.WorkshopCategoryId), nameof(WorkshopCategory.Name));
    }
}
