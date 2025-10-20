using KargoAdmin.Data;
using KargoAdmin.Models;
using KargoAdmin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KargoAdmin.Controllers
{
    // Bu controller kullanıcı tarafı için sayfaları yönetir
    public class AlerisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AlerisController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            // Son 3 blogu getir - sadece gerekli alanları seç (projection ile performans artışı)
            var recentBlogs = await _context.Blogs
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishDate)
                .Take(3)
                .Select(b => new Blog
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    ImageUrl = b.ImageUrl,
                    Content = b.Content.Substring(0, Math.Min(b.Content.Length, 200)), // Sadece ilk 200 karakter
                    PublishDate = b.PublishDate,
                    Author = new ApplicationUser
                    {
                        FirstName = b.Author.FirstName,
                        LastName = b.Author.LastName
                    }
                })
                .AsNoTracking() // Tracking kapalı - daha hızlı
                .ToListAsync();

            ViewBag.RecentBlogs = recentBlogs;

            return View();
        }

        [Route("Aleris/Hakkımızda")]
        public IActionResult About()
        {
            return View();
        }

        [Route("Aleris/Iletisim")]
        public IActionResult Contact()
        {
            return View();
        }

        // İletişim form gönderimi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Burada e-posta gönderme işlemi yapılabilir
                // Şimdilik sadece başarılı mesaj gösteriyoruz
                TempData["SuccessMessage"] = "Mesajınız başarıyla gönderildi. En kısa sürede sizinle iletişime geçeceğiz.";
                return RedirectToAction(nameof(Contact));
            }

            return View(model);
        }

        // Kargo takip sayfası (gelecekte eklenecek)
        public IActionResult CargoTracking()
        {
            return View();
        }

        // Mevcut controller'a aşağıdaki action'ları ekleyin:
        [Route("Aleris/Servisler/KaraYolu")]
        public IActionResult LandTransport()
        {
            ViewData["Title"] = "Kara Yolu Taşımacılığı";
            return View();
        }

        [Route("Aleris/Servisler/HavaYolu")]
        public IActionResult AirTransport()
        {
            ViewData["Title"] = "Hava Yolu Taşımacılığı";
            return View();
        }

        [Route("Aleris/Servisler/DenizYolu")]
        public IActionResult SeaTransport()
        {
            ViewData["Title"] = "Deniz Yolu Taşımacılığı";
            return View();
        }

        [Route("Aleris/Servisler/DepolamaDagitim")]
        public IActionResult StorageDistribution()
        {
            ViewData["Title"] = "Depolama ve Dağıtım";
            return View();
        }
    }
}