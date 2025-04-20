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
            // ImageFile için ModelState doğrulamasını kaldır
            ModelState.Remove("ImageFile");

            if (ModelState.IsValid)
            {
                string fileName = null; // Varsayılan olarak null

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    fileName = await UploadImage(model.ImageFile);
                }

                var user = await _userManager.GetUserAsync(User);

                // URL uyumlu slug oluştur
                string slug = GenerateSlug(model.Title);

                var blog = new Blog
                {
                    Title = model.Title,
                    Content = model.Content,
                    ImageUrl = fileName, // Null olabilir
                    PublishDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    IsPublished = model.IsPublished,
                    AuthorId = user.Id,
                    MetaDescription = model.MetaDescription ?? "", // Null ise boş string
                    Tags = model.Tags ?? "", // Null ise boş string
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

        // Blog düzenleme sayfası
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }

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
            if (id != model.Id)
            {
                return NotFound();
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
                        return NotFound();
                    }

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

                    // Başarılı güncelleme, listeye dön
                    TempData["SuccessMessage"] = "Blog yazısı başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Hata durumunda log
                    Console.WriteLine($"Edit error: {ex.Message}");
                    ModelState.AddModelError("", "Güncelleme sırasında bir hata oluştu: " + ex.Message);
                }
            }

            // Kod buraya ulaşırsa (hata varsa) modelini tekrar view'e döndür
            return View(model);
        }

        // Blog silme
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // Blog silme onayı (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);

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

        // Blog detayı
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // Yardımcı metotlar
        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }

        private async Task<string> UploadImage(IFormFile file)
        {
            if (file == null) return null;

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // uploads klasörü yoksa oluştur
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        private void DeleteImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        // SEO dostu URL oluşturmak için slug oluşturucu
        private string GenerateSlug(string title)
        {
            // Türkçe karakterleri değiştir
            string turkishChars = "ığüşöçĞÜŞİÖÇ";
            string englishChars = "igusocGUSIOC";

            for (int i = 0; i < turkishChars.Length; i++)
            {
                title = title.Replace(turkishChars[i], englishChars[i]);
            }

            // Boşlukları, özel karakterleri, sembolleri temizle
            string slug = Regex.Replace(title.ToLower(), @"[^a-z0-9\s-]", "");

            // Boşlukları tire ile değiştir
            slug = Regex.Replace(slug, @"\s+", "-");

            // Ardışık tireleri tek tire yap
            slug = Regex.Replace(slug, @"-+", "-");

            // Başta ve sonda tire varsa kaldır
            slug = slug.Trim('-');

            return slug;
        }
    }
}