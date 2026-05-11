using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;
using Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

[Authorize(Roles = "Admin")]
public class AdminCampaignController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminCampaignController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _context.Campaigns.OrderByDescending(c => c.StartDate).ToListAsync();
        return View(list);
    }

    public IActionResult Create()
    {
        return View(new CampaignManageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CampaignManageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.EndDate < model.StartDate)
        {
            ModelState.AddModelError(nameof(model.EndDate), "Bitiş tarihi başlangıçtan önce olamaz.");
            return View(model);
        }

        _context.Campaigns.Add(new Campaign
        {
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            DiscountValue = model.DiscountValue,
            IsPercentage = model.IsPercentage,
            StartDate = model.StartDate.ToUniversalTime(),
            EndDate = model.EndDate.ToUniversalTime(),
            IsActive = model.IsActive
        });
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Kampanya oluşturuldu. Eser veya etkinlik düzenleyerek bağlayabilirsiniz.";
        return RedirectToAction(nameof(Index));
    }
}
