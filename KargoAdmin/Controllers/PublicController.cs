using KargoAdmin.Data;
using KargoAdmin.Models;
using KargoAdmin.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KargoAdmin.Controllers
{
    // Bu controller kullanıcı tarafı için sayfaları yönetir
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ana sayfa
        public async Task<IActionResult> Index()
        {
            // Son 3 blogu getir
            var recentBlogs = await _context.Blogs
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishDate)
                .Take(3)
                .Include(b => b.Author)
                .ToListAsync();

            ViewBag.RecentBlogs = recentBlogs;

            return View();
        }

        // Hakkımızda sayfası
        public IActionResult About()
        {
            return View();
        }

        // İletişim sayfası
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

        public IActionResult LandTransport()
        {
            return View();
        }

        public IActionResult SeaTransport()
        {
            return View();
        }

        public IActionResult AirTransport()
        {
            return View();
        }

        public IActionResult WarehouseDistribution()
        {
            return View();
        }
    }
}