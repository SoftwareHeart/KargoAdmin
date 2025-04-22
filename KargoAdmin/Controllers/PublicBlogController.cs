using KargoAdmin.Data;
using KargoAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KargoAdmin.Controllers
{
    // Bu controller kullanıcı tarafı için blog sayfalarını yönetir
    public class PublicBlogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublicBlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Blog listesi
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6; // Her sayfada gösterilecek blog sayısı

            // Sadece yayınlanmış blogları getir
            var blogs = await _context.Blogs
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Author)
                .ToListAsync();

            // Toplam blog sayısı
            var totalBlogs = await _context.Blogs
                .Where(b => b.IsPublished)
                .CountAsync();

            // Sayfalama bilgisi
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);

            return View(blogs);
        }

        // Blog detay sayfası
        public async Task<IActionResult> Details(int? id, string slug)
        {
            if (id == null && string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            Blog blog;

            // ID ile mi yoksa slug ile mi arama yapılacak
            if (id.HasValue)
            {
                blog = await _context.Blogs
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync(m => m.Id == id && m.IsPublished);
            }
            else
            {
                blog = await _context.Blogs
                    .Include(b => b.Author)
                    .FirstOrDefaultAsync(m => m.Slug == slug && m.IsPublished);
            }

            if (blog == null)
            {
                return NotFound();
            }

            // Görüntülenme sayısını artır
            blog.ViewCount++;
            _context.Update(blog);
            await _context.SaveChangesAsync();

            // İlgili diğer blogları getir
            var relatedBlogs = new List<Blog>();

            // Etiketlere göre ilgili blogları bul
            if (!string.IsNullOrEmpty(blog.Tags))
            {
                var tags = blog.Tags.Split(',').Select(t => t.Trim()).ToList();

                // Aynı etiketlere sahip en fazla 3 blog
                relatedBlogs = await _context.Blogs
                    .Where(b => b.Id != blog.Id &&
                           b.IsPublished &&
                           tags.Any(tag => b.Tags.Contains(tag)))
                    .OrderByDescending(b => b.PublishDate)
                    .Take(3)
                    .ToListAsync();
            }

            // Eğer ilgili blog bulunamazsa, son eklenen bloglardan göster
            if (relatedBlogs.Count == 0)
            {
                relatedBlogs = await _context.Blogs
                    .Where(b => b.Id != blog.Id && b.IsPublished)
                    .OrderByDescending(b => b.PublishDate)
                    .Take(3)
                    .ToListAsync();
            }

            ViewBag.RelatedBlogs = relatedBlogs;

            return View(blog);
        }

        // Tag ile filtreleme
        public async Task<IActionResult> Tag(string tag, int page = 1)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return RedirectToAction(nameof(Index));
            }

            int pageSize = 6;

            // Belirli etiketi içeren blogları getir
            var blogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Tags.Contains(tag))
                .OrderByDescending(b => b.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Author)
                .ToListAsync();

            // Toplam blog sayısı
            var totalBlogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Tags.Contains(tag))
                .CountAsync();

            // Sayfalama bilgisi
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);
            ViewBag.CurrentTag = tag;

            return View("Index", blogs);
        }
    }
}