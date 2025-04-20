using KargoAdmin.Data;
using KargoAdmin.Models;
using KargoAdmin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KargoAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var blogCount = await _context.Blogs.CountAsync();
            var publishedBlogCount = await _context.Blogs.Where(b => b.IsPublished).CountAsync();
            var draftBlogCount = await _context.Blogs.Where(b => !b.IsPublished).CountAsync();
            var lastWeekBlogCount = await _context.Blogs.Where(b => b.PublishDate >= DateTime.Now.AddDays(-7)).CountAsync();

            var recentBlogs = await _context.Blogs
                .OrderByDescending(b => b.PublishDate)
                .Take(5)
                .ToListAsync();

            var currentUser = await _userManager.GetUserAsync(User);

            ViewBag.BlogCount = blogCount;
            ViewBag.PublishedBlogCount = publishedBlogCount;
            ViewBag.DraftBlogCount = draftBlogCount;
            ViewBag.LastWeekBlogCount = lastWeekBlogCount;
            ViewBag.RecentBlogs = recentBlogs;
            ViewBag.CurrentUser = currentUser;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Site başlığı zorunlu, kaydedilmeli
                await SaveSetting("SiteTitle", model.SiteTitle);

                // Diğer alanlar isteğe bağlı, boş değilse kaydedilmeli
                if (!string.IsNullOrEmpty(model.SiteDescription))
                    await SaveSetting("SiteDescription", model.SiteDescription);

                if (!string.IsNullOrEmpty(model.ContactEmail))
                    await SaveSetting("ContactEmail", model.ContactEmail);

                if (!string.IsNullOrEmpty(model.ContactPhone))
                    await SaveSetting("ContactPhone", model.ContactPhone);

                if (!string.IsNullOrEmpty(model.FacebookUrl))
                    await SaveSetting("FacebookUrl", model.FacebookUrl);

                if (!string.IsNullOrEmpty(model.TwitterUrl))
                    await SaveSetting("TwitterUrl", model.TwitterUrl);

                if (!string.IsNullOrEmpty(model.InstagramUrl))
                    await SaveSetting("InstagramUrl", model.InstagramUrl);

                TempData["SuccessMessage"] = "Site ayarları başarıyla güncellendi.";
                return RedirectToAction(nameof(Settings));
            }

            return View(model);
        }

        // Ayarı kaydetmek için helper metod
        private async Task SaveSetting(string key, string value)
        {
            var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
            {
                // Ayar daha önce kaydedilmemişse yeni ekle
                setting = new Settings { Key = key, Value = value };
                _context.Settings.Add(setting);
            }
            else
            {
                // Ayar zaten varsa değerini güncelle
                setting.Value = value;
                _context.Settings.Update(setting);
            }

            await _context.SaveChangesAsync();
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Profil bilgileriniz başarıyla güncellendi.";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}