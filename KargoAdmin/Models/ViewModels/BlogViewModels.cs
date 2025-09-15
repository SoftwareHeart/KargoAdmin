using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace KargoAdmin.Models.ViewModels
{
    public class BlogCreateViewModel
    {
        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(100, ErrorMessage = "Başlık en fazla {1} karakter olabilir")]
        [Display(Name = "Başlık")]
        public string Title { get; set; }

        [StringLength(100, ErrorMessage = "İngilizce başlık en fazla {1} karakter olabilir")]
        [Display(Name = "İngilizce Başlık")]
        public string? TitleEn { get; set; }

        [Required(ErrorMessage = "İçerik gereklidir")]
        [Display(Name = "İçerik")]
        public string Content { get; set; }

        [Display(Name = "İngilizce İçerik")]
        public string? ContentEn { get; set; }

        [Display(Name = "Kapak Görseli")]
		public IFormFile? ImageFile { get; set; }

        [Display(Name = "Yayınla")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "Meta Açıklama")]
        [StringLength(160, ErrorMessage = "Meta açıklama en fazla {1} karakter olabilir")]
        public string MetaDescription { get; set; } = "";  // Boş string ata

        [Display(Name = "İngilizce Meta Açıklama")]
        [StringLength(160, ErrorMessage = "İngilizce meta açıklama en fazla {1} karakter olabilir")]
        public string? MetaDescriptionEn { get; set; }

        [Display(Name = "Etiketler")]
        public string Tags { get; set; } = ""; // Boş string ata

        [Display(Name = "İngilizce Etiketler")]
        public string? TagsEn { get; set; }

        [Required(ErrorMessage = "İçerik türü seçilmelidir")]
        [Display(Name = "İçerik Türü")]
        public string Type { get; set; } = "Faydalı Bilgi"; // Default: Faydalı Bilgi
    }

    public class BlogEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık gereklidir")]
        [StringLength(100, ErrorMessage = "Başlık en fazla {1} karakter olabilir")]
        [Display(Name = "Başlık")]
        public string Title { get; set; }

        [StringLength(100, ErrorMessage = "İngilizce başlık en fazla {1} karakter olabilir")]
        [Display(Name = "İngilizce Başlık")]
        public string? TitleEn { get; set; }

        [Required(ErrorMessage = "İçerik gereklidir")]
        [Display(Name = "İçerik")]
        public string Content { get; set; }

        [Display(Name = "İngilizce İçerik")]
        public string? ContentEn { get; set; }

        [Display(Name = "Yeni Kapak Görseli")]
        public IFormFile? ImageFile { get; set; }

        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Yayınla")]
        public bool IsPublished { get; set; }

        [Display(Name = "Meta Açıklama")]
        [StringLength(160, ErrorMessage = "Meta açıklama en fazla {1} karakter olabilir")]
        public string? MetaDescription { get; set; }

        [Display(Name = "İngilizce Meta Açıklama")]
        [StringLength(160, ErrorMessage = "İngilizce meta açıklama en fazla {1} karakter olabilir")]
        public string? MetaDescriptionEn { get; set; }

        [Display(Name = "Etiketler")]
        public string? Tags { get; set; } = "";

        [Display(Name = "İngilizce Etiketler")]
        public string? TagsEn { get; set; }

        [Required(ErrorMessage = "İçerik türü seçilmelidir")]
        [Display(Name = "İçerik Türü")]
        public string Type { get; set; } = "Faydalı Bilgi"; // Default: Faydalı Bilgi
    }
}