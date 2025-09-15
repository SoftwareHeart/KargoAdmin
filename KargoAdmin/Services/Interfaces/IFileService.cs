using Microsoft.AspNetCore.Http;

namespace KargoAdmin.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Dosyayı belirtilen kategoride yükler
        /// </summary>
        /// <param name="file">Yüklenecek dosya</param>
        /// <param name="category">Dosya kategorisi (blog, user, etc.)</param>
        /// <param name="cancellationToken">İptal token'ı</param>
        /// <returns>Yüklenen dosyanın relatif yolu</returns>
        Task<string> UploadFileAsync(IFormFile file, string category, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dosyayı siler
        /// </summary>
        /// <param name="filePath">Silinecek dosyanın yolu</param>
        /// <param name="cancellationToken">İptal token'ı</param>
        Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dosyanın var olup olmadığını kontrol eder
        /// </summary>
        /// <param name="filePath">Kontrol edilecek dosya yolu</param>
        /// <returns>Dosya varsa true, yoksa false</returns>
        bool FileExists(string filePath);

        /// <summary>
        /// Dosya boyutunu döner
        /// </summary>
        /// <param name="filePath">Dosya yolu</param>
        /// <returns>Dosya boyutu (bytes)</returns>
        long GetFileSize(string filePath);

        /// <summary>
        /// Desteklenen dosya formatlarını kontrol eder
        /// </summary>
        /// <param name="file">Kontrol edilecek dosya</param>
        /// <param name="allowedTypes">İzin verilen dosya türleri</param>
        /// <returns>Dosya geçerliyse true</returns>
        bool IsValidFileType(IFormFile file, string[] allowedTypes);

        /// <summary>
        /// Dosya boyutu limitini kontrol eder
        /// </summary>
        /// <param name="file">Kontrol edilecek dosya</param>
        /// <param name="maxSizeBytes">Maksimum boyut (bytes)</param>
        /// <returns>Dosya boyutu uygunsa true</returns>
        bool IsValidFileSize(IFormFile file, long maxSizeBytes);
    }
}