using KargoAdmin.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace KargoAdmin.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileService> _logger;

        // Güvenlik ayarları
        private static readonly Dictionary<string, string[]> AllowedFileTypes = new()
        {
            ["image"] = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" },
            ["document"] = new[] { ".pdf", ".doc", ".docx", ".txt" },
            ["video"] = new[] { ".mp4", ".avi", ".mov", ".wmv" }
        };

        private static readonly Dictionary<string, string[]> AllowedMimeTypes = new()
        {
            ["image"] = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" },
            ["document"] = new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/plain" },
            ["video"] = new[] { "video/mp4", "video/avi", "video/quicktime", "video/x-ms-wmv" }
        };

        private static readonly Dictionary<string, byte[][]> FileSignatures = new()
        {
            [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            [".webp"] = new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } },
            [".gif"] = new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } }
        };

        private const long DefaultMaxFileSize = 10 * 1024 * 1024; // 10MB
        private static readonly Regex SafeFileNameRegex = new(@"[^a-zA-Z0-9._-]", RegexOptions.Compiled);

        public FileService(IWebHostEnvironment webHostEnvironment, ILogger<FileService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string category, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Boş dosya yükleme girişimi");
                throw new ArgumentException("Dosya seçilmedi veya boş.", nameof(file));
            }

            try
            {
                // Kategori validasyonu
                if (string.IsNullOrWhiteSpace(category))
                    category = "general";

                category = SanitizeFileName(category);

                // Dosya validasyonları
                await ValidateFileAsync(file, cancellationToken);

                // Upload klasörünü oluştur
                var uploadPath = await EnsureUploadDirectoryAsync(category, cancellationToken);

                // Güvenli dosya adı oluştur
                var fileName = await GenerateSecureFileNameAsync(file);
                var fullPath = Path.Combine(uploadPath, fileName);

                _logger.LogInformation("Dosya yükleniyor: {FileName} -> {FullPath}", file.FileName, fullPath);

                // Dosyayı async olarak kaydet
                await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                await file.CopyToAsync(fileStream, cancellationToken);
                await fileStream.FlushAsync(cancellationToken);

                // Başarı kontrolü
                if (!File.Exists(fullPath))
                {
                    _logger.LogError("Dosya kaydedildi ancak bulunamıyor: {FullPath}", fullPath);
                    throw new InvalidOperationException("Dosya kaydedilemedi.");
                }

                var fileInfo = new FileInfo(fullPath);
                var relativePath = Path.Combine("uploads", category, fileName).Replace("\\", "/");

                _logger.LogInformation("Dosya başarıyla yüklendi: {FileName}, Boyut: {Size} bytes, RelativePath: {RelativePath}",
                    fileName, fileInfo.Length, relativePath);

                return relativePath;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Dosya yükleme işlemi iptal edildi: {FileName}", file.FileName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya yükleme hatası: {FileName}", file.FileName);
                throw new InvalidOperationException($"Dosya yüklenemedi: {ex.Message}", ex);
            }
        }

        public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Task.CompletedTask;

            try
            {
                var fullPath = GetFullPath(filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    _logger.LogInformation("Dosya silindi: {FilePath}", fullPath);
                }
                else
                {
                    _logger.LogWarning("Silinecek dosya bulunamadı: {FilePath}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya silme hatası: {FilePath}", filePath);
                throw new InvalidOperationException($"Dosya silinemedi: {ex.Message}", ex);
            }

            return Task.CompletedTask;
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                var fullPath = GetFullPath(filePath);
                return File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }

        public long GetFileSize(string filePath)
        {
            if (!FileExists(filePath))
                return 0;

            try
            {
                var fullPath = GetFullPath(filePath);
                var fileInfo = new FileInfo(fullPath);
                return fileInfo.Length;
            }
            catch
            {
                return 0;
            }
        }

        public bool IsValidFileType(IFormFile file, string[] allowedTypes)
        {
            if (file == null || allowedTypes == null || allowedTypes.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) && allowedTypes.Contains(extension);
        }

        public bool IsValidFileSize(IFormFile file, long maxSizeBytes)
        {
            return file?.Length <= maxSizeBytes;
        }

        #region Private Helper Methods

        private async Task ValidateFileAsync(IFormFile file, CancellationToken cancellationToken)
        {
            // Boyut kontrolü
            if (!IsValidFileSize(file, DefaultMaxFileSize))
            {
                throw new InvalidOperationException($"Dosya boyutu {DefaultMaxFileSize / 1024 / 1024}MB'ı aşamaz.");
            }

            // Extension kontrolü
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                throw new InvalidOperationException("Dosya uzantısı belirlenemedi.");
            }

            // MIME type kontrolü
            var isValidMimeType = AllowedMimeTypes.Values
                .SelectMany(types => types)
                .Contains(file.ContentType);

            if (!isValidMimeType)
            {
                throw new InvalidOperationException($"Desteklenmeyen dosya türü: {file.ContentType}");
            }

            // Magic number kontrolü
            if (FileSignatures.ContainsKey(extension))
            {
                var isValidSignature = await ValidateFileSignatureAsync(file, extension, cancellationToken);
                if (!isValidSignature)
                {
                    throw new InvalidOperationException("Dosya içeriği geçersiz.");
                }
            }

            _logger.LogDebug("Dosya validasyonu başarılı: {FileName}", file.FileName);
        }

        private static async Task<bool> ValidateFileSignatureAsync(IFormFile file, string extension, CancellationToken cancellationToken)
        {
            if (!FileSignatures.ContainsKey(extension))
                return true;

            try
            {
                using var stream = file.OpenReadStream();
                var signatures = FileSignatures[extension];
                var headerBytes = new byte[signatures.Max(s => s.Length)];
                var bytesRead = await stream.ReadAsync(headerBytes, 0, headerBytes.Length, cancellationToken);

                return signatures.Any(signature =>
                {
                    if (bytesRead < signature.Length) return false;
                    return !signature.Where((t, i) => headerBytes[i] != t).Any();
                });
            }
            catch
            {
                return false;
            }
        }

        private async Task<string> EnsureUploadDirectoryAsync(string category, CancellationToken cancellationToken)
        {
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
            var categoryPath = Path.Combine(uploadsPath, category);

            try
            {
                if (!Directory.Exists(categoryPath))
                {
                    Directory.CreateDirectory(categoryPath);
                    _logger.LogInformation("Upload klasörü oluşturuldu: {CategoryPath}", categoryPath);

                    // .gitkeep dosyası oluştur (git için)
                    var gitKeepPath = Path.Combine(categoryPath, ".gitkeep");
                    await File.WriteAllTextAsync(gitKeepPath, "", cancellationToken);
                }

                return categoryPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload klasörü oluşturulamadı: {CategoryPath}", categoryPath);
                throw new InvalidOperationException($"Upload klasörü oluşturulamadı: {ex.Message}", ex);
            }
        }

        private async Task<string> GenerateSecureFileNameAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            var originalName = Path.GetFileNameWithoutExtension(file.FileName);

            // Güvenli dosya adı oluştur
            var safeName = SanitizeFileName(originalName);
            if (string.IsNullOrEmpty(safeName) || safeName.Length < 3)
            {
                safeName = "file";
            }

            // Benzersizlik için hash kullan
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{file.FileName}_{file.Length}"));
            var hash = Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();

            var fileName = $"{safeName}_{hash}{extension}";

            // Dosya adı uzunluk kontrolü
            if (fileName.Length > 100)
            {
                fileName = $"{hash}{extension}";
            }

            _logger.LogDebug("Güvenli dosya adı oluşturuldu: {OriginalName} -> {FileName}", file.FileName, fileName);
            return fileName;
        }

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "file";

            // Tehlikeli karakterleri temizle
            var sanitized = SafeFileNameRegex.Replace(fileName, "_");

            // Türkçe karakterleri değiştir
            sanitized = sanitized
                .Replace("ı", "i").Replace("İ", "I")
                .Replace("ş", "s").Replace("Ş", "S")
                .Replace("ğ", "g").Replace("Ğ", "G")
                .Replace("ü", "u").Replace("Ü", "U")
                .Replace("ö", "o").Replace("Ö", "O")
                .Replace("ç", "c").Replace("Ç", "C");

            // Çoklu alt çizgileri tek yapın
            sanitized = Regex.Replace(sanitized, @"_+", "_");

            // Baş ve sondaki alt çizgileri temizle
            sanitized = sanitized.Trim('_');

            return string.IsNullOrEmpty(sanitized) ? "file" : sanitized.ToLowerInvariant();
        }

        private string GetFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                throw new ArgumentException("Dosya yolu boş olamaz", nameof(relativePath));

            // Güvenlik: Directory traversal saldırılarına karşı
            var normalizedPath = Path.GetFullPath(Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/', '\\')));
            var webRootPath = Path.GetFullPath(_webHostEnvironment.WebRootPath);

            if (!normalizedPath.StartsWith(webRootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Dosya yolu güvenlik ihlali");
            }

            return normalizedPath;
        }

        #endregion
    }
}