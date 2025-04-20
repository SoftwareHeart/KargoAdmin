// Models/ViewModels/SettingsViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace KargoAdmin.Models.ViewModels
{
    public class SettingsViewModel
    {
        [Required(ErrorMessage = "Site başlığı gereklidir")]
        [Display(Name = "Site Başlığı")]
        public string SiteTitle { get; set; }

        // Bu alanlar isteğe bağlı olsun
        [Display(Name = "Site Açıklaması")]
        public string? SiteDescription { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "İletişim E-posta")]
        public string? ContactEmail { get; set; }

        [Display(Name = "İletişim Telefon")]
        public string? ContactPhone { get; set; }

        [Display(Name = "Facebook URL")]
        public string? FacebookUrl { get; set; }

        [Display(Name = "Twitter URL")]
        public string? TwitterUrl { get; set; }

        [Display(Name = "Instagram URL")]
        public string? InstagramUrl { get; set; }
    }
}