using KargoAdmin.Data;
using KargoAdmin.Models;
using KargoAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KargoAdmin.Controllers
{
    public class PublicBlogController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILanguageService _languageService;

        public PublicBlogController(ApplicationDbContext context, ILanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }

        // Blog listesi
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6;
            var currentLanguage = _languageService.GetCurrentLanguage();

            var blogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Haber")
                .OrderByDescending(b => b.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Author)
                .ToListAsync();

            var totalBlogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Haber")
                .CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);
            ViewBag.CurrentLanguage = currentLanguage;

            // Dinamik tag listesi - BASİT YÖNTEM
            ViewBag.AvailableTags = await GetAvailableTagsSimple();

            return View(blogs);
        }

        // Tag ile filtreleme (TR+EN tag alanlarını destekle)
        public async Task<IActionResult> Tag(string tag, int page = 1)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return RedirectToAction(nameof(Index));
            }

            int pageSize = 6;

            var blogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Haber" &&
                            ((b.Tags != null && b.Tags.Contains(tag)) ||
                             (b.TagsEn != null && b.TagsEn.Contains(tag))))
                .OrderByDescending(b => b.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Author)
                .ToListAsync();

            var totalBlogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Haber" &&
                            ((b.Tags != null && b.Tags.Contains(tag)) ||
                             (b.TagsEn != null && b.TagsEn.Contains(tag))))
                .CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);
            ViewBag.CurrentTag = tag;
            ViewBag.AvailableTags = await GetAvailableTagsSimple();

            return View("Index", blogs);
        }

        // Arama (TR+EN alanlarını destekle)
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction(nameof(Index));
            }

            int pageSize = 6;

            var blogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Haber" &&
                           ((b.Title != null && b.Title.Contains(query)) ||
                            (b.TitleEn != null && b.TitleEn.Contains(query)) ||
                            (b.Content != null && b.Content.Contains(query)) ||
                            (b.ContentEn != null && b.ContentEn.Contains(query)) ||
                            (b.Tags != null && b.Tags.Contains(query)) ||
                            (b.TagsEn != null && b.TagsEn.Contains(query))))
                .OrderByDescending(b => b.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Author)
                .ToListAsync();

            var totalBlogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Haber" &&
                           ((b.Title != null && b.Title.Contains(query)) ||
                            (b.TitleEn != null && b.TitleEn.Contains(query)) ||
                            (b.Content != null && b.Content.Contains(query)) ||
                            (b.ContentEn != null && b.ContentEn.Contains(query)) ||
                            (b.Tags != null && b.Tags.Contains(query)) ||
                            (b.TagsEn != null && b.TagsEn.Contains(query))))
                .CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);
            ViewBag.SearchQuery = query;
            ViewBag.AvailableTags = await GetAvailableTagsSimple();

            return View("Index", blogs);
        }

        // Blog detay sayfası - GELİŞTİRİLMİŞ VERSİYON
        public async Task<IActionResult> Details(int? id, string slug)
        {
            if (id == null && string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            Blog blog;

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
                    .FirstOrDefaultAsync(m => m.IsPublished && (m.Slug == slug || m.SlugEn == slug));
            }

            if (blog == null)
            {
                return NotFound();
            }

            // View sayısını artır
            blog.ViewCount++;
            _context.Update(blog);
            await _context.SaveChangesAsync();

            // İlgili yazıları bul - GELİŞTİRİLMİŞ MANTIK
            var relatedBlogs = await GetRelatedBlogs(blog);

            ViewBag.RelatedBlogs = relatedBlogs.relatedPosts;
            ViewBag.RelatedPostsType = relatedBlogs.type; // "related", "popular", "recent"

            return View(blog);
        }

        // İlgili yazıları bulma mantığı - YENİ METOD
        private async Task<(List<Blog> relatedPosts, string type)> GetRelatedBlogs(Blog currentBlog)
        {
            var relatedBlogs = new List<Blog>();
            string relationType = "recent"; // varsayılan

            // 1. ÖNCE ETİKETLERE GÖRE İLGİLİ YAZILAR ARA
            if (!string.IsNullOrEmpty(currentBlog.Tags))
            {
                var tags = currentBlog.Tags.Split(',').Select(t => t.Trim()).ToList();

                relatedBlogs = await _context.Blogs
                    .Where(b => b.Id != currentBlog.Id &&
                               b.IsPublished && b.Type == "Haber" &&
                               tags.Any(tag => b.Tags.Contains(tag)))
                    .OrderByDescending(b => b.PublishDate)
                    .Take(3)
                    .ToListAsync();

                if (relatedBlogs.Count >= 2) // En az 2 ilgili yazı varsa
                {
                    relationType = "related";
                    return (relatedBlogs, relationType);
                }
            }

            // 2. ETİKET BAZLI İLGİLİ YAZI YOKSA, POPÜLER YAZILAR GETİR
            var popularBlogs = await _context.Blogs
                .Where(b => b.Id != currentBlog.Id && b.IsPublished && b.Type == "Haber")
                .OrderByDescending(b => b.ViewCount)
                .ThenByDescending(b => b.PublishDate)
                .Take(3)
                .ToListAsync();

            if (popularBlogs.Count >= 2) // En az 2 popüler yazı varsa
            {
                relationType = "popular";
                return (popularBlogs, relationType);
            }

            // 3. POPÜLER YAZI DA YOKSA, EN SON YAZILAR GETİR
            var recentBlogs = await _context.Blogs
                .Where(b => b.Id != currentBlog.Id && b.IsPublished && b.Type == "Haber")
                .OrderByDescending(b => b.PublishDate)
                .Take(3)
                .ToListAsync();

            relationType = "recent";
            return (recentBlogs, relationType);
        }

        // BASİT TAG LİSTESİ - Dil duyarlı
        private async Task<List<object>> GetAvailableTagsSimple()
        {
            var result = new List<object>();

            try
            {
                var currentLang = _languageService.GetCurrentLanguage();

                // Mevcut dile göre tag alanını seç
                IQueryable<string> tagQuery;
                if (currentLang == "en")
                {
                    tagQuery = _context.Blogs
                        .Where(b => b.IsPublished && b.Type == "Haber" && !string.IsNullOrEmpty(b.TagsEn))
                        .Select(b => b.TagsEn!);
                }
                else
                {
                    tagQuery = _context.Blogs
                        .Where(b => b.IsPublished && b.Type == "Haber" && !string.IsNullOrEmpty(b.Tags))
                        .Select(b => b.Tags!);
                }

                var allTagStrings = await tagQuery.ToListAsync();

                var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var tagString in allTagStrings)
                {
                    var tags = tagString.Split(',', StringSplitOptions.RemoveEmptyEntries);

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
                result = new List<object>();
            }

            return result;
        }
    }
}