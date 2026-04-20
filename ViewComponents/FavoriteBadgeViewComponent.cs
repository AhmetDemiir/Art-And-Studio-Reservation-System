using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;

namespace Online_Art_Gallery_and_Studio_Reservation_System.ViewComponents;

public class FavoriteBadgeViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public FavoriteBadgeViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return View(0);
        }

        var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return View(0);
        }

        var favoriteCount = await _context.ArtworkFavorites.CountAsync(f => f.UserId == userId);
        return View(favoriteCount);
    }
}
