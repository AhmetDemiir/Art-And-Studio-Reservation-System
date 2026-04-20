using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Data;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;
using Online_Art_Gallery_and_Studio_Reservation_System.Models.ViewModels;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Controllers;

[Authorize(Roles = "Admin")]
public class AdminArtworkController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminArtworkController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var artworks = await _context.Artworks
            .Include(a => a.Artist)
            .Include(a => a.ArtworkCategory)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
        return View(artworks);
    }

    public async Task<IActionResult> Create()
    {
        await LoadSelectsAsync();
        return View(new ArtworkManageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArtworkManageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectsAsync();
            return View(model);
        }

        var artwork = new Artwork
        {
            Title = model.Title.Trim(),
            Description = model.Description?.Trim(),
            Price = model.Price,
            IsForSale = model.IsForSale,
            StockQuantity = model.StockQuantity,
            ArtistId = model.ArtistId,
            ArtworkCategoryId = model.ArtworkCategoryId,
            CreatedAt = DateTime.UtcNow
        };
        _context.Artworks.Add(artwork);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(model.PrimaryImageUrl))
        {
            _context.ArtworkImages.Add(new ArtworkImage
            {
                ArtworkId = artwork.ArtworkId,
                ImageUrl = model.PrimaryImageUrl.Trim(),
                AltText = model.PrimaryImageAltText?.Trim(),
                IsPrimary = true,
                DisplayOrder = 0
            });
            await _context.SaveChangesAsync();
        }

        TempData["SuccessMessage"] = "Eser başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var artwork = await _context.Artworks
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.ArtworkId == id);
        if (artwork is null)
        {
            return NotFound();
        }

        var primaryImage = artwork.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault();
        var vm = new ArtworkManageViewModel
        {
            ArtworkId = artwork.ArtworkId,
            Title = artwork.Title,
            Description = artwork.Description,
            Price = artwork.Price,
            IsForSale = artwork.IsForSale,
            StockQuantity = artwork.StockQuantity,
            ArtistId = artwork.ArtistId,
            ArtworkCategoryId = artwork.ArtworkCategoryId,
            PrimaryImageUrl = primaryImage?.ImageUrl,
            PrimaryImageAltText = primaryImage?.AltText
        };

        await LoadSelectsAsync();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ArtworkManageViewModel model)
    {
        if (!ModelState.IsValid || !model.ArtworkId.HasValue)
        {
            await LoadSelectsAsync();
            return View(model);
        }

        var artwork = await _context.Artworks
            .Include(a => a.Images)
            .FirstOrDefaultAsync(a => a.ArtworkId == model.ArtworkId.Value);
        if (artwork is null)
        {
            return NotFound();
        }

        artwork.Title = model.Title.Trim();
        artwork.Description = model.Description?.Trim();
        artwork.Price = model.Price;
        artwork.IsForSale = model.IsForSale;
        artwork.StockQuantity = model.StockQuantity;
        artwork.ArtistId = model.ArtistId;
        artwork.ArtworkCategoryId = model.ArtworkCategoryId;

        var primary = artwork.Images.FirstOrDefault(i => i.IsPrimary) ?? artwork.Images.FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(model.PrimaryImageUrl))
        {
            if (primary is null)
            {
                artwork.Images.Add(new ArtworkImage
                {
                    ImageUrl = model.PrimaryImageUrl.Trim(),
                    AltText = model.PrimaryImageAltText?.Trim(),
                    IsPrimary = true,
                    DisplayOrder = 0
                });
            }
            else
            {
                primary.ImageUrl = model.PrimaryImageUrl.Trim();
                primary.AltText = model.PrimaryImageAltText?.Trim();
                primary.IsPrimary = true;
            }
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Eser başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var artwork = await _context.Artworks.FirstOrDefaultAsync(a => a.ArtworkId == id);
        if (artwork is null)
        {
            return NotFound();
        }

        _context.Artworks.Remove(artwork);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Eser silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectsAsync()
    {
        ViewBag.Artists = new SelectList(await _context.Artists.OrderBy(a => a.FullName).ToListAsync(), nameof(Artist.ArtistId), nameof(Artist.FullName));
        ViewBag.Categories = new SelectList(await _context.ArtworkCategories.OrderBy(c => c.Name).ToListAsync(), nameof(ArtworkCategory.ArtworkCategoryId), nameof(ArtworkCategory.Name));
    }
}
