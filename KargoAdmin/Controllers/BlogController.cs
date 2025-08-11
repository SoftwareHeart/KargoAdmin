// BlogController.cs - Düzeltilmiş versiyon
using KargoAdmin.Data;
using KargoAdmin.Models;
using KargoAdmin.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KargoAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public BlogController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // Blog listesi
        public async Task<IActionResult> Index()
        {
            var blogs = await _context.Blogs
                .Include(b => b.Author)
                .OrderByDescending(b => b.PublishDate)
                .ToListAsync();

            return View(blogs);
        }

        // Blog detayı - ID parametresini güvenli şekilde al
        public async Task<IActionResult> Details(int? id)
        {
            // ID kontrolü
            if (id == null || id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null)
            {
                TempData["ErrorMessage"] = "Blog yazısı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Log ekleme - debugging için
            System.Diagnostics.Debug.WriteLine($"Blog Details - Requested ID: {id}, Found Blog ID: {blog.Id}, Title: {blog.Title}");

            return View(blog);
        }

        // Blog düzenleme sayfası - ID parametresini güvenli şekilde al
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                TempData["ErrorMessage"] = "Düzenlenecek blog yazısı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new BlogEditViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                TitleEn = blog.TitleEn,
                Content = blog.Content,
                ContentEn = blog.ContentEn,
                ExistingImageUrl = blog.ImageUrl,
                IsPublished = blog.IsPublished,
                MetaDescription = blog.MetaDescription,
                MetaDescriptionEn = blog.MetaDescriptionEn,
                Tags = blog.Tags,
                TagsEn = blog.TagsEn,
                Type = blog.Type ?? "Haber" // Type alanını ekle
            };

            return View(viewModel);
        }

        // Blog düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogEditViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                try
                {
                    var blog = await _context.Blogs.FindAsync(id);
                    if (blog == null)
                    {
                        TempData["ErrorMessage"] = "Blog yazısı bulunamadı.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Yeni resim yüklendiyse
                    string fileName = blog.ImageUrl;
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(blog.ImageUrl))
                        {
                            DeleteImage(blog.ImageUrl);
                        }

                        fileName = await UploadImage(model.ImageFile);
                    }

                    // Blog bilgilerini güncelle
                    blog.Title = model.Title;
                    blog.TitleEn = model.TitleEn;
                    blog.Content = model.Content;
                    blog.ContentEn = model.ContentEn;
                    blog.ImageUrl = fileName;
                    blog.UpdateDate = DateTime.Now;
                    blog.IsPublished = model.IsPublished;
                    blog.MetaDescription = model.MetaDescription ?? "";
                    blog.MetaDescriptionEn = model.MetaDescriptionEn;
                    blog.Tags = model.Tags ?? "";
                    blog.TagsEn = model.TagsEn;
                    blog.Type = model.Type ?? "Haber"; // Type alanını güncelle
                    blog.Slug = GenerateSlug(model.Title);
                    blog.SlugEn = !string.IsNullOrEmpty(model.TitleEn) ? GenerateSlug(model.TitleEn) : null;

                    _context.Update(blog);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"{blog.Type} başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }

            return View(model);
        }

        // Blog silme sayfası
        public async Task<IActionResult> Delete(int? id)
        {
            // ID kontrolü
            if (id == null || id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null)
            {
                TempData["ErrorMessage"] = "Silinecek blog yazısı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Log ekleme - debugging için
            System.Diagnostics.Debug.WriteLine($"Blog Delete - Requested ID: {id}, Found Blog ID: {blog.Id}, Title: {blog.Title}");

            return View(blog);
        }

        // Blog silme onayı (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // ID geçerlilik kontrolü
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                TempData["ErrorMessage"] = "Silinecek blog yazısı bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            // Log ekleme - debugging için
            System.Diagnostics.Debug.WriteLine($"Blog Delete Confirmed - ID: {id}, Blog ID: {blog.Id}, Title: {blog.Title}");

            // Resmi sil
            if (!string.IsNullOrEmpty(blog.ImageUrl))
            {
                DeleteImage(blog.ImageUrl);
            }

            _context.Blogs.Remove(blog);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Blog yazısı başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }

        // Blog oluşturma sayfası
        public IActionResult Create()
        {
            return View(new BlogCreateViewModel { IsPublished = true });
        }

        // Blog oluşturma (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogCreateViewModel model)
        {
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                string fileName = null;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    fileName = await UploadImage(model.ImageFile);
                }

                var user = await _userManager.GetUserAsync(User);
                string slug = GenerateSlug(model.Title);

                var blog = new Blog
                {
                    Title = model.Title,
                    TitleEn = model.TitleEn,
                    Content = model.Content,
                    ContentEn = model.ContentEn,
                    ImageUrl = fileName,
                    PublishDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    IsPublished = model.IsPublished,
                    AuthorId = user.Id,
                    MetaDescription = model.MetaDescription ?? "",
                    MetaDescriptionEn = model.MetaDescriptionEn,
                    Tags = model.Tags ?? "",
                    TagsEn = model.TagsEn,
                    Slug = slug,
                    SlugEn = !string.IsNullOrEmpty(model.TitleEn) ? GenerateSlug(model.TitleEn) : null,
                    ViewCount = 0,
                    Type = model.Type ?? "Haber" // Type alanını ekle
                };

                _context.Add(blog);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{blog.Type} başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Yardımcı metodlar
        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title))
                return Guid.NewGuid().ToString();

            // Türkçe karakterleri değiştir
            title = title.Replace("ı", "i").Replace("İ", "I")
                         .Replace("ş", "s").Replace("Ş", "S")
                         .Replace("ğ", "g").Replace("Ğ", "G")
                         .Replace("ü", "u").Replace("Ü", "U")
                         .Replace("ö", "o").Replace("Ö", "O")
                         .Replace("ç", "c").Replace("Ç", "C");

            // Küçük harfe çevir
            title = title.ToLowerInvariant();

            // Özel karakterleri temizle
            title = Regex.Replace(title, @"[^a-z0-9\s-]", "");

            // Boşlukları ve çoklu tire işaretlerini tek tire ile değiştir
            title = Regex.Replace(title, @"\s+", "-");
            title = Regex.Replace(title, @"-+", "-");

            // Baş ve sondaki tireleri temizle
            title = title.Trim('-');

            return string.IsNullOrEmpty(title) ? Guid.NewGuid().ToString() : title;
        }

        private async Task<string> UploadImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            // ESKİ SİSTEM: Sadece dosya adını döndür
            return uniqueFileName;
        }

        private void DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            try
            {
                string imagePath;

                // Eski sistem: Sadece dosya adı
                if (!imageUrl.StartsWith("/") && !imageUrl.StartsWith("http"))
                {
                    imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", imageUrl);
                }
                else
                {
                    // Yeni sistem: Tam yol (geçici uyumluluk için)
                    imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                }

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting image: {ex.Message}");
            }
        }
    }
}