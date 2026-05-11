using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<ApplicationUser> _userManager;

    public CouponController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var coupons = await _context.Coupons
            .Include(c => c.RestrictedUser)
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

        string? restrictedUserId = null;
        if (!string.IsNullOrWhiteSpace(model.RestrictedUserEmail))
        {
            var normalizedEmail = model.RestrictedUserEmail.Trim();
            var targetUser = await _userManager.FindByEmailAsync(normalizedEmail);
            if (targetUser is null)
            {
                ModelState.AddModelError(nameof(model.RestrictedUserEmail), "Bu e-posta ile kayıtlı kullanıcı bulunamadı.");
                return View(model);
            }

            restrictedUserId = targetUser.Id;
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
            CurrentUsageCount = 0,
            RestrictedUserId = restrictedUserId
        };

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Kupon başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }
}
