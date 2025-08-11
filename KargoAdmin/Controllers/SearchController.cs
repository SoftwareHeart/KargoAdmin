using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KargoAdmin.Data;
using KargoAdmin.Models;
using KargoAdmin.Models.ViewModels;
using KargoAdmin.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KargoAdmin.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILanguageService _languageService;

        public SearchController(ApplicationDbContext context, ILanguageService languageService)
        {
            _context = context;
            _languageService = languageService;
        }

        // Site genel arama
        public async Task<IActionResult> Index(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Index", "PublicBlog");
            }

            string query = q.Trim();

            // Haberler (Blog/News)
            var blogQueryable = _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Haber" &&
                            ((b.Title != null && b.Title.Contains(query)) ||
                             (b.TitleEn != null && b.TitleEn.Contains(query)) ||
                             (b.Content != null && b.Content.Contains(query)) ||
                             (b.ContentEn != null && b.ContentEn.Contains(query)) ||
                             (b.Tags != null && b.Tags.Contains(query)) ||
                             (b.TagsEn != null && b.TagsEn.Contains(query))));

            int blogTotal = await blogQueryable.CountAsync();
            var blogResults = await blogQueryable
                .OrderByDescending(b => b.PublishDate)
                .Include(b => b.Author)
                .Take(6)
                .ToListAsync();

            // Faydalı Bilgiler
            var usefulQueryable = _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Faydalı Bilgi" &&
                            ((b.Title != null && b.Title.Contains(query)) ||
                             (b.TitleEn != null && b.TitleEn.Contains(query)) ||
                             (b.Content != null && b.Content.Contains(query)) ||
                             (b.ContentEn != null && b.ContentEn.Contains(query)) ||
                             (b.Tags != null && b.Tags.Contains(query)) ||
                             (b.TagsEn != null && b.TagsEn.Contains(query))));

            int usefulTotal = await usefulQueryable.CountAsync();
            var usefulResults = await usefulQueryable
                .OrderByDescending(b => b.PublishDate)
                .Include(b => b.Author)
                .Take(6)
                .ToListAsync();

            // Statik sayfa eşleşmeleri (basit anahtar kelime eşlemesi)
            var pageMatches = BuildPageMatches(query);

            var viewModel = new SearchViewModel
            {
                Query = query,
                BlogResults = blogResults,
                BlogTotal = blogTotal,
                UsefulInfoResults = usefulResults,
                UsefulInfoTotal = usefulTotal,
                Pages = pageMatches
            };

            ViewBag.CurrentLanguage = _languageService.GetCurrentLanguage();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> PopularTags()
        {
            try
            {
                var lang = _languageService.GetCurrentLanguage();

                IQueryable<string> haberTags = lang == "en"
                    ? _context.Blogs.Where(b => b.IsPublished && b.Type == "Haber" && b.TagsEn != null).Select(b => b.TagsEn!)
                    : _context.Blogs.Where(b => b.IsPublished && b.Type == "Haber" && b.Tags != null).Select(b => b.Tags!);

                IQueryable<string> usefulTags = lang == "en"
                    ? _context.Blogs.Where(b => b.IsPublished && b.Type == "Faydalı Bilgi" && b.TagsEn != null).Select(b => b.TagsEn!)
                    : _context.Blogs.Where(b => b.IsPublished && b.Type == "Faydalı Bilgi" && b.Tags != null).Select(b => b.Tags!);

                var all = await haberTags.Concat(usefulTags).ToListAsync();

                var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var s in all)
                {
                    foreach (var raw in (s ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var t = raw.Trim();
                        if (t.Length == 0) continue;
                        tagCounts[t] = tagCounts.ContainsKey(t) ? tagCounts[t] + 1 : 1;
                    }
                }

                var top = tagCounts
                    .OrderByDescending(kv => kv.Value)
                    .ThenBy(kv => kv.Key)
                    .Take(12)
                    .Select(kv => kv.Key)
                    .ToList();

                return Json(top);
            }
            catch
            {
                return Json(Array.Empty<string>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Suggest(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Json(new { pages = Array.Empty<object>(), news = Array.Empty<object>(), useful = Array.Empty<object>() });
            }

            string query = q.Trim();
            string lang = _languageService.GetCurrentLanguage();

            // Pages suggestions
            var pages = BuildPageMatches(query)
                .Take(5)
                .Select(p => new { title = p.Title, url = p.Url })
                .ToList();

            // News titles
            var news = await _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Haber" &&
                            ((b.Title != null && b.Title.Contains(query)) ||
                             (b.TitleEn != null && b.TitleEn.Contains(query))))
                .OrderByDescending(b => b.PublishDate)
                .Select(b => new
                {
                    title = lang == "en" && b.TitleEn != null && b.TitleEn != string.Empty ? b.TitleEn : b.Title,
                    url = Url.Action("Details", "PublicBlog", new { id = b.Id, slug = lang == "en" && !string.IsNullOrEmpty(b.SlugEn) ? b.SlugEn : b.Slug })
                })
                .Take(5)
                .ToListAsync();

            // Useful info titles
            var useful = await _context.Blogs
                .Where(b => b.IsPublished && b.Type == "Faydalı Bilgi" &&
                            ((b.Title != null && b.Title.Contains(query)) ||
                             (b.TitleEn != null && b.TitleEn.Contains(query))))
                .OrderByDescending(b => b.PublishDate)
                .Select(b => new
                {
                    title = lang == "en" && b.TitleEn != null && b.TitleEn != string.Empty ? b.TitleEn : b.Title,
                    url = Url.Action("Details", "UsefulInfo", new { id = b.Id, slug = lang == "en" && !string.IsNullOrEmpty(b.SlugEn) ? b.SlugEn : b.Slug })
                })
                .Take(5)
                .ToListAsync();

            return Json(new { pages, news, useful });
        }
        private List<SearchViewModel.PageResult> BuildPageMatches(string query)
        {
            var lower = query.ToLowerInvariant();

            var pages = new List<(string Title, string Url, string[] Keywords)>
            {
                (Title: "Anasayfa", Url: "/", Keywords: new[]{"anasayfa","home"}),
                (Title: "Hakkımızda", Url: "/Aleris/Hakkımızda", Keywords: new[]{"hakkında","hakkımızda","about"}),
                (Title: "İletişim", Url: "/Aleris/Iletisim", Keywords: new[]{"iletişim","contact","telefon","mail"}),
                (Title: "Kara Yolu Taşımacılığı", Url: "/Aleris/Servisler/KaraYolu", Keywords: new[]{"kara","karayolu","road","truck"}),
                (Title: "Hava Yolu Taşımacılığı", Url: "/Aleris/Servisler/HavaYolu", Keywords: new[]{"hava","uçak","air","air cargo","uçuş"}),
                (Title: "Deniz Yolu Taşımacılığı", Url: "/Aleris/Servisler/DenizYolu", Keywords: new[]{"deniz","gemi","sea","ocean","ship"}),
                (Title: "Depolama ve Dağıtım", Url: "/Aleris/Servisler/DepolamaDagitim", Keywords: new[]{"depo","depolama","dağıtım","storage","distribution"}),
                (Title: "Blog/Haber", Url: "/Blog", Keywords: new[]{"blog","haber","news","yazı"}),
                (Title: "Blog/Faydalı Bilgiler", Url: "/UsefulInfo", Keywords: new[]{"faydalı","bilgi","rehber","guide","ipucu","blog"})
            };

            var matches = new List<SearchViewModel.PageResult>();
            foreach (var page in pages)
            {
                if (page.Keywords.Any(k => lower.Contains(k)))
                {
                    matches.Add(new SearchViewModel.PageResult
                    {
                        Title = page.Title,
                        Url = page.Url
                    });
                }
            }

            // Eğer doğrudan isim eşleşmesi yoksa ve çok genel bir arama değilse, bazı sayfaları öneri olarak ekleyelim
            if (!matches.Any())
            {
                if (lower.Contains("servis") || lower.Contains("hizmet") || lower.Contains("service"))
                {
                    matches.Add(new SearchViewModel.PageResult { Title = "Hizmetlerimiz", Url = "/Aleris/Servisler/KaraYolu" });
                    matches.Add(new SearchViewModel.PageResult { Title = "Hizmetlerimiz - Hava", Url = "/Aleris/Servisler/HavaYolu" });
                    matches.Add(new SearchViewModel.PageResult { Title = "Hizmetlerimiz - Deniz", Url = "/Aleris/Servisler/DenizYolu" });
                }
            }

            return matches.DistinctBy(p => p.Url).ToList();
        }
    }
}


