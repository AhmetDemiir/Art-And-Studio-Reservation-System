using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;
using Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

[Authorize(Roles = "Admin")]
public class CouponController : Controller
{
    private readonly ApplicationDbContext _context;

    public CouponController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var coupons = await _context.Coupons
            .OrderByDescending(c => c.ValidTo)
            .ToListAsync();
        return View(coupons);
    }

    public IActionResult Create()
    {
        return View(new CouponCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CouponCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedCode = model.Code.Trim().ToUpperInvariant();
        var exists = await _context.Coupons.AnyAsync(c => c.Code.ToUpper() == normalizedCode);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Code), "Bu kupon kodu zaten mevcut.");
            return View(model);
        }

        var coupon = new Coupon
        {
            Code = normalizedCode,
            Description = model.Description?.Trim(),
            IsPercentage = model.IsPercentage,
            DiscountValue = model.DiscountValue,
            MaxDiscountAmount = model.MaxDiscountAmount,
            MinimumOrderAmount = model.MinimumOrderAmount,
            MaxUsageCount = model.MaxUsageCount,
            ValidFrom = DateTime.UtcNow,
            ValidTo = model.ValidTo.ToUniversalTime(),
            IsActive = true,
            CurrentUsageCount = 0
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Kupon başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }
}
