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
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
        public async Task<IActionResult> Settings()
        {
            var settings = await _context.Settings.AsNoTracking().ToListAsync();

            var model = new SettingsViewModel
            {
                SiteTitle = settings.FirstOrDefault(s => s.Key == "SiteTitle")?.Value ?? string.Empty,
                SiteDescription = settings.FirstOrDefault(s => s.Key == "SiteDescription")?.Value,
                ContactEmail = settings.FirstOrDefault(s => s.Key == "ContactEmail")?.Value,
                ContactPhone = settings.FirstOrDefault(s => s.Key == "ContactPhone")?.Value,
                Address = settings.FirstOrDefault(s => s.Key == "Address")?.Value,
                FacebookUrl = settings.FirstOrDefault(s => s.Key == "FacebookUrl")?.Value,
                TwitterUrl = settings.FirstOrDefault(s => s.Key == "TwitterUrl")?.Value,
                InstagramUrl = settings.FirstOrDefault(s => s.Key == "InstagramUrl")?.Value,
                LinkedInUrl = settings.FirstOrDefault(s => s.Key == "LinkedInUrl")?.Value
            };

            return View(model);
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

                if (!string.IsNullOrEmpty(model.Address))
                    await SaveSetting("Address", model.Address);

                if (!string.IsNullOrEmpty(model.FacebookUrl))
                    await SaveSetting("FacebookUrl", model.FacebookUrl);

                if (!string.IsNullOrEmpty(model.TwitterUrl))
                    await SaveSetting("TwitterUrl", model.TwitterUrl);

                if (!string.IsNullOrEmpty(model.InstagramUrl))
                    await SaveSetting("InstagramUrl", model.InstagramUrl);

                if (!string.IsNullOrEmpty(model.LinkedInUrl))
                    await SaveSetting("LinkedInUrl", model.LinkedInUrl);

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

        // Şifre değiştirme action metodları
        [Authorize(Roles = "Admin")]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Şifreyi değiştir
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                // Şifre başarıyla değiştirildi, tekrar giriş yap
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction(nameof(Profile));
            }

            // Eğer buraya kadar geldiyse, bir şeyler başarısız oldu
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
    }
}