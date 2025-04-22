using System.ComponentModel.DataAnnotations;

namespace KargoAdmin.Models.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "İsim gereklidir")]
        [Display(Name = "Adınız")]
        public string Name { get; set; }

        [Required(ErrorMessage = "E-posta adresi gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon numarası gereklidir")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Konu gereklidir")]
        [Display(Name = "Konu")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "Mesaj gereklidir")]
        [StringLength(1000, ErrorMessage = "Mesaj en fazla {1} karakter olabilir")]
        [Display(Name = "Mesajınız")]
        public string Message { get; set; }
    }
}