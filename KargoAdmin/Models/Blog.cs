using System.ComponentModel.DataAnnotations;

namespace KargoAdmin.Models
{
    public class Blog
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        // İngilizce alanlar
        public string? TitleEn { get; set; }
        public string? ContentEn { get; set; }

        public string? ImageUrl { get; set; }

        public DateTime PublishDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public bool IsPublished { get; set; }

        public string AuthorId { get; set; }

        public ApplicationUser Author { get; set; }

        // Yeni alanlar

        // SEO için meta açıklaması
        [StringLength(160)]
        public string? MetaDescription { get; set; }

        // İngilizce meta açıklaması
        [StringLength(160)]
        public string? MetaDescriptionEn { get; set; }

        // Etiketler (virgülle ayrılmış)
        public string? Tags { get; set; }

        // İngilizce etiketler
        public string? TagsEn { get; set; }

        // Görüntülenme sayısı
        public int ViewCount { get; set; } = 0;

        // SEO dostu URL için slug
        public string? Slug { get; set; }

        // İngilizce slug
        public string? SlugEn { get; set; }

        [StringLength(20)]
        public string Type { get; set; } = "Faydalı Bilgi";

        // Dil durumu
        public bool HasEnglishContent => !string.IsNullOrEmpty(TitleEn) && !string.IsNullOrEmpty(ContentEn);
    }
}