/**
 * Forklift Animation - Mobile Responsive
 * Aleris Global - Responsive forklift movement animation
 * Version: Mobile Optimized 2.0
 */

(function () {
    'use strict';

    function initForkliftAnimation() {
        const section = document.getElementById('services-section-land-van');
        if (!section) return;

        const forklift = section.querySelector('.land-van-forklift');
        if (!forklift) return;

        const cabin = forklift.querySelector('.forklift-cabin');
        const tip = forklift.querySelector('.forklift-tip');
        const pallet = forklift.querySelector('.forklift-pallet');

        if (!cabin || !tip || !pallet) return;

        // Mobile responsive configuration
        function getResponsiveConfig() {
            const screenWidth = window.innerWidth;
            const isMobile = screenWidth <= 768;
            const isSmallMobile = screenWidth <= 480;
            const isVerySmallMobile = screenWidth <= 375;

            if (isVerySmallMobile) {
                return {
                    startDistance: 120,
                    exitDistance: -350,
                    animationDuration: 1.2,
                    exitDuration: 1.0,
                    palletEnlargeAmount: 30,
                    dropOffset: 15,
                    timing: {
                        startDelay: 200,
                        palletDropDelay: 1800,
                        exitDelay: 2200
                    }
                };
            } else if (isSmallMobile) {
                return {
                    startDistance: 150,
                    exitDistance: -400,
                    animationDuration: 1.4,
                    exitDuration: 1.1,
                    palletEnlargeAmount: 40,
                    dropOffset: 18,
                    timing: {
                        startDelay: 250,
                        palletDropDelay: 2000,
                        exitDelay: 2400
                    }
                };
            } else if (isMobile) {
                return {
                    startDistance: 200,
                    exitDistance: -500,
                    animationDuration: 1.6,
                    exitDuration: 1.3,
                    palletEnlargeAmount: 50,
                    dropOffset: 20,
                    timing: {
                        startDelay: 300,
                        palletDropDelay: 2200,
                        exitDelay: 2600
                    }
                };
            } else {
                return {
                    startDistance: 300,
                    exitDistance: -800,
                    animationDuration: 2.0,
                    exitDuration: 1.5,
                    palletEnlargeAmount: 75,
                    dropOffset: 20,
                    timing: {
                        startDelay: 500,
                        palletDropDelay: 2800,
                        exitDelay: 3200
                    }
                };
            }
        }

        function runAnimation() {
            const config = getResponsiveConfig();

            // 1. Sağdan başla
            forklift.style.transform = `translateX(${config.startDistance}px)`;
            cabin.style.opacity = '1';
            tip.style.opacity = '1';
            pallet.style.opacity = '1';

            // 2. Merkeze gel
            setTimeout(() => {
                forklift.style.transition = `transform ${config.animationDuration}s ease-in-out`;
                forklift.style.transform = 'translateX(0px)';
            }, config.timing.startDelay);

            // 3. Koliyi bırak (pallet'i section'a taşı) - Güzel animasyonla
            setTimeout(() => {
                // Pallet'in current pozisyonunu ve boyutunu al
                const palletRect = pallet.getBoundingClientRect();
                const sectionRect = section.getBoundingClientRect();
                const computedStyle = window.getComputedStyle(pallet);

                // Pallet'i section'a clone olarak ekle
                const palletClone = pallet.cloneNode(true);
                palletClone.style.position = 'absolute';
                palletClone.style.left = (palletRect.left - sectionRect.left) + 'px';
                palletClone.style.top = (palletRect.top - sectionRect.top + config.dropOffset) + 'px';

                const currentWidthPx = parseFloat(computedStyle.width) || palletRect.width;
                const currentHeightPx = parseFloat(computedStyle.height) || palletRect.height;
                const enlargedWidthPx = currentWidthPx + config.palletEnlargeAmount;
                const enlargedHeightPx = currentWidthPx > 0 && currentHeightPx > 0
                    ? (currentHeightPx / currentWidthPx) * enlargedWidthPx
                    : currentHeightPx;

                palletClone.style.width = enlargedWidthPx + 'px';
                palletClone.style.height = isNaN(enlargedHeightPx) ? 'auto' : (enlargedHeightPx + 'px');
                palletClone.style.transform = 'scale(1.05)';
                palletClone.style.transition = 'all 0.7s cubic-bezier(0.34, 1.56, 0.64, 1)';
                palletClone.style.zIndex = '10';
                palletClone.style.opacity = '1';
                section.appendChild(palletClone);

                // Forklift'teki pallet'i gizle
                pallet.style.opacity = '0';

                // Koli bırakma animasyonu - yerçekimi efekti
                setTimeout(() => {
                    palletClone.style.top = (palletRect.top - sectionRect.top - config.dropOffset) + 'px';
                    palletClone.style.transform = 'scale(1)';
                }, 80);

                // Hafif sarsılma efekti
                setTimeout(() => {
                    palletClone.style.transform = 'scale(0.98)';
                }, 400);

                setTimeout(() => {
                    palletClone.style.transform = 'scale(1)';
                }, 600);
            }, config.timing.palletDropDelay);

            // 4. Forklift güzelce sola çıksın
            setTimeout(() => {
                // Forklift'e çıkış transition'ı ekle
                forklift.style.transition = `transform ${config.exitDuration}s cubic-bezier(0.55, 0.085, 0.68, 0.53)`;
                forklift.style.transform = `translateX(${config.exitDistance}px)`;

                // Parçaları hafifçe sallan
                cabin.style.transition = 'transform 0.25s ease-in-out';
                tip.style.transition = 'transform 0.25s ease-in-out';

                setTimeout(() => {
                    cabin.style.transform = 'translateY(-1px)';
                    tip.style.transform = 'translateY(1px)';
                }, 150);

                setTimeout(() => {
                    cabin.style.transform = 'translateY(0px)';
                    tip.style.transform = 'translateY(0px)';
                }, 300);
            }, config.timing.exitDelay);
        }

        // Intersection observer ile tetikle
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    runAnimation();
                    observer.unobserve(section);
                }
            });
        }, { threshold: 0.3 });

        observer.observe(section);
    }

    // Initialize
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initForkliftAnimation);
    } else {
        initForkliftAnimation();
    }

})();