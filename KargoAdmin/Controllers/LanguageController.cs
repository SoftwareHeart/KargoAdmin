using KargoAdmin.Services;
using Microsoft.AspNetCore.Mvc;

namespace KargoAdmin.Controllers
{
    public class LanguageController : Controller
    {
        private readonly ILanguageService _languageService;

        public LanguageController(ILanguageService languageService)
        {
            _languageService = languageService;
        }

        [HttpPost]
        public IActionResult ChangeLanguage(string language, string returnUrl = null)
        {
            if (language == "tr" || language == "en")
            {
                _languageService.SetLanguage(language);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Aleris");
        }

        [HttpGet]
        public IActionResult GetCurrentLanguage()
        {
            return Json(new { language = _languageService.GetCurrentLanguage() });
        }
    }
}
