// Blog Cards JavaScript - Temiz ve Modern Kod
document.addEventListener('DOMContentLoaded', function () {

    // CSS stillerini head'e ekle
    addShareMenuStyles();

    // Paylaşım butonlarını başlat
    initializeShareButtons();

    // Scroll animasyonlarını başlat
    initializeScrollAnimations();

});

// CSS stillerini ekle
function addShareMenuStyles() {
    const styleId = 'share-menu-styles';

    // Eğer daha önce eklenmişse tekrar ekleme
    if (document.getElementById(styleId)) {
        return;
    }

    const style = document.createElement('style');
    style.id = styleId;
    style.textContent = `
        .share-menu {
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: rgba(0, 0, 0, 0.6);
            display: flex;
            align-items: center;
            justify-content: center;
            z-index: 1000;
            opacity: 0;
            transition: opacity 0.3s ease;
            backdrop-filter: blur(4px);
        }

        .share-menu.active {
            opacity: 1;
        }

        .share-menu-content {
            background: white;
            border-radius: 20px;
            padding: 2rem;
            max-width: 320px;
            width: 90%;
            position: relative;
            transform: scale(0.8) translateY(20px);
            transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
            box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
        }

        .share-menu.active .share-menu-content {
            transform: scale(1) translateY(0);
        }

        .share-menu h4 {
            margin: 0 0 1.5rem 0;
            text-align: center;
            color: #1e293b;
            font-weight: 600;
            font-size: 1.2rem;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 0.5rem;
        }

        .share-options {
            display: flex;
            flex-direction: column;
            gap: 0.75rem;
        }

        .share-option {
            display: flex;
            align-items: center;
            gap: 1rem;
            padding: 1rem;
            border-radius: 12px;
            text-decoration: none;
            transition: all 0.3s ease;
            font-weight: 500;
            border: 1px solid transparent;
        }

        .share-option:hover {
            background: #f8fafc;
            border-color: currentColor;
            transform: translateX(4px);
        }

        .share-option i {
            font-size: 1.2rem;
            width: 20px;
            text-align: center;
        }

        .close-share-menu {
            position: absolute;
            top: 1rem;
            right: 1rem;
            background: #f1f5f9;
            border: none;
            font-size: 1.2rem;
            color: #64748b;
            cursor: pointer;
            transition: all 0.3s ease;
            width: 32px;
            height: 32px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .close-share-menu:hover {
            background: #e2e8f0;
            color: #475569;
            transform: scale(1.1);
        }

        @media (max-width: 480px) {
            .share-menu-content {
                padding: 1.5rem;
                max-width: 280px;
            }
            
            .share-option {
                padding: 0.75rem;
            }
        }
    `;

    document.head.appendChild(style);
}

// Paylaşım butonlarını başlat
function initializeShareButtons() {
    const shareBtns = document.querySelectorAll('.share-btn');

    shareBtns.forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            showShareMenu(this);
        });
    });
}

// Paylaşım menüsünü göster
function showShareMenu(button) {
    // Blog kartını ve bilgilerini bul
    const blogCard = button.closest('.modern-blog-card') || button.closest('.blog-card');
    if (!blogCard) return;

    const titleElement = blogCard.querySelector('.blog-title-modern a') ||
        blogCard.querySelector('.blog-card-title a') ||
        blogCard.querySelector('.blog-card-title');

    const linkElement = blogCard.querySelector('.read-more-modern') ||
        blogCard.querySelector('.blog-read-more') ||
        blogCard.querySelector('.blog-title-modern a');

    if (!titleElement || !linkElement) return;

    const blogTitle = titleElement.textContent.trim();
    const blogUrl = linkElement.href || window.location.href;

    // Paylaşım seçenekleri
    const shareOptions = [
        {
            name: 'Twitter',
            url: `https://twitter.com/intent/tweet?text=${encodeURIComponent(blogTitle)}&url=${encodeURIComponent(blogUrl)}`,
            icon: 'fab fa-twitter',
            color: '#1da1f2'
        },
        {
            name: 'Facebook',
            url: `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(blogUrl)}`,
            icon: 'fab fa-facebook',
            color: '#4267b2'
        },
        {
            name: 'LinkedIn',
            url: `https://www.linkedin.com/sharing/share-offsite/?url=${encodeURIComponent(blogUrl)}`,
            icon: 'fab fa-linkedin',
            color: '#0077b5'
        },
        {
            name: 'WhatsApp',
            url: `https://wa.me/?text=${encodeURIComponent(blogTitle + ' ' + blogUrl)}`,
            icon: 'fab fa-whatsapp',
            color: '#25d366'
        },
        {
            name: 'Linki Kopyala',
            action: 'copy',
            icon: 'fas fa-link',
            color: '#64748b'
        }
    ];

    // Modal oluştur
    const shareMenu = createShareMenu(shareOptions, blogUrl, blogTitle);
    document.body.appendChild(shareMenu);

    // Modal'ı aktive et
    setTimeout(() => shareMenu.classList.add('active'), 10);

    // Event listener'ları ekle
    setupShareMenuEvents(shareMenu, blogUrl);
}

// Paylaşım menüsü HTML'ini oluştur
function createShareMenu(shareOptions, blogUrl, blogTitle) {
    const shareMenu = document.createElement('div');
    shareMenu.className = 'share-menu';
    shareMenu.innerHTML = `
        <div class="share-menu-content">
            <h4>
                <i class="fas fa-share-alt"></i> 
                Paylaş
            </h4>
            <div class="share-options">
                ${shareOptions.map(option => `
                    <a href="${option.url || '#'}" 
                       class="share-option" 
                       data-action="${option.action || 'share'}"
                       data-url="${blogUrl}"
                       data-title="${blogTitle}"
                       style="color: ${option.color}"
                       target="_blank"
                       rel="noopener noreferrer">
                        <i class="${option.icon}"></i>
                        <span>${option.name}</span>
                    </a>
                `).join('')}
            </div>
            <button class="close-share-menu" aria-label="Kapat">
                ✕
            </button>
        </div>
    `;

    return shareMenu;
}

// Paylaşım menüsü event listener'ları
function setupShareMenuEvents(shareMenu, blogUrl) {
    // Kapat butonu
    const closeBtn = shareMenu.querySelector('.close-share-menu');
    closeBtn.addEventListener('click', () => closeShareMenu(shareMenu));

    // Link kopyala
    const copyBtn = shareMenu.querySelector('[data-action="copy"]');
    copyBtn.addEventListener('click', function (e) {
        e.preventDefault();
        copyToClipboard(blogUrl, this, shareMenu);
    });

    // Dışarı tıklama ile kapat
    shareMenu.addEventListener('click', function (e) {
        if (e.target === this) {
            closeShareMenu(shareMenu);
        }
    });

    // ESC tuşu ile kapat
    const escHandler = function (e) {
        if (e.key === 'Escape') {
            closeShareMenu(shareMenu);
            document.removeEventListener('keydown', escHandler);
        }
    };
    document.addEventListener('keydown', escHandler);
}

// Link'i clipboard'a kopyala
function copyToClipboard(url, button, shareMenu) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
        navigator.clipboard.writeText(url)
            .then(() => {
                showCopySuccess(button, shareMenu);
            })
            .catch(() => {
                showCopyError(button);
            });
    } else {
        // Fallback for older browsers
        try {
            const textArea = document.createElement('textarea');
            textArea.value = url;
            textArea.style.position = 'fixed';
            textArea.style.opacity = '0';
            document.body.appendChild(textArea);
            textArea.select();
            document.execCommand('copy');
            document.body.removeChild(textArea);
            showCopySuccess(button, shareMenu);
        } catch (err) {
            showCopyError(button);
        }
    }
}

// Kopyalama başarılı
function showCopySuccess(button, shareMenu) {
    button.innerHTML = '<i class="fas fa-check"></i><span>Kopyalandı!</span>';
    button.style.color = '#10b981';

    setTimeout(() => {
        closeShareMenu(shareMenu);
    }, 1000);
}

// Kopyalama hatası
function showCopyError(button) {
    button.innerHTML = '<i class="fas fa-exclamation"></i><span>Kopyalanamadı</span>';
    button.style.color = '#ef4444';

    setTimeout(() => {
        button.innerHTML = '<i class="fas fa-link"></i><span>Linki Kopyala</span>';
        button.style.color = '#64748b';
    }, 2000);
}

// Paylaşım menüsünü kapat
function closeShareMenu(shareMenu) {
    shareMenu.classList.remove('active');

    setTimeout(() => {
        if (shareMenu.parentNode) {
            shareMenu.parentNode.removeChild(shareMenu);
        }
    }, 300);
}

// Scroll animasyonlarını başlat
function initializeScrollAnimations() {
    // Intersection Observer ayarları
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    // Blog kartlarına animasyon uygula
    const blogCards = document.querySelectorAll('.modern-blog-card, .blog-card');
    blogCards.forEach((card, index) => {
        // Başlangıç stillerini ayarla
        card.style.opacity = '0';
        card.style.transform = 'translateY(30px)';
        card.style.transition = `opacity 0.6s ease ${index * 0.1}s, transform 0.6s ease ${index * 0.1}s`;

        // Observer'a ekle
        observer.observe(card);
    });
}

// Debug fonksiyonu (geliştirme amaçlı)
function debugBlogCards() {
    console.log('Blog Cards Debug Info:');
    console.log('Share buttons found:', document.querySelectorAll('.share-btn').length);
    console.log('Blog cards found:', document.querySelectorAll('.modern-blog-card, .blog-card').length);
    console.log('CSS styles loaded:', !!document.getElementById('share-menu-styles'));
}