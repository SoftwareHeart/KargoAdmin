// Models/ViewModels/ProfileViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace KargoAdmin.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Ad gereklidir")]
        [Display(Name = "Ad")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Soyad gereklidir")]
        [Display(Name = "Soyad")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }
    }
}