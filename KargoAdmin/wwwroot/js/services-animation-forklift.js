/**
 * Forklift Animation - Clean Start
 * Aleris Global - Simple forklift movement animation
 * Version: Clean 1.0
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

        function runAnimation() {
            // 1. Sağdan başla
            forklift.style.transform = 'translateX(300px)';
            cabin.style.opacity = '1';
            tip.style.opacity = '1';
            pallet.style.opacity = '1';

            // 2. Merkeze gel
            setTimeout(() => {
                forklift.style.transition = 'transform 2s ease-in-out';
                forklift.style.transform = 'translateX(0px)';
            }, 500);

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
                palletClone.style.top = (palletRect.top - sectionRect.top + 20) + 'px'; // Biraz yukarıdan başla
                const currentWidthPx = parseFloat(computedStyle.width) || palletRect.width;
                const currentHeightPx = parseFloat(computedStyle.height) || palletRect.height;
                const enlargedWidthPx = currentWidthPx + 75;
                const enlargedHeightPx = currentWidthPx > 0 && currentHeightPx > 0
                    ? (currentHeightPx / currentWidthPx) * enlargedWidthPx
                    : currentHeightPx;

                palletClone.style.width = enlargedWidthPx + 'px';
                palletClone.style.height = isNaN(enlargedHeightPx) ? 'auto' : (enlargedHeightPx + 'px');
                palletClone.style.transform = 'scale(1.1)'; // Biraz büyük başla
                palletClone.style.transition = 'all 0.8s cubic-bezier(0.34, 1.56, 0.64, 1)'; // Bounce effect
                palletClone.style.zIndex = '10';
                palletClone.style.opacity = '1';
                section.appendChild(palletClone);

                // Forklift'teki pallet'i gizle
                pallet.style.opacity = '0';

                // Koli bırakma animasyonu - yerçekimi efekti
                setTimeout(() => {
                    palletClone.style.top = (palletRect.top - sectionRect.top - 20) + 'px'; // 60px daha yukarıda bırak
                    palletClone.style.transform = 'scale(1)'; // Normal boyuta gel
                }, 100);

                // Hafif sarsılma efekti
                setTimeout(() => {
                    palletClone.style.transform = 'scale(0.98)';
                }, 500);

                setTimeout(() => {
                    palletClone.style.transform = 'scale(1)';
                }, 700);
            }, 2800);

            // 4. Forklift güzelce sola çıksın
            setTimeout(() => {
                // Forklift'e çıkış transition'ı ekle
                forklift.style.transition = 'transform 1.5s cubic-bezier(0.55, 0.085, 0.68, 0.53)'; // Hızlanarak çık
                forklift.style.transform = 'translateX(-800px)'; // Daha uzağa git

                // Parçaları hafifçe sallan
                cabin.style.transition = 'transform 0.3s ease-in-out';
                tip.style.transition = 'transform 0.3s ease-in-out';

                setTimeout(() => {
                    cabin.style.transform = 'translateY(-2px)';
                    tip.style.transform = 'translateY(2px)';
                }, 200);

                setTimeout(() => {
                    cabin.style.transform = 'translateY(0px)';
                    tip.style.transform = 'translateY(0px)';
                }, 400);
            }, 3200);
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