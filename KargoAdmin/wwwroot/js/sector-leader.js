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

    // Intersection Observer for animations
    const observerOptions = {
        threshold: 0.3,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const section = entry.target;
                console.log('Section görüldü, animasyonlar başlıyor');

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
        const counters = section.querySelectorAll('.stats-box');
        console.log('Counter animasyonu başlıyor, box sayısı:', counters.length);

        counters.forEach((counter, index) => {
            const target = parseInt(counter.getAttribute('data-target'));
            const numberElement = counter.querySelector('.stats-number');
            const duration = 2000; // 2 seconds
            const increment = target / (duration / 16); // 60fps
            let current = 0;

            console.log(`Counter ${index} hedef: ${target}`);

            const timer = setInterval(() => {
                current += increment;
                if (current >= target) {
                    current = target;
                    clearInterval(timer);
                }
                numberElement.textContent = Math.floor(current) + '+';
            }, 16);
        });
    }

    // Optional: Add hover effects for stats boxes - TEMİZLENMİŞ
    const statsBoxes = document.querySelectorAll('.stats-box');
    statsBoxes.forEach(box => {
        // Önce tüm inline style'ları temizle
        box.style.removeProperty('background');
        box.style.removeProperty('background-color');
        box.style.removeProperty('transform');

        box.addEventListener('mouseenter', function () {
            // Sadece transform değiştir, background dokunma
            this.style.transform = 'translateX(0) translateY(5px) scale(1.05)';
        });

        box.addEventListener('mouseleave', function () {
            // Mevcut durumu koru, background'a dokunma
            if (this.classList.contains('loaded')) {
                this.style.transform = 'translateX(0) translateY(10px) scale(1)';
            } else if (this.classList.contains('drop')) {
                this.style.transform = 'translateX(0) translateY(10px) scale(1)';
            } else {
                this.style.transform = 'translateX(0) translateY(0) scale(1)';
            }
        });
    });
});