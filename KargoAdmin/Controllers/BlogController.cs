using KargoAdmin.Data;
using KargoAdmin.Models;
using KargoAdmin.Models.ViewModels;
using KargoAdmin.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace KargoAdmin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BlogController> _logger;

        private static readonly string[] AllowedImageTypes = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        public BlogController(
            ApplicationDbContext context,
            IFileService fileService,
            UserManager<ApplicationUser> userManager,
            ILogger<BlogController> logger)
        {
            _context = context;
            _fileService = fileService;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Blog/Index - Blog listesi
        public async Task<IActionResult> Index()
        {
            try
            {
                var blogs = await _context.Blogs
                    .Include(b => b.Author)
                    .OrderByDescending(b => b.PublishDate)
                    .ToListAsync();

                _logger.LogInformation("Blog listesi başarıyla yüklendi. Toplam blog sayısı: {Count}", blogs.Count);
                return View(blogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog listesi yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Blog listesi yüklenirken bir hata oluştu.";
                return View(new List<Blog>());
            }
        }

        // GET: Blog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarning("Details action'a geçersiz ID ile erişim: {Id}", id);
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var blog = await _context.Blogs
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (blog == null)
                {
                    _logger.LogWarning("Blog bulunamadı. ID: {Id}", id);
                    TempData["ErrorMessage"] = "Blog yazısı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Blog detayı görüntülendi. ID: {Id}, Başlık: {Title}", blog.Id, blog.Title);
                return View(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog detayı yüklenirken hata oluştu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Blog detayı yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Blog/Create
        public IActionResult Create()
        {
            _logger.LogInformation("Blog oluşturma sayfası açıldı");
            return View(new BlogCreateViewModel { IsPublished = false, Type = "Faydalı Bilgi" });
        }

        // POST: Blog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogCreateViewModel model)
        {
            _logger.LogInformation("Blog oluşturma işlemi başlatıldı. Başlık: {Title}", model.Title);

            try
            {
                // Tags alanı boşsa ModelState'den hatayı kaldır
                if (string.IsNullOrWhiteSpace(model.Tags))
                {
                    ModelState.Remove("Tags");
                    model.Tags = "";
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Model validation hatası. Hata sayısı: {ErrorCount}", ModelState.ErrorCount);
                    return View(model);
                }

                string? fileName = null;

                // Modern resim yükleme işlemi
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    try
                    {
                        // Dosya validasyonu
                        if (!_fileService.IsValidFileType(model.ImageFile, AllowedImageTypes))
                        {
                            ModelState.AddModelError("ImageFile", "Sadece JPG, JPEG, PNG veya WEBP formatları desteklenir.");
                            return View(model);
                        }

                        if (!_fileService.IsValidFileSize(model.ImageFile, 10 * 1024 * 1024)) // 10MB
                        {
                            ModelState.AddModelError("ImageFile", "Dosya boyutu 10MB'ı aşamaz.");
                            return View(model);
                        }

                        fileName = await _fileService.UploadFileAsync(model.ImageFile, "blogs");
                        _logger.LogInformation("Resim başarıyla yüklendi: {FileName}", fileName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Resim yükleme hatası");
                        ModelState.AddModelError("ImageFile", ex.Message);
                        return View(model);
                    }
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    _logger.LogError("Kullanıcı bulunamadı");
                    TempData["ErrorMessage"] = "Kullanıcı bilgisi alınamadı.";
                    return View(model);
                }

                var blog = new Blog
                {
                    Title = model.Title?.Trim(),
                    TitleEn = model.TitleEn?.Trim(),
                    Content = model.Content?.Trim(),
                    ContentEn = model.ContentEn?.Trim(),
                    ImageUrl = fileName,
                    PublishDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    IsPublished = model.IsPublished,
                    AuthorId = currentUser.Id,
                    MetaDescription = model.MetaDescription?.Trim() ?? "",
                    MetaDescriptionEn = model.MetaDescriptionEn?.Trim(),
                    Tags = model.Tags?.Trim() ?? "",
                    TagsEn = model.TagsEn?.Trim(),
                    Slug = GenerateSlug(model.Title),
                    SlugEn = !string.IsNullOrEmpty(model.TitleEn) ? GenerateSlug(model.TitleEn) : null,
                    ViewCount = 0,
                    Type = model.Type ?? "Faydalı Bilgi"
                };

                _context.Blogs.Add(blog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog başarıyla oluşturuldu. ID: {Id}, Başlık: {Title}", blog.Id, blog.Title);
                TempData["SuccessMessage"] = $"{blog.Type} başarıyla oluşturuldu.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog oluşturma sırasında hata oluştu");
                TempData["ErrorMessage"] = "Blog oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.";
                return View(model);
            }
        }

        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarning("Edit action'a geçersiz ID ile erişim: {Id}", id);
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var blog = await _context.Blogs.FindAsync(id.Value);
                if (blog == null)
                {
                    _logger.LogWarning("Düzenlenecek blog bulunamadı. ID: {Id}", id);
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
                    Type = blog.Type ?? "Haber"
                };

                _logger.LogInformation("Blog düzenleme sayfası açıldı. ID: {Id}, Başlık: {Title}", blog.Id, blog.Title);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog düzenleme sayfası yüklenirken hata oluştu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Blog düzenleme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Blog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogEditViewModel model)
        {
            if (id != model.Id)
            {
                _logger.LogWarning("Edit işleminde ID uyumsuzluğu. URL ID: {UrlId}, Model ID: {ModelId}", id, model.Id);
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            // ImageFile validation'ı kaldır çünkü opsiyonel
            ModelState.Remove("ImageFile");
            
            // Tags alanı boşsa ModelState'den hatayı kaldır
            if (string.IsNullOrWhiteSpace(model.Tags))
            {
                ModelState.Remove("Tags");
                model.Tags = "";
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Edit model validation hatası. ID: {Id}", id);
                return View(model);
            }

            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog == null)
                {
                    _logger.LogWarning("Güncellenecek blog bulunamadı. ID: {Id}", id);
                    TempData["ErrorMessage"] = "Blog yazısı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                string? imageUrl = blog.ImageUrl;

                // Yeni resim yüklendiyse (Modern approach)
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    try
                    {
                        // Dosya validasyonu
                        if (!_fileService.IsValidFileType(model.ImageFile, AllowedImageTypes))
                        {
                            ModelState.AddModelError("ImageFile", "Sadece JPG, JPEG, PNG veya WEBP formatları desteklenir.");
                            return View(model);
                        }

                        if (!_fileService.IsValidFileSize(model.ImageFile, 10 * 1024 * 1024)) // 10MB
                        {
                            ModelState.AddModelError("ImageFile", "Dosya boyutu 10MB'ı aşamaz.");
                            return View(model);
                        }

                        // Eski resmi sil
                        if (!string.IsNullOrEmpty(blog.ImageUrl))
                        {
                            await _fileService.DeleteFileAsync(blog.ImageUrl);
                        }

                        imageUrl = await _fileService.UploadFileAsync(model.ImageFile, "blogs");
                        _logger.LogInformation("Blog için yeni resim yüklendi. ID: {Id}, FileName: {FileName}", id, imageUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Blog güncellerken resim yükleme hatası. ID: {Id}", id);
                        ModelState.AddModelError("ImageFile", ex.Message);
                        return View(model);
                    }
                }

                // Blog bilgilerini güncelle
                blog.Title = model.Title?.Trim();
                blog.TitleEn = model.TitleEn?.Trim();
                blog.Content = model.Content?.Trim();
                blog.ContentEn = model.ContentEn?.Trim();
                blog.ImageUrl = imageUrl;
                blog.UpdateDate = DateTime.Now;
                blog.IsPublished = model.IsPublished;
                blog.MetaDescription = model.MetaDescription?.Trim() ?? "";
                blog.MetaDescriptionEn = model.MetaDescriptionEn?.Trim();
                blog.Tags = model.Tags?.Trim() ?? "";
                blog.TagsEn = model.TagsEn?.Trim();
                blog.Type = model.Type ?? "Faydalı Bilgi";
                blog.Slug = GenerateSlug(model.Title);
                blog.SlugEn = !string.IsNullOrEmpty(model.TitleEn) ? GenerateSlug(model.TitleEn) : null;

                _context.Update(blog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog başarıyla güncellendi. ID: {Id}, Başlık: {Title}", blog.Id, blog.Title);
                TempData["SuccessMessage"] = $"{blog.Type} başarıyla güncellendi.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Blog güncelleme concurrency hatası. ID: {Id}", id);
                if (!BlogExists(id))
                {
                    TempData["ErrorMessage"] = "Blog yazısı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog güncelleme sırasında hata oluştu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Blog güncellenirken bir hata oluştu. Lütfen tekrar deneyin.";
                return View(model);
            }
        }

        // GET: Blog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                _logger.LogWarning("Delete action'a geçersiz ID ile erişim: {Id}", id);
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var blog = await _context.Blogs
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (blog == null)
                {
                    _logger.LogWarning("Silinecek blog bulunamadı. ID: {Id}", id);
                    TempData["ErrorMessage"] = "Silinecek blog yazısı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("Blog silme onay sayfası açıldı. ID: {Id}, Başlık: {Title}", blog.Id, blog.Title);
                return View(blog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog silme sayfası yüklenirken hata oluştu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Blog silme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("DeleteConfirmed'a geçersiz ID ile erişim: {Id}", id);
                TempData["ErrorMessage"] = "Geçersiz blog ID'si.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var blog = await _context.Blogs.FindAsync(id);
                if (blog == null)
                {
                    _logger.LogWarning("Silinecek blog bulunamadı. ID: {Id}", id);
                    TempData["ErrorMessage"] = "Silinecek blog yazısı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Resmi sil (Modern approach)
                if (!string.IsNullOrEmpty(blog.ImageUrl))
                {
                    await _fileService.DeleteFileAsync(blog.ImageUrl);
                }

                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Blog başarıyla silindi. ID: {Id}, Başlık: {Title}", blog.Id, blog.Title);
                TempData["SuccessMessage"] = $"{blog.Type} başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blog silme sırasında hata oluştu. ID: {Id}", id);
                TempData["ErrorMessage"] = "Blog silinirken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Private Helper Methods

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }

        private string GenerateSlug(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Guid.NewGuid().ToString("N")[0..8];

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

            return string.IsNullOrEmpty(title) ? Guid.NewGuid().ToString("N")[0..8] : title;
        }


        #endregion
    }
}