/**
 * Services Section Scroll Animations - FIXED VERSION
 * Aleris Global - Services Animations Controller
 * Author: Yazılım Mühendisi
 * Version: 2.1 - Performans kodları kaldırıldı, sadece temel optimizasyonlar
 */

class ServicesAnimationController {
    constructor() {
        this.isInitialized = false;
        this.animatedSections = new Set();
        this.processingSections = new Set(); // Yeni: İşlem görülen section'ları takip et
        this.observers = [];
        this.scrollProgress = 0;

        // Configuration - Basit ve çalışan
        this.config = {
            rootMargin: '-15% 0px -15% 0px',
            threshold: [0.4], // Tek threshold
            animationDelay: {
                world: 200,
                vehicle: 400,
                containers: 600,
                content: 800
            }
        };

        this.init();
    }

    /**
     * Initialize the animation controller
     */
    init() {
        if (this.isInitialized) return;

        // Wait for DOM to be ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.setup());
        } else {
            this.setup();
        }
    }

    /**
     * Setup all animations and observers
     */
    setup() {
        this.createScrollProgressBar();
        this.setupIntersectionObserver();
        this.setupScrollListeners();
        this.setupServicesSectionAnimations();
        this.bindEvents();

        this.isInitialized = true;
        console.log('🚀 Services Animation Controller v2.1 initialized successfully!');
    }

    /**
     * Create scroll progress bar
     */
    createScrollProgressBar() {
        if (document.querySelector('.scroll-progress')) return;

        const progressBar = document.createElement('div');
        progressBar.className = 'scroll-progress';
        document.body.appendChild(progressBar);

        this.progressBar = progressBar;
    }

    /**
     * Setup Intersection Observer for sections - FIX: Tekrar animasyon sorunu
     */
    setupIntersectionObserver() {
        // Services sections observer
        const servicesObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                const section = entry.target;
                const sectionId = section.getAttribute('data-service-index');

                if (entry.isIntersecting && entry.intersectionRatio >= 0.4) {
                    // CRITICAL FIX: Çoklu kontrol
                    if (!this.animatedSections.has(sectionId) &&
                        !this.processingSections.has(sectionId) &&
                        !section.classList.contains('animate-in')) {

                        this.animateServicesSection(section);
                    }
                }
            });
        }, {
            rootMargin: this.config.rootMargin,
            threshold: this.config.threshold
        });

        // Observe all services sections
        const servicesSections = document.querySelectorAll('.services-section');
        servicesSections.forEach((section, index) => {
            // Add unique identifier
            section.setAttribute('data-service-index', `section-${index}`);
            servicesObserver.observe(section);
        });

        this.observers.push(servicesObserver);
    }

    /**
     * Animate services section when it comes into view - FIX: Tekrar animasyon sorunu
     */
    animateServicesSection(section) {
        const sectionId = section.getAttribute('data-service-index') || `unknown-${Date.now()}`;

        // CRITICAL FIX: Çoklu güvenlik kontrolleri
        if (this.animatedSections.has(sectionId) ||
            this.processingSections.has(sectionId) ||
            section.classList.contains('animate-in') ||
            section.hasAttribute('data-animated')) {

            console.log(`⏭️ Section ${sectionId} already processed, skipping...`);
            return;
        }

        // Hemen işaretleyerek tekrar tetiklenmeyi engelle
        this.processingSections.add(sectionId);
        this.animatedSections.add(sectionId);
        section.classList.add('animate-in');
        section.setAttribute('data-animated', 'true');

        console.log(`🎬 Animating services section ${sectionId}`);

        // Observer'dan hemen kaldır
        this.observers.forEach(observer => {
            observer.unobserve(section);
        });

        // Animate individual elements
        this.animateElementsSequentially(section);

        // Add special effects with delay
        setTimeout(() => {
            this.enhanceActiveContainers(section);
            this.processingSections.delete(sectionId); // İşlem tamamlandı
        }, 1500);
    }

    /**
     * Animate elements sequentially
     */
    animateElementsSequentially(section) {
        const world = section.querySelector('.world-services-container');
        const vehicles = section.querySelectorAll('.vehicle-container');
        const containers = section.querySelector('.containers-wrapper');
        const serviceContainers = section.querySelectorAll('.service-container');
        const content = section.querySelectorAll('.container-content');
        const title = section.querySelector('.services-title');

        // Title animation
        if (title) {
            setTimeout(() => {
                title.style.transitionDelay = '0.2s';
            }, 100);
        }

        // World animation
        if (world) {
            setTimeout(() => {
                this.addWorldSpecialEffect(world, section);
            }, this.config.animationDelay.world);
        }

        // Vehicles animation
        vehicles.forEach((vehicle, i) => {
            setTimeout(() => {
                this.addVehicleSpecialEffect(vehicle, section);
            }, this.config.animationDelay.vehicle + (i * 100));
        });

        // Containers animation
        if (containers) {
            setTimeout(() => {
                // Container animasyonu CSS'de otomatik çalışıyor
            }, this.config.animationDelay.containers);
        }

        // Service containers animation
        serviceContainers.forEach((container, i) => {
            setTimeout(() => {
                container.style.transitionDelay = `${this.config.animationDelay.containers + (i * 150)}ms`;
            }, 50);
        });

        // Container content animations - Yazıları hareket ettir
        content.forEach((item, i) => {
            setTimeout(() => {
                item.style.transitionDelay = `${this.config.animationDelay.content + (i * 100)}ms`;
                item.classList.add('fade-in-up');

                // Daha sonra visible class'ını ekle
                setTimeout(() => {
                    item.classList.add('visible');
                }, this.config.animationDelay.content + (i * 100) + 100);
            }, 100);
        });

        // Container numbers animation - Numaraları hareket ettir  
        const containerNumbers = section.querySelectorAll('.container-number');
        containerNumbers.forEach((number, i) => {
            setTimeout(() => {
                number.classList.add('scale-in');
                setTimeout(() => {
                    number.classList.add('visible');
                }, 50);
            }, this.config.animationDelay.content + (i * 150));
        });
    }

    /**
     * Add special world effects
     */
    addWorldSpecialEffect(worldContainer, section) {
        const worldImage = worldContainer.querySelector('.world-services-image');
        if (worldImage) {
            worldImage.style.animation = 'worldPulse 6s ease-in-out infinite';
        }
    }

    /**
     * Add special vehicle effects based on vehicle type
     */
    addVehicleSpecialEffect(vehicleContainer, section) {
        const vehicleImage = vehicleContainer.querySelector('.vehicle-image');
        if (!vehicleImage) return;

        // Determine vehicle type and apply appropriate animation
        if (section.id === 'services-section-air' || vehicleContainer.classList.contains('air-vehicle')) {
            vehicleImage.style.animation = 'vehicleFloat 4s ease-in-out infinite, airPlaneSpecial 8s linear infinite';
        } else if (section.id === 'services-section-sea' || vehicleContainer.classList.contains('sea-vehicle')) {
            vehicleImage.style.animation = 'vehicleFloat 4.5s ease-in-out infinite 1s, seaWave 6s ease-in-out infinite';
        } else if (vehicleContainer.classList.contains('land-truck-vehicle')) {
            vehicleImage.style.animation = 'vehicleFloat 4.2s ease-in-out infinite 0.5s, truckRoll 7s linear infinite';
        } else if (vehicleContainer.classList.contains('land-van-vehicle')) {
            vehicleImage.style.animation = 'vehicleFloat 5s ease-in-out infinite 2s, landBounce 4s ease-in-out infinite';
        }
    }

    /**
     * Enhance active containers with special effects
     */
    enhanceActiveContainers(section) {
        const activeContainers = section.querySelectorAll('.service-container');

        activeContainers.forEach((container, index) => {
            // Container content animasyonunu tetikle
            const containerContent = container.querySelector('.container-content');
            if (containerContent) {
                setTimeout(() => {
                    containerContent.classList.add('fade-in-up', 'visible');
                }, 800 + (index * 200)); // Her konteyner için farklı gecikme
            }

            // Container number animasyonunu tetikle
            const containerNumber = container.querySelector('.container-number');
            if (containerNumber) {
                setTimeout(() => {
                    containerNumber.classList.add('scale-in', 'visible');
                }, 1000 + (index * 200));
            }

            // Container title animasyonunu tetikle
            const containerTitle = container.querySelector('.container-content::before, .container-content h3');
            if (containerTitle) {
                setTimeout(() => {
                    containerTitle.classList.add('fade-in-up', 'visible');
                }, 900 + (index * 200));
            }

            // Add hover enhancement
            container.addEventListener('mouseenter', (e) => {
                const container = e.currentTarget;
                container.style.transform = 'translateY(-2px) scale(1.02)';
                container.style.zIndex = '15';

                const image = container.querySelector('.container-image');
                if (image) {
                    image.style.animationPlayState = 'paused';
                }
            });

            container.addEventListener('mouseleave', (e) => {
                const container = e.currentTarget;
                container.style.transform = '';
                container.style.zIndex = '';

                const image = container.querySelector('.container-image');
                if (image) {
                    image.style.animationPlayState = 'running';
                }
            });
        });
    }

    /**
     * Setup scroll listeners
     */
    setupScrollListeners() {
        let ticking = false;

        const updateScrollProgress = () => {
            const winScroll = document.body.scrollTop || document.documentElement.scrollTop;
            const height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
            const scrolled = Math.min((winScroll / height) * 100, 100);

            this.scrollProgress = scrolled;

            if (this.progressBar) {
                this.progressBar.style.width = scrolled + '%';
            }

            ticking = false;
        };

        window.addEventListener('scroll', () => {
            if (!ticking) {
                requestAnimationFrame(updateScrollProgress);
                ticking = true;
            }
        }, { passive: true });

        // Setup smooth scroll
        this.setupSmoothScroll();
    }

    /**
     * Setup smooth scroll for navigation links
     */
    setupSmoothScroll() {
        const links = document.querySelectorAll('a[href^="#"]');
        links.forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = link.getAttribute('href');
                const target = document.querySelector(targetId);

                if (target) {
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    /**
     * Setup services section specific animations
     */
    setupServicesSectionAnimations() {
        const servicesSections = document.querySelectorAll('.services-section');

        servicesSections.forEach((section, index) => {
            this.prepareServicesSectionForAnimation(section, index);
        });
    }

    /**
     * Prepare services section for animation
     */
    prepareServicesSectionForAnimation(section, index) {
        // Basit hazırlık, CSS'de zaten tanımlı animasyonlar var
        const elements = section.querySelectorAll('.world-services-container, .vehicle-container, .containers-wrapper, .service-container, .container-content');

        elements.forEach(element => {
            // Sadece başlangıç durumunu kontrol et
            if (!element.classList.contains('animate-in')) {
                // CSS'deki initial state'ler zaten tanımlı
            }
        });
    }

    /**
     * Bind additional events
     */
    bindEvents() {
        // Resize handler
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                // Basit resize handling
                console.log('Window resized');
            }, 250);
        });
    }

    /**
     * Cleanup method
     */
    destroy() {
        // Remove all observers
        this.observers.forEach(observer => observer.disconnect());
        this.observers = [];

        // Remove progress bar
        if (this.progressBar && this.progressBar.parentNode) {
            this.progressBar.parentNode.removeChild(this.progressBar);
        }

        // Reset flags
        this.animatedSections.clear();
        this.processingSections.clear();
        this.isInitialized = false;

        console.log('🧹 Services Animation Controller destroyed');
    }
}

// Initialize when DOM is ready
let servicesAnimationController;

const initServicesAnimation = () => {
    if (!servicesAnimationController) {
        servicesAnimationController = new ServicesAnimationController();
    }
};

// Auto-initialize
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initServicesAnimation);
} else {
    initServicesAnimation();
}

// Export for manual control
window.ServicesAnimationController = ServicesAnimationController;