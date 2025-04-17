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
            return View();
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

                var blog = new Blog
                {
                    Title = model.Title,
                    Content = model.Content,
                    ImageUrl = fileName, // Null olabilir
                    PublishDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    IsPublished = model.IsPublished,
                    AuthorId = user.Id
                };

                _context.Add(blog);
                await _context.SaveChangesAsync();
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
                IsPublished = blog.IsPublished
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

            if (ModelState.IsValid)
            {
                try
                {
                    var blog = await _context.Blogs.FindAsync(id);
                    if (blog == null)
                    {
                        return NotFound();
                    }

                    // Blog bilgilerini güncelle
                    blog.Title = model.Title;
                    blog.Content = model.Content;
                    blog.IsPublished = model.IsPublished;
                    blog.UpdateDate = DateTime.Now;

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
                    // Resim olmadığında mevcut değeri koru (ImageUrl güncellenmeyecek)

                    _context.Update(blog);
                    await _context.SaveChangesAsync();

                    // Başarılı güncelleme, listeye dön
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
    }
}