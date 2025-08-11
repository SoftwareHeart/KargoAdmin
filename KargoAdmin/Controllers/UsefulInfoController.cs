using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KargoAdmin.Data;
using KargoAdmin.Models;
using System.Text.RegularExpressions;

namespace KargoAdmin.Controllers
{
    public class UsefulInfoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsefulInfoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ana sayfa - Faydalı Bilgiler listesi
        public async Task<IActionResult> Index(string tag, int page = 1, int pageSize = 12)
        {
            var query = _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Faydalı Bilgi")
                .OrderByDescending(b => b.PublishDate);

            // Tag filtreleme
            if (!string.IsNullOrEmpty(tag))
            {
                query = (IOrderedQueryable<Blog>)query.Where(b => b.Tags.Contains(tag));
                ViewBag.CurrentTag = tag;
            }

            // Toplam sayı hesaplama
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Sayfalama
            var usefulInfos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Mevcut etiketleri getir - GetAvailableTagsSimple kullan
            ViewBag.AvailableTags = await GetAvailableTagsSimple();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(usefulInfos);
        }

        // Tag'e göre filtreleme - PublicBlog ile aynı mantık
        public async Task<IActionResult> Tag(string tag, int page = 1)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return RedirectToAction(nameof(Index));
            }

            int pageSize = 6;

            var blogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Tags.Contains(tag) && b.Type == "Faydalı Bilgi")
                .OrderByDescending(b => b.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Author)
                .ToListAsync();

            var totalBlogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Tags.Contains(tag) && b.Type == "Faydalı Bilgi")
                .CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);
            ViewBag.CurrentTag = tag;
            ViewBag.AvailableTags = await GetAvailableTagsSimple();

            return View("Index", blogs);
        }

        // Detay sayfası - PublicBlog ile aynı yapı
        public async Task<IActionResult> Details(int? id, string slug)
        {
            Blog usefulInfo = null;

            // Önce ID ile arama (PublicBlog gibi)
            if (id.HasValue)
            {
                usefulInfo = await _context.Blogs
                    .Include(b => b.Author) // Author bilgisini de yükle
                    .FirstOrDefaultAsync(b => b.Id == id.Value &&
                                            b.IsPublished &&
                                            b.Type == "Faydalı Bilgi");
            }
            // ID bulunamadıysa slug ile arama
            else if (!string.IsNullOrEmpty(slug))
            {
                usefulInfo = await _context.Blogs
                    .Include(b => b.Author) // Author bilgisini de yükle
                    .FirstOrDefaultAsync(b => b.Slug == slug &&
                                            b.IsPublished &&
                                            b.Type == "Faydalı Bilgi");
            }

            if (usefulInfo == null)
            {
                return NotFound();
            }

            // Görüntülenme sayısını artır
            usefulInfo.ViewCount++;
            await _context.SaveChangesAsync();

            // İlgili faydalı bilgileri getir (aynı etiketlere sahip)
            var relatedInfos = new List<Blog>();
            if (!string.IsNullOrEmpty(usefulInfo.Tags))
            {
                var tags = usefulInfo.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim()).ToList();

                relatedInfos = await _context.Blogs
                    .Where(b => b.Id != usefulInfo.Id &&
                              b.IsPublished &&
                              b.Type == "Faydalı Bilgi" &&
                              tags.Any(tag => b.Tags.Contains(tag)))
                    .OrderByDescending(b => b.PublishDate)
                    .Take(4)
                    .ToListAsync();
            }

            // Eğer yeterli ilgili içerik yoksa, son eklenenlerden getir
            if (relatedInfos.Count < 4)
            {
                var additionalInfos = await _context.Blogs
                    .Where(b => b.Id != usefulInfo.Id &&
                              b.IsPublished &&
                              b.Type == "Faydalı Bilgi" &&
                              !relatedInfos.Select(r => r.Id).Contains(b.Id))
                    .OrderByDescending(b => b.PublishDate)
                    .Take(4 - relatedInfos.Count)
                    .ToListAsync();

                relatedInfos.AddRange(additionalInfos);
            }

            ViewBag.RelatedInfos = relatedInfos;

            // SEO için meta veriler
            ViewBag.MetaTitle = usefulInfo.Title;
            ViewBag.MetaDescription = !string.IsNullOrEmpty(usefulInfo.MetaDescription)
                ? usefulInfo.MetaDescription
                : StripHtml(usefulInfo.Content).Substring(0, Math.Min(160, usefulInfo.Content.Length));
            ViewBag.MetaKeywords = usefulInfo.Tags;

            return View(usefulInfo);
        }

        // Arama
        public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 12)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Index");
            }

            var searchQuery = _context.Blogs
                .Where(b => b.IsPublished &&
                          b.Type == "Faydalı Bilgi" &&
                          (b.Title.Contains(query) ||
                           b.Content.Contains(query) ||
                           b.Tags.Contains(query)))
                .OrderByDescending(b => b.PublishDate);

            var totalCount = await searchQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var results = await searchQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SearchQuery = query;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.AvailableTags = await GetAvailableTagsSimple();

            return View("Index", results);
        }

        // HTML etiketlerini temizleme
        private string StripHtml(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        // BASİT TAG LİSTESİ - PublicBlog ile aynı mantık
        private async Task<List<object>> GetAvailableTagsSimple()
        {
            var result = new List<object>();

            try
            {
                // Sadece "Faydalı Bilgi" tipindeki yayınlanmış blogların tag'lerini al
                var allTagStrings = await _context.Blogs
                    .Where(b => b.IsPublished && b.Type == "Faydalı Bilgi" && !string.IsNullOrEmpty(b.Tags))
                    .Select(b => b.Tags)
                    .ToListAsync();

                // Tag sayılarını hesapla
                var tagCounts = new Dictionary<string, int>();

                foreach (var tagString in allTagStrings)
                {
                    // "Dijital Dönüşüm,Teknoloji,Lojistik,IoT" formatındaki string'i ayır
                    var tags = tagString.Split(',');

                    foreach (var tag in tags)
                    {
                        var cleanTag = tag.Trim();
                        if (!string.IsNullOrEmpty(cleanTag))
                        {
                            if (tagCounts.ContainsKey(cleanTag))
                                tagCounts[cleanTag]++;
                            else
                                tagCounts[cleanTag] = 1;
                        }
                    }
                }

                // En çok kullanılan tag'lerden az kullanılana doğru sırala
                var sortedTags = tagCounts
                    .OrderByDescending(x => x.Value)
                    .ThenBy(x => x.Key);

                foreach (var tag in sortedTags)
                {
                    result.Add(new { Name = tag.Key, Count = tag.Value });
                }
            }
            catch (Exception)
            {
                // Hata durumunda boş liste döndür
                result = new List<object>();
            }

            return result;
        }
    }
}