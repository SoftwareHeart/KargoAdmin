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

        // Etiketler (virgülle ayrılmış)
        public string? Tags { get; set; }

        // Görüntülenme sayısı
        public int ViewCount { get; set; } = 0;

        // SEO dostu URL için slug
        public string? Slug { get; set; }

        [StringLength(20)]
        public string Type { get; set; } = "Haber";
    }
}