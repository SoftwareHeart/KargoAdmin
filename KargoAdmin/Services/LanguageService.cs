using Microsoft.AspNetCore.Http;

namespace KargoAdmin.Services
{
    public interface ILanguageService
    {
        string GetCurrentLanguage();
        void SetLanguage(string language);
        string GetLocalizedText(string trText, string enText);
    }

    public class LanguageService : ILanguageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LanguageService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentLanguage()
        {
            var language = _httpContextAccessor.HttpContext?.Session.GetString("CurrentLanguage");
            return string.IsNullOrEmpty(language) ? "tr" : language;
        }

        public void SetLanguage(string language)
        {
            _httpContextAccessor.HttpContext?.Session.SetString("CurrentLanguage", language);
        }

        public string GetLocalizedText(string trText, string enText)
        {
            var currentLang = GetCurrentLanguage();
            return currentLang == "en" && !string.IsNullOrEmpty(enText) ? enText : trText;
        }
    }
}
