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
            // ID kontrolü
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

            // Log ekleme - debugging için
            System.Diagnostics.Debug.WriteLine($"Blog Edit - Requested ID: {id}, Found Blog ID: {blog.Id}, Title: {blog.Title}");

            var model = new BlogEditViewModel
            {
                Id = blog.Id,
                Title = blog.Title,
                Content = blog.Content,
                ExistingImageUrl = blog.ImageUrl,
                IsPublished = blog.IsPublished,
                MetaDescription = blog.MetaDescription,
                Tags = blog.Tags
            };

            return View(model);
        }

        // Blog düzenleme (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogEditViewModel model)
        {
            // ID eşleşme kontrolü
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "ID eşleşmiyor.";
                return RedirectToAction(nameof(Index));
            }

            // ID geçerlilik kontrolü
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            // ImageFile için ModelState doğrulamasını kaldır
            ModelState.Remove("ImageFile");
            ModelState.Remove("ExistingImageUrl");
            ModelState.Remove("Tags");

            if (ModelState.IsValid)
            {
                try
                {
                    var blog = await _context.Blogs.FindAsync(id);
                    if (blog == null)
                    {
                        TempData["ErrorMessage"] = "Güncellenecek blog yazısı bulunamadı.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Log ekleme - debugging için
                    System.Diagnostics.Debug.WriteLine($"Blog Edit POST - ID: {id}, Blog ID: {blog.Id}, Title: {blog.Title}");

                    // URL uyumlu slug oluştur (başlık değiştiyse)
                    if (blog.Title != model.Title)
                    {
                        blog.Slug = GenerateSlug(model.Title);
                    }

                    // Blog bilgilerini güncelle
                    blog.Title = model.Title;
                    blog.Content = model.Content;
                    blog.IsPublished = model.IsPublished;
                    blog.UpdateDate = DateTime.Now;
                    blog.MetaDescription = model.MetaDescription;
                    blog.Tags = model.Tags;

                    // Yeni resim yüklendiyse işle
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(blog.ImageUrl))
                        {
                            DeleteImage(blog.ImageUrl);
                        }

                        // Yeni resmi yükle
                        blog.ImageUrl = await UploadImage(model.ImageFile);
                    }

                    // Eğer varsa ve true ise mevcut görseli sil
                    bool deleteExistingImage = false;
                    if (bool.TryParse(Request.Form["DeleteExistingImage"], out deleteExistingImage) && deleteExistingImage)
                    {
                        if (!string.IsNullOrEmpty(blog.ImageUrl))
                        {
                            DeleteImage(blog.ImageUrl);
                            blog.ImageUrl = null;
                        }
                    }

                    _context.Update(blog);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Blog yazısı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Edit error: {ex.Message}");
                    ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu: " + ex.Message);
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
                    Content = model.Content,
                    ImageUrl = fileName,
                    PublishDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    IsPublished = model.IsPublished,
                    AuthorId = user.Id,
                    MetaDescription = model.MetaDescription ?? "",
                    Tags = model.Tags ?? "",
                    Slug = slug,
                    ViewCount = 0
                };

                _context.Add(blog);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Blog yazısı başarıyla oluşturuldu.";
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