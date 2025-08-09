/*!
 * Blog Table Manager - Güvenli Arama ve Navigasyon
 * Admin Blog Yönetimi için arama sonrasında doğru ID'lere tıklama sorunu çözümü
 */
class BlogTableManager {
    constructor() {
        this.searchInput = document.getElementById('tableSearch');
        this.clearButton = document.getElementById('clearSearch');
        this.tableBody = document.getElementById('tableBody');
        this.noResults = document.getElementById('noResults');
        this.allRows = [];
        this.originalUrls = new Map(); // Orijinal URL'leri saklamak için

        this.init();
    }

    init() {
        // Tüm satırları ve orijinal URL'leri kaydet
        this.allRows = Array.from(this.tableBody.querySelectorAll('tr[data-blog-id]'));
        this.storeOriginalUrls();

        // Event listener'ları ekle
        this.searchInput?.addEventListener('input', (e) => this.handleSearch(e.target.value));
        this.clearButton?.addEventListener('click', () => this.clearSearch());

        // Blog linklerine güvenli event listener'ları ekle
        this.attachSafeBlogLinks();

        console.log('Blog Table Manager initialized with', this.allRows.length, 'rows');
    }

    storeOriginalUrls() {
        // Her blog ID'si için orijinal URL'leri sakla
        this.allRows.forEach(row => {
            const blogId = row.getAttribute('data-blog-id');

            const detailsLink = row.querySelector('a[href*="Details"]');
            const editLink = row.querySelector('a[href*="Edit"]');
            const deleteLink = row.querySelector('a[href*="Delete"]');

            this.originalUrls.set(`${blogId}_details`, detailsLink?.href || '');
            this.originalUrls.set(`${blogId}_edit`, editLink?.href || '');
            this.originalUrls.set(`${blogId}_delete`, deleteLink?.href || '');
        });
    }

    handleSearch(searchTerm) {
        const term = searchTerm.toLowerCase().trim();

        // Clear button'u göster/gizle
        if (this.clearButton) {
            this.clearButton.style.display = term.length > 0 ? 'block' : 'none';
        }

        if (term === '') {
            this.showAllRows();
            return;
        }

        let visibleCount = 0;

        this.allRows.forEach(row => {
            const isMatch = this.isRowMatching(row, term);

            if (isMatch) {
                row.style.display = '';
                visibleCount++;
            } else {
                row.style.display = 'none';
            }
        });

        // Sonuç bulunamadı mesajını göster/gizle
        if (this.noResults) {
            if (visibleCount === 0) {
                this.noResults.classList.remove('d-none');
            } else {
                this.noResults.classList.add('d-none');
            }
        }

        console.log(`Search: "${term}" - ${visibleCount} results found`);
    }

    isRowMatching(row, term) {
        const blogId = row.getAttribute('data-blog-id');
        const titleElement = row.querySelector('.blog-title-link, a[href*="Details"]');
        const authorElement = row.querySelector('.author-cell, td:nth-child(3)');
        const dateElement = row.querySelector('.date-cell, td:nth-child(4)');

        const title = titleElement?.textContent?.toLowerCase() || '';
        const author = authorElement?.textContent?.toLowerCase() || '';
        const date = dateElement?.textContent?.toLowerCase() || '';

        return title.includes(term) ||
            author.includes(term) ||
            date.includes(term) ||
            blogId === term;
    }

    showAllRows() {
        this.allRows.forEach(row => {
            row.style.display = '';
        });

        if (this.noResults) {
            this.noResults.classList.add('d-none');
        }
    }

    clearSearch() {
        if (this.searchInput) {
            this.searchInput.value = '';
        }

        if (this.clearButton) {
            this.clearButton.style.display = 'none';
        }

        this.showAllRows();
        this.searchInput?.focus();
    }

    attachSafeBlogLinks() {
        // Her satırdaki linklere güvenli event listener'lar ekle
        this.allRows.forEach(row => {
            const blogId = row.getAttribute('data-blog-id');

            if (!blogId) {
                console.warn('Blog ID not found for row:', row);
                return;
            }

            // Tüm linkleri bul ve güvenli hale getir
            const allLinks = row.querySelectorAll('a[href]');

            allLinks.forEach(link => {
                // Orijinal href'i data-original-href olarak sakla
                if (!link.getAttribute('data-original-href')) {
                    link.setAttribute('data-original-href', link.href);
                }

                // Click event'ini güvenli şekilde ekle
                link.addEventListener('click', (e) => {
                    e.preventDefault();
                    e.stopPropagation();

                    const currentBlogId = row.getAttribute('data-blog-id');
                    this.handleSafeBlogClick(link, currentBlogId);
                });
            });
        });
    }

    handleSafeBlogClick(linkElement, blogId) {
        const originalHref = linkElement.getAttribute('data-original-href') || linkElement.href;

        console.log(`Safe blog click - Blog ID: ${blogId}, Original href: ${originalHref}`);

        // URL'yi ID ile yeniden oluştur (güvenlik için)
        let targetUrl = '';

        if (originalHref.includes('Details')) {
            targetUrl = `/Admin/Blog/Details/${blogId}`;
        } else if (originalHref.includes('Edit')) {
            targetUrl = `/Admin/Blog/Edit/${blogId}`;
        } else if (originalHref.includes('Delete')) {
            targetUrl = `/Admin/Blog/Delete/${blogId}`;
        } else {
            // Varsayılan olarak details sayfasına git
            targetUrl = `/Admin/Blog/Details/${blogId}`;
        }

        console.log(`Navigating to: ${targetUrl}`);

        // Delete işlemi için onay iste
        if (targetUrl.includes('Delete')) {
            if (confirm(`${blogId} ID'li blog yazısını silmek istediğinize emin misiniz?`)) {
                window.location.href = targetUrl;
            }
        } else {
            window.location.href = targetUrl;
        }
    }

    // Debugging için yararlı metod
    debugRowInfo(row) {
        const blogId = row.getAttribute('data-blog-id');
        const title = row.querySelector('.blog-title-link, a[href*="Details"]')?.textContent;
        const isVisible = row.style.display !== 'none';

        console.log(`Row Debug - ID: ${blogId}, Title: ${title}, Visible: ${isVisible}`);

        return {
            blogId,
            title,
            isVisible,
            row
        };
    }

    // Tüm satırları debug etmek için
    debugAllRows() {
        console.log('=== Blog Table Debug Info ===');
        this.allRows.forEach((row, index) => {
            console.log(`Row ${index}:`, this.debugRowInfo(row));
        });
        console.log('============================');
    }

    // Event listener'ları temizlemek için
    destroy() {
        this.searchInput?.removeEventListener('input', this.handleSearch);
        this.clearButton?.removeEventListener('click', this.clearSearch);

        // Tüm link event listener'larını temizle
        this.allRows.forEach(row => {
            const links = row.querySelectorAll('a[href]');
            links.forEach(link => {
                link.replaceWith(link.cloneNode(true));
            });
        });

        console.log('Blog Table Manager destroyed');
    }
}

// Global değişken olarak blog table manager'ı saklayalım
let globalBlogTableManager = null;

// Sayfa yüklendiğinde otomatik başlatma
document.addEventListener('DOMContentLoaded', function () {
    // Eğer blog tablosu varsa manager'ı başlat
    const tableBody = document.getElementById('tableBody');
    if (tableBody) {
        globalBlogTableManager = new BlogTableManager();

        // Bootstrap tooltip'lerini aktif et
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });

        // Tablo satırlarına hover efekti
        const tableRows = document.querySelectorAll('#blogsTable tbody tr');
        tableRows.forEach(row => {
            row.addEventListener('mouseenter', function () {
                this.style.backgroundColor = '#f8f9fa';
            });

            row.addEventListener('mouseleave', function () {
                this.style.backgroundColor = '';
            });
        });

        // Debug için global fonksiyonlar (geliştirme aşamasında kullanılabilir)
        window.debugBlogTable = () => globalBlogTableManager?.debugAllRows();
        window.clearBlogSearch = () => globalBlogTableManager?.clearSearch();
    }
});

// Sayfa ayrılırken temizlik yap
window.addEventListener('beforeunload', function () {
    if (globalBlogTableManager) {
        globalBlogTableManager.destroy();
        globalBlogTableManager = null;
    }
});

// Sayfa geri geldiğinde (browser back button) yeniden başlat
window.addEventListener('pageshow', function (event) {
    if (event.persisted && !globalBlogTableManager) {
        // Sayfa cache'den geliyorsa yeniden başlat
        const tableBody = document.getElementById('tableBody');
        if (tableBody) {
            globalBlogTableManager = new BlogTableManager();
        }
    }
});

// Export etme (modül sistemi kullanılıyorsa)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = BlogTableManager;
}

// AMD desteği
if (typeof define === 'function' && define.amd) {
    define([], function () {
        return BlogTableManager;
    });
}