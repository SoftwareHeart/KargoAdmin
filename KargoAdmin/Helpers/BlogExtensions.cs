using KargoAdmin.Models;
using KargoAdmin.Services;

namespace KargoAdmin.Helpers
{
    public static class BlogExtensions
    {
        public static string GetLocalizedTitle(this Blog blog, ILanguageService languageService)
        {
            var currentLang = languageService.GetCurrentLanguage();
            return currentLang == "en" && !string.IsNullOrEmpty(blog.TitleEn) ? blog.TitleEn : blog.Title;
        }

        public static string GetLocalizedContent(this Blog blog, ILanguageService languageService)
        {
            var currentLang = languageService.GetCurrentLanguage();
            return currentLang == "en" && !string.IsNullOrEmpty(blog.ContentEn) ? blog.ContentEn : blog.Content;
        }

        public static string GetLocalizedMetaDescription(this Blog blog, ILanguageService languageService)
        {
            var currentLang = languageService.GetCurrentLanguage();
            return currentLang == "en" && !string.IsNullOrEmpty(blog.MetaDescriptionEn) ? blog.MetaDescriptionEn : blog.MetaDescription;
        }

        public static string GetLocalizedTags(this Blog blog, ILanguageService languageService)
        {
            var currentLang = languageService.GetCurrentLanguage();
            return currentLang == "en" && !string.IsNullOrEmpty(blog.TagsEn) ? blog.TagsEn : blog.Tags;
        }

        public static string GetLocalizedSlug(this Blog blog, ILanguageService languageService)
        {
            var currentLang = languageService.GetCurrentLanguage();
            return currentLang == "en" && !string.IsNullOrEmpty(blog.SlugEn) ? blog.SlugEn : blog.Slug;
        }
    }
}
