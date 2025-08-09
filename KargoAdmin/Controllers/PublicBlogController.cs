using KargoAdmin.Data;
using KargoAdmin.Models;
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

        public PublicBlogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Blog listesi
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6;

            var blogs = await _context.Blogs
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Author)
                .ToListAsync();

            var totalBlogs = await _context.Blogs
                .Where(b => b.IsPublished)
                .CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);

            // Dinamik tag listesi - BASİT YÖNTEM
            ViewBag.AvailableTags = await GetAvailableTagsSimple();

            return View(blogs);
        }

        // Tag ile filtreleme
        public async Task<IActionResult> Tag(string tag, int page = 1)
        {
            if (string.IsNullOrEmpty(tag))
            {
                return RedirectToAction(nameof(Index));
            }

            int pageSize = 6;

            var blogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Tags.Contains(tag))
                .OrderByDescending(b => b.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Author)
                .ToListAsync();

            var totalBlogs = await _context.Blogs
                .Where(b => b.IsPublished && b.Tags.Contains(tag))
                .CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);
            ViewBag.CurrentTag = tag;
            ViewBag.AvailableTags = await GetAvailableTagsSimple();

            return View("Index", blogs);
        }

        // Arama
        public async Task<IActionResult> Search(string query, int page = 1)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction(nameof(Index));
            }

            int pageSize = 6;

            var blogs = await _context.Blogs
                .Where(b => b.IsPublished &&
                           (b.Title.Contains(query) ||
                            b.Content.Contains(query) ||
                            b.Tags.Contains(query)))
                .OrderByDescending(b => b.PublishDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Author)
                .ToListAsync();

            var totalBlogs = await _context.Blogs
                .Where(b => b.IsPublished &&
                           (b.Title.Contains(query) ||
                            b.Content.Contains(query) ||
                            b.Tags.Contains(query)))
                .CountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalBlogs / (double)pageSize);
            ViewBag.SearchQuery = query;
            ViewBag.AvailableTags = await GetAvailableTagsSimple();

            return View("Index", blogs);
        }

        // Blog detay sayfası
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
                    .FirstOrDefaultAsync(m => m.Slug == slug && m.IsPublished);
            }

            if (blog == null)
            {
                return NotFound();
            }

            blog.ViewCount++;
            _context.Update(blog);
            await _context.SaveChangesAsync();

            var relatedBlogs = new List<Blog>();

            if (!string.IsNullOrEmpty(blog.Tags))
            {
                var tags = blog.Tags.Split(',').Select(t => t.Trim()).ToList();

                relatedBlogs = await _context.Blogs
                    .Where(b => b.Id != blog.Id &&
                           b.IsPublished &&
                           tags.Any(tag => b.Tags.Contains(tag)))
                    .OrderByDescending(b => b.PublishDate)
                    .Take(3)
                    .ToListAsync();
            }

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

        // BASİT TAG LİSTESİ - Hata vermez
        private async Task<List<object>> GetAvailableTagsSimple()
        {
            var result = new List<object>();

            try
            {
                // Tüm yayınlanmış blogların tag'lerini al
                var allTagStrings = await _context.Blogs
                    .Where(b => b.IsPublished && !string.IsNullOrEmpty(b.Tags))
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