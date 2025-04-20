// HomeController.cs
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using KargoAdmin.Models;
using Microsoft.AspNetCore.Identity;
namespace KargoAdmin.Controllers
{
    public class HomeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public HomeController(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            // Kullanıcı giriş yapmışsa Admin sayfasına yönlendir
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Admin");
            }

            // Giriş yapmamışsa giriş sayfasına yönlendir
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}