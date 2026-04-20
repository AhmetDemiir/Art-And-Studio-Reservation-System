using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;
using Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

[Authorize]
public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrderController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Checkout(int artworkId, string? couponCode = null)
    {
        var artwork = await _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.ArtworkId == artworkId);

        if (artwork is null)
        {
            return NotFound();
        }

        if (!artwork.IsForSale || artwork.StockQuantity <= 0)
        {
            TempData["ErrorMessage"] = "Bu eser su an satin alima uygun degil.";
            return RedirectToAction("Details", "Artwork", new { id = artworkId });
        }

        var vm = new CheckoutViewModel
        {
            ArtworkId = artwork.ArtworkId,
            ArtworkTitle = artwork.Title,
            ArtistName = artwork.Artist.FullName,
            ImageUrl = artwork.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? artwork.Images.FirstOrDefault()?.ImageUrl,
            UnitPrice = artwork.Price,
            AvailableStock = artwork.StockQuantity,
            FinalAmount = artwork.Price
        };

        if (!string.IsNullOrWhiteSpace(couponCode))
        {
            await ApplyCouponToViewModel(vm, couponCode.Trim());
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApplyCoupon(int artworkId, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            TempData["ErrorMessage"] = "Kupon kodu boş bırakılamaz.";
            return RedirectToAction(nameof(Checkout), new { artworkId });
        }

        return RedirectToAction(nameof(Checkout), new { artworkId, couponCode = code.Trim() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
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

        var artwork = await _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.ArtworkId == model.ArtworkId);

        if (artwork is null)
        {
            return NotFound();
        }

        if (!artwork.IsForSale || artwork.StockQuantity <= 0)
        {
            TempData["ErrorMessage"] = "Bu eser su an satin alima uygun degil.";
            return RedirectToAction("Details", "Artwork", new { id = artwork.ArtworkId });
        }

        var appliedCoupon = await ResolveValidCouponAsync(model.CouponCode, artwork.Price);
        var discountAmount = appliedCoupon is null ? 0 : CalculateDiscount(appliedCoupon, artwork.Price);
        var finalAmount = Math.Max(0, artwork.Price - discountAmount);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
            var totalAmount = finalAmount;

            var order = new Order
            {
                UserId = userId,
                CouponId = appliedCoupon?.CouponId,
                OrderNumber = orderNumber,
                Subtotal = artwork.Price,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                Status = OrderStatus.Paid,
                CreatedAt = DateTime.UtcNow
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                ArtworkId = artwork.ArtworkId,
                Quantity = 1,
                UnitPrice = artwork.Price,
                TotalPrice = artwork.Price
            };
            _context.OrderItems.Add(orderItem);

            var payment = new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = model.SelectedPaymentMethod,
                PaymentStatus = PaymentStatus.Succeeded,
                Amount = totalAmount,
                TransactionId = $"TX-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);

            if (appliedCoupon is not null)
            {
                appliedCoupon.CurrentUsageCount += 1;
                _context.CouponRedemptions.Add(new CouponRedemption
                {
                    CouponId = appliedCoupon.CouponId,
                    UserId = userId,
                    OrderId = order.OrderId,
                    RedeemedAt = DateTime.UtcNow
                });
            }

            artwork.StockQuantity -= 1;
            if (artwork.StockQuantity <= 0)
            {
                artwork.StockQuantity = 0;
                artwork.IsForSale = false;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["SuccessMessage"] = "Siparişiniz başarıyla alındı.";
            return RedirectToAction(nameof(MyOrders));
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = "Odeme islenirken bir hata olustu. Lutfen tekrar deneyin.";
            return RedirectToAction(nameof(Checkout), new { artworkId = model.ArtworkId });
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Artwork)
                    .ThenInclude(a => a.Images)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return View(orders);
    }

    private async Task<Coupon?> ResolveValidCouponAsync(string? code, decimal orderAmount)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var normalizedCode = code.Trim().ToUpperInvariant();
        var now = DateTime.UtcNow;
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c =>
                c.Code.ToUpper() == normalizedCode &&
                c.IsActive &&
                c.ValidFrom <= now &&
                c.ValidTo >= now);

        if (coupon is null)
        {
            return null;
        }

        if (coupon.MinimumOrderAmount.HasValue && orderAmount < coupon.MinimumOrderAmount.Value)
        {
            return null;
        }

        if (coupon.MaxUsageCount.HasValue && coupon.CurrentUsageCount >= coupon.MaxUsageCount.Value)
        {
            return null;
        }

        return coupon;
    }

    private static decimal CalculateDiscount(Coupon coupon, decimal orderAmount)
    {
        var discount = coupon.IsPercentage
            ? orderAmount * coupon.DiscountValue / 100m
            : coupon.DiscountValue;

        if (coupon.MaxDiscountAmount.HasValue)
        {
            discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);
        }

        return Math.Max(0, Math.Min(discount, orderAmount));
    }

    private async Task ApplyCouponToViewModel(CheckoutViewModel vm, string code)
    {
        var coupon = await ResolveValidCouponAsync(code, vm.UnitPrice);
        if (coupon is null)
        {
            vm.CouponCode = code;
            vm.DiscountAmount = 0;
            vm.FinalAmount = vm.UnitPrice;
            vm.IsCouponApplied = false;
            TempData["ErrorMessage"] = "Kupon kodu geçersiz veya süresi dolmuş.";
            return;
        }

        vm.CouponCode = coupon.Code;
        vm.DiscountAmount = CalculateDiscount(coupon, vm.UnitPrice);
        vm.FinalAmount = Math.Max(0, vm.UnitPrice - vm.DiscountAmount);
        vm.IsCouponApplied = true;
        vm.CouponValidTo = coupon.ValidTo;
        TempData["SuccessMessage"] = "Kupon başarıyla uygulandı.";
    }
}
