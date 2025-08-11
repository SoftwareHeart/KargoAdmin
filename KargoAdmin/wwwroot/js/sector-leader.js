// Sector Leader Section JavaScript

document.addEventListener('DOMContentLoaded', function () {


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

                // Counter animation
                animateCounters(section);

                // Stats box drop animation after initial slide-in (3 saniye sonra)
                setTimeout(() => {
                    const statsBoxes = section.querySelectorAll('.stats-box');

                    statsBoxes.forEach((box, index) => {
                        setTimeout(() => {
                            box.classList.add('drop');

                            // Add floating animation after drop (1 saniye sonra)
                            setTimeout(() => {
                                box.classList.add('loaded');
                            }, 1000);
                        }, index * 200);
                    });
                }, 3000);

                // Forklift choreography: forklift moves right->left, stops by each box, drops load, proceeds
                try {
                    const forklift = document.querySelector('.forklift-container');
                    const cabin = document.querySelector('.forklift-cabin');
                    const tip = document.querySelector('.forklift-tip');
                    const load = document.querySelector('.forklift-load');
                    const vehicle = document.querySelector('.forklift-vehicle');
                    const mover = forklift; // move the whole container to ensure visible motion

                    const boxes = Array.from(section.querySelectorAll('.stats-box'));
                    if (forklift && vehicle && cabin && tip && load && boxes.length) {
                        // Ensure initial state
                        forklift.classList.add('forklift-moving-in', 'forklift-holding-load');
                        let currentForkX = 0; // cumulative horizontal movement for forklift
                        // Bring forklift elements onscreen smoothly
                        requestAnimationFrame(() => {
                            cabin.style.opacity = '1';
                            tip.style.opacity = '1';
                            load.style.opacity = '1';
                            // Move the whole forklift container for reliable motion
                            mover.style.transform = 'translateX(0px)';
                            // Ensure load starts attached to the forks (no extra offset)
                            load.style.transform = 'translate(0px, 0px)';
                        });

                        const moveForkliftNearBoxAndDrop = (boxEl, delayMs, isLast) => {
                            setTimeout(() => {
                                const boxRect = boxEl.getBoundingClientRect();
                                const tipRect = tip.getBoundingClientRect();

                                // 1) Move forklift horizontally so that the tip comes near the left side of the target box
                                const targetTipX = boxRect.left + boxRect.width * 0.10; // slightly less inside so box is more to the right of the tip
                                const currentTipX = tipRect.left;
                                const forkDeltaX = targetTipX - currentTipX;

                                currentForkX += forkDeltaX;

                                mover.style.transition = 'transform 1.6s cubic-bezier(0.22, 0.61, 0.36, 1)';
                                mover.style.transform = `translateX(${currentForkX}px)`;

                                // 2) After forklift reaches the stop point, nudge the load slightly down/right into the box and drop
                                setTimeout(() => {
                                    const loadRect = load.getBoundingClientRect();
                                    // Desired contact point slightly inside the target box (visually aligns with fork tips)
                                    const baseAlignX = 0.12; // inside the box width
                                    const baseAlignY = 0.50; // mid-height of the box

                                    // Small viewport-specific nudge to keep alignment consistent on laptops/tablets
                                    const isLaptop = window.innerWidth >= 992 && window.innerWidth <= 1199;
                                    const isTablet = window.innerWidth >= 768 && window.innerWidth <= 991;
                                    const LOAD_ALIGN_X = 36 + (isLaptop ? 10 : 0) + (isTablet ? 8 : 0);
                                    const LOAD_ALIGN_Y = 0;

                                    const desiredX = (boxRect.left + boxRect.width * baseAlignX) + LOAD_ALIGN_X;
                                    const desiredY = (boxRect.top + boxRect.height * baseAlignY) + LOAD_ALIGN_Y;

                                    const deltaX = desiredX - loadRect.left;
                                    const deltaY = desiredY - loadRect.top;

                                    load.style.transition = 'transform 0.9s cubic-bezier(0.2, 0.8, 0.2, 1)';
                                    load.style.transform = `translate(${deltaX}px, ${deltaY}px)`;

                                    setTimeout(() => {
                                        // Drop: hide the carried load, reveal the box's own image
                                        load.classList.add('dropped');
                                        boxEl.classList.add('placed');

                                        // Reset load to fork position for next item
                                        setTimeout(() => {
                                            load.classList.remove('dropped');
                                            load.style.transition = 'transform 0.5s ease';
                                            load.style.transform = 'translate(0px, 0px)';
                                        }, 500);

                                        // If this was the last box, return forklift to the far right (starting) position
                                        if (isLast) {
                                            setTimeout(() => {
                                                mover.style.transition = 'transform 1.8s cubic-bezier(0.22, 0.61, 0.36, 1)';
                                                mover.style.transform = 'translateX(0px)';
                                            }, 300);
                                        }
                                    }, 750);
                                }, 1200);
                            }, delayMs);
                        };

                        // Wait until cabin and tip entrance animations are finished, then start choreography
                        let animationsRemaining = 2;
                        const maybeStart = () => {
                            animationsRemaining -= 1;
                            if (animationsRemaining <= 0) {
                                // Clear animations to ensure transforms are controlled by JS
                                cabin.style.animation = 'none';
                                tip.style.animation = 'none';
                                // Sequence: visit each box with spacing (slower & smoother)
                                boxes.forEach((b, i) => {
                                    const isLast = i === boxes.length - 1;
                                    moveForkliftNearBoxAndDrop(b, 800 + i * 2400, isLast);
                                });
                            }
                        };

                        const onCabinEnd = () => { cabin.removeEventListener('animationend', onCabinEnd); maybeStart(); };
                        const onTipEnd = () => { tip.removeEventListener('animationend', onTipEnd); maybeStart(); };

                        // Fallback timeout in case animationend doesn't fire
                        const fallback = setTimeout(() => {
                            animationsRemaining = 0;
                            maybeStart();
                        }, 1600);

                        cabin.addEventListener('animationend', () => { clearTimeout(fallback); onCabinEnd(); });
                        tip.addEventListener('animationend', () => { clearTimeout(fallback); onTipEnd(); });
                    }
                } catch (err) {
                    console.warn('Forklift choreography error:', err);
                }

                // Unobserve after animation is triggered
                observer.unobserve(section);
            }
        });
    }, observerOptions);

    // Observe the sector leader section
    const sectorLeaderSection = document.querySelector('.sector-leader-section');
    if (sectorLeaderSection) {
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