// Sector Leader Section JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // ÖNCE TÜM STATS BOXLARIN INLINE STYLE'LARINI TEMİZLE
    const allStatsBoxes = document.querySelectorAll('.stats-box');
    allStatsBoxes.forEach(box => {
        box.style.removeProperty('background');
        box.style.removeProperty('background-color');
        box.style.removeProperty('backgroundColor');
        console.log('Stats box temizlendi:', box);
    });

    // Intersection Observer for animations - Services section ile aynı yapı
    const observerOptions = {
        threshold: 0.3,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const section = entry.target;

                // Section'a animate-in class'ı ekle (video için)
                section.classList.add('animate-in');
                console.log('Sector Leader section görüldü, animasyonlar başlıyor');

                // Counter animation
                animateCounters(section);

                // Stats box drop animation after initial slide-in (3 saniye sonra)
                setTimeout(() => {
                    const statsBoxes = section.querySelectorAll('.stats-box');
                    console.log('Drop animasyonu başlıyor, bulunan box sayısı:', statsBoxes.length);

                    statsBoxes.forEach((box, index) => {
                        setTimeout(() => {
                            console.log('Box', index, 'drop class ekleniyor');
                            box.classList.add('drop');

                            // Add floating animation after drop (1 saniye sonra)
                            setTimeout(() => {
                                console.log('Box', index, 'loaded class ekleniyor');
                                box.classList.add('loaded');
                            }, 1000);
                        }, index * 200);
                    });
                }, 3000);

                // Unobserve after animation is triggered
                observer.unobserve(section);
            }
        });
    }, observerOptions);

    // Observe the sector leader section
    const sectorLeaderSection = document.querySelector('.sector-leader-section');
    if (sectorLeaderSection) {
        console.log('Sector leader section bulundu, observer ekleniyor');
        observer.observe(sectorLeaderSection);
    } else {
        console.log('Sector leader section bulunamadı!');
    }

    // Counter animation function
    function animateCounters(section) {
        const statsBoxes = section.querySelectorAll('.stats-box');

        statsBoxes.forEach(box => {
            const target = parseInt(box.getAttribute('data-target'));
            const counterElement = box.querySelector('.stats-number');

            if (target && counterElement) {
                const increment = target / 120; // 120 frame'de tamamla (yaklaşık 2 saniye)
                let current = 0;

                const updateCounter = () => {
                    if (current < target) {
                        current += increment;
                        if (current > target) current = target;
                        counterElement.textContent = Math.floor(current) + '+';
                        requestAnimationFrame(updateCounter);
                    } else {
                        counterElement.textContent = target + '+';
                    }
                };

                // Animasyonu başlat
                setTimeout(() => {
                    updateCounter();
                }, 500);
            }
        });
    }
});

// Hover effects for stats boxes
document.addEventListener('DOMContentLoaded', function () {
    const statsBoxes = document.querySelectorAll('.stats-box');

    statsBoxes.forEach(box => {
        box.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-10px) scale(1.05)';
            this.style.filter = 'brightness(1.1)';
        });

        box.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0) scale(1)';
            this.style.filter = 'brightness(1)';
        });
    });
});