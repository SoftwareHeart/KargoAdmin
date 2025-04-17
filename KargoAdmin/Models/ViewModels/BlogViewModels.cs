using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KargoAdmin.Models.ViewModels
{
    public class BlogCreateViewModel
    {
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; }

        [Required(ErrorMessage = "İçerik gereklidir")]
        public string Content { get; set; }

        [Display(Name = "Resim")]
        public IFormFile ImageFile { get; set; }

        [Display(Name = "Yayınla")]
        public bool IsPublished { get; set; }
    }

    public class BlogEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; }

        [Required(ErrorMessage = "İçerik gereklidir")]
        public string Content { get; set; }

        [Display(Name = "Yeni Resim")]
        // Required özniteliğini kaldırdık
        public IFormFile ImageFile { get; set; }

        public string ExistingImageUrl { get; set; }

        [Display(Name = "Yayınla")]
        public bool IsPublished { get; set; }
    }
}