using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Online_Art_Gallery_and_Studio_Reservation_System.Models;

namespace Online_Art_Gallery_and_Studio_Reservation_System.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedServices = scope.ServiceProvider;

        var context = scopedServices.GetRequiredService<ApplicationDbContext>();
        var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedAdminUserAsync(userManager);
        await SeedSampleDataAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = new[] { "Admin", "Member" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@artgallery.local";
        const string adminPassword = "Admin123!";
        const string adminRole = "Admin";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                PhoneNumberConfirmed = true,
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Admin user could not be created: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, adminRole))
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }

    private static async Task SeedSampleDataAsync(ApplicationDbContext context)
    {
        if (await context.Artists.AnyAsync() || await context.ArtworkCategories.AnyAsync())
        {
            return;
        }

        var artists = new List<Artist>
        {
            new()
            {
                FullName = "Aylin Karaca",
                Nationality = "Türkiye",
                Biography = "Soyut ekspresyonist kompozisyonlar üreten çağdaş sanatçı.",
                ProfileImageUrl = "https://images.unsplash.com/photo-1487412720507-e7ab37603c6f"
            },
            new()
            {
                FullName = "Mert Demir",
                Nationality = "Türkiye",
                Biography = "Kent temalı dijital kolajlarıyla bilinen görsel sanatçı.",
                ProfileImageUrl = "https://images.unsplash.com/photo-1542204625-de293a9b7b1f"
            },
            new()
            {
                FullName = "Selin Atay",
                Nationality = "Türkiye",
                Biography = "Minimalist renk dili ve doku denemeleriyle öne çıkar.",
                ProfileImageUrl = "https://images.unsplash.com/photo-1544005313-94ddf0286df2"
            }
        };

        var categories = new List<ArtworkCategory>
        {
            new() { Name = "Yağlı Boya", Description = "Kanvas üzerine yağlı boya çalışmaları." },
            new() { Name = "Dijital Sanat", Description = "Dijital tekniklerle üretilmiş eserler." },
            new() { Name = "Akrilik", Description = "Akrilik boya ile üretilen modern eserler." }
        };

        var workshopCategories = new List<WorkshopCategory>
        {
            new() { Name = "Resim Atölyesi", Description = "Temel ve ileri seviye resim teknikleri." },
            new() { Name = "Dijital Tasarım", Description = "Dijital araçlarla sanat üretimi." }
        };

        context.Artists.AddRange(artists);
        context.ArtworkCategories.AddRange(categories);
        context.WorkshopCategories.AddRange(workshopCategories);
        await context.SaveChangesAsync();

        var artworks = new List<Artwork>
        {
            new()
            {
                Title = "Şehir Işıkları",
                Description = "Gece şehir manzarasını soyut fırça dokusuyla yorumlayan eser.",
                Price = 12500,
                ArtistId = artists[0].ArtistId,
                ArtworkCategoryId = categories[0].ArtworkCategoryId,
                CreatedAt = DateTime.UtcNow.AddDays(-12)
            },
            new()
            {
                Title = "Mavi Sessizlik",
                Description = "Dinginliği temsil eden mavi tonlu minimalist kompozisyon.",
                Price = 9800,
                ArtistId = artists[2].ArtistId,
                ArtworkCategoryId = categories[2].ArtworkCategoryId,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Title = "Ritmik Katmanlar",
                Description = "Kent ritmini katmanlı dijital kolaj teknikleriyle ele alır.",
                Price = 7600,
                ArtistId = artists[1].ArtistId,
                ArtworkCategoryId = categories[1].ArtworkCategoryId,
                CreatedAt = DateTime.UtcNow.AddDays(-8)
            },
            new()
            {
                Title = "Doğaya Dönüş",
                Description = "Doğal dokuları yumuşak renk geçişleriyle bir araya getiren tablo.",
                Price = 14300,
                ArtistId = artists[0].ArtistId,
                ArtworkCategoryId = categories[0].ArtworkCategoryId,
                CreatedAt = DateTime.UtcNow.AddDays(-6)
            },
            new()
            {
                Title = "Kırmızı Hatıra",
                Description = "Renk kontrastı odaklı, duygusal anlatımı güçlü akrilik eser.",
                Price = 11200,
                ArtistId = artists[2].ArtistId,
                ArtworkCategoryId = categories[2].ArtworkCategoryId,
                CreatedAt = DateTime.UtcNow.AddDays(-4)
            }
        };

        context.Artworks.AddRange(artworks);
        await context.SaveChangesAsync();

        var artworkImages = new List<ArtworkImage>
        {
            new() { ArtworkId = artworks[0].ArtworkId, ImageUrl = "https://images.unsplash.com/photo-1579783902614-a3fb3927b6a5", IsPrimary = true, AltText = "Şehir Işıkları" },
            new() { ArtworkId = artworks[1].ArtworkId, ImageUrl = "https://images.unsplash.com/photo-1578301978018-3005759f48f7", IsPrimary = true, AltText = "Mavi Sessizlik" },
            new() { ArtworkId = artworks[2].ArtworkId, ImageUrl = "https://images.unsplash.com/photo-1545239351-1141bd82e8a6", IsPrimary = true, AltText = "Ritmik Katmanlar" },
            new() { ArtworkId = artworks[3].ArtworkId, ImageUrl = "https://images.unsplash.com/photo-1513364776144-60967b0f800f", IsPrimary = true, AltText = "Doğaya Dönüş" },
            new() { ArtworkId = artworks[4].ArtworkId, ImageUrl = "https://images.unsplash.com/photo-1460661419201-fd4cecdf8a8b", IsPrimary = true, AltText = "Kırmızı Hatıra" }
        };

        context.ArtworkImages.AddRange(artworkImages);

        var workshops = new List<WorkshopEvent>
        {
            new()
            {
                Title = "Akrilikle Modern Dokular",
                Description = "Katman teknikleri, spatula kullanımı ve renk geçişleri üzerine uygulamalı atölye.",
                CoverImageUrl = "https://images.unsplash.com/photo-1513364776144-60967b0f800f",
                Location = "Beyoğlu Studio",
                BasePrice = 1500,
                WorkshopCategoryId = workshopCategories[0].WorkshopCategoryId,
                IsActive = true
            },
            new()
            {
                Title = "Dijital Kolaj ve Kompozisyon",
                Description = "Dijital araçlarla kompozisyon kurma, katman düzeni ve hikâye anlatımı.",
                CoverImageUrl = "https://images.unsplash.com/photo-1517694712202-14dd9538aa97",
                Location = "Kadıköy Creative Lab",
                BasePrice = 1800,
                WorkshopCategoryId = workshopCategories[1].WorkshopCategoryId,
                IsActive = true
            }
        };

        context.WorkshopEvents.AddRange(workshops);
        await context.SaveChangesAsync();

        var schedules = new List<WorkshopSchedule>
        {
            new()
            {
                WorkshopEventId = workshops[0].WorkshopEventId,
                StartDateTime = DateTime.UtcNow.AddDays(5).Date.AddHours(11),
                EndDateTime = DateTime.UtcNow.AddDays(5).Date.AddHours(14),
                Capacity = 20,
                Fee = 1500
            },
            new()
            {
                WorkshopEventId = workshops[0].WorkshopEventId,
                StartDateTime = DateTime.UtcNow.AddDays(20).Date.AddHours(12),
                EndDateTime = DateTime.UtcNow.AddDays(20).Date.AddHours(15),
                Capacity = 20,
                Fee = 1650
            },
            new()
            {
                WorkshopEventId = workshops[1].WorkshopEventId,
                StartDateTime = DateTime.UtcNow.AddDays(9).Date.AddHours(10),
                EndDateTime = DateTime.UtcNow.AddDays(9).Date.AddHours(13),
                Capacity = 16,
                Fee = 1800
            }
        };

        context.WorkshopSchedules.AddRange(schedules);
        await context.SaveChangesAsync();
    }
}
