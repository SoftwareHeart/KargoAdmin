/**
 * Services Section Scroll Animations
 * Aleris Global - Services Animations Controller
 * Author: Yazılım Mühendisi
 * Version: 1.0
 */

class ServicesAnimationController {
    constructor() {
        this.isInitialized = false;
        this.animatedSections = new Set();
        this.observers = [];
        this.scrollProgress = 0;

        // Configuration
        this.config = {
            rootMargin: '-10% 0px -10% 0px',
            threshold: [0, 0.25, 0.5, 0.75, 1],
            animationDelay: {
                world: 200,
                vehicle: 400,
                containers: 600,
                content: 800
            },
            easing: 'cubic-bezier(0.34, 1.56, 0.64, 1)',
            duration: 1200
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
        console.log('🚀 Services Animation Controller initialized successfully!');
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
     * Setup Intersection Observer for sections
     */
    setupIntersectionObserver() {
        // Services sections observer
        const servicesObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && entry.intersectionRatio > 0.3) {
                    // Sadece intersection ratio %30'dan fazla olunca animate et
                    this.animateServicesSection(entry.target);
                }
            });
        }, {
            rootMargin: this.config.rootMargin,
            threshold: [0, 0.1, 0.3, 0.5, 0.7]
        });

        // Observe all services sections
        const servicesSections = document.querySelectorAll('.services-section');
        servicesSections.forEach((section, index) => {
            // Add unique identifier
            section.setAttribute('data-service-index', index);
            // Sadece henüz animate olmamış section'ları observe et
            if (!section.classList.contains('animate-in')) {
                servicesObserver.observe(section);
            }
        });

        this.observers.push(servicesObserver);

        // General elements observer - Daha konservatif threshold
        const elementsObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && entry.intersectionRatio > 0.2) {
                    if (!entry.target.classList.contains('visible')) {
                        entry.target.classList.add('visible');
                    }
                }
            });
        }, {
            rootMargin: '-5% 0px -5% 0px',
            threshold: [0, 0.2, 0.5]
        });

        // Observe fade elements
        const fadeElements = document.querySelectorAll('.fade-in-up, .fade-in-left, .fade-in-right, .scale-in');
        fadeElements.forEach(element => {
            elementsObserver.observe(element);
        });

        this.observers.push(elementsObserver);
    }

    /**
     * Setup scroll listeners
     */
    setupScrollListeners() {
        let ticking = false;

        const updateScrollProgress = () => {
            const winScroll = document.body.scrollTop || document.documentElement.scrollTop;
            const height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
            const scrolled = (winScroll / height) * 100;

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

        // Smooth scroll for anchor links
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
                const target = document.querySelector(link.getAttribute('href'));
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
        // Add animation classes to elements
        const world = section.querySelector('.world-services-container');
        const vehicle = section.querySelector('.vehicle-container');
        const containers = section.querySelector('.containers-wrapper');
        const serviceContainers = section.querySelectorAll('.service-container');
        const content = section.querySelectorAll('.container-content');

        // Set initial states
        if (world) world.classList.add('fade-in-left');
        if (vehicle) vehicle.classList.add('scale-in');
        if (containers) containers.classList.add('fade-in-right');

        serviceContainers.forEach((container, i) => {
            container.classList.add('scale-in');
            container.style.transitionDelay = `${0.8 + (i * 0.2)}s`;
        });

        content.forEach((item, i) => {
            item.classList.add('fade-in-up');
            item.style.transitionDelay = `${1.5 + (i * 0.1)}s`;
        });
    }

    /**
     * Animate services section when it comes into view
     */
    animateServicesSection(section) {
        const sectionId = section.getAttribute('data-service-index') || 'unknown';

        // Prevent multiple animations - Güçlü kontrol
        if (this.animatedSections.has(sectionId) || section.classList.contains('animate-in')) {
            console.log(`⏭️ Section ${sectionId} already animated, skipping...`);
            return;
        }

        // Section'ı hemen mark et ki tekrar animate olmasın
        this.animatedSections.add(sectionId);
        section.classList.add('animate-in');

        console.log(`🎬 Animating services section ${sectionId}`);

        // Observer'dan kaldır ki bir daha tetiklenmesin
        this.observers.forEach(observer => {
            if (observer.root === null) { // Services observer
                observer.unobserve(section);
            }
        });

        // Animate individual elements with delays
        this.animateElementsSequentially(section);

        // Add special effects for active containers
        setTimeout(() => {
            this.enhanceActiveContainers(section);
        }, 1000);

        // Particle effect for dramatic entrance
        this.createParticleEffect(section);
    }

    /**
     * Animate elements sequentially within a section
     */
    animateElementsSequentially(section) {
        const world = section.querySelector('.world-services-container');
        const vehicle = section.querySelector('.vehicle-container');
        const containers = section.querySelector('.containers-wrapper');
        const serviceContainers = section.querySelectorAll('.service-container');
        const content = section.querySelectorAll('.container-content');

        // World animation
        setTimeout(() => {
            if (world) {
                world.classList.add('visible');
                this.addRotationEffect(world);
            }
        }, this.config.animationDelay.world);

        // Vehicle animation
        setTimeout(() => {
            if (vehicle) {
                vehicle.classList.add('visible');
                this.addVehicleSpecialEffect(vehicle, section);
            }
        }, this.config.animationDelay.vehicle);

        // Containers animation
        setTimeout(() => {
            if (containers) {
                containers.classList.add('visible');
            }

            // Individual service containers
            serviceContainers.forEach((container, i) => {
                setTimeout(() => {
                    container.classList.add('visible');
                    this.addContainerHoverEffect(container);
                }, i * 200);
            });
        }, this.config.animationDelay.containers);

        // Content animation
        setTimeout(() => {
            content.forEach((item, i) => {
                setTimeout(() => {
                    item.classList.add('visible');
                }, i * 100);
            });
        }, this.config.animationDelay.content);
    }

    /**
     * Add rotation effect to world element
     */
    addRotationEffect(worldElement) {
        const worldImage = worldElement.querySelector('.world-services-image');
        if (worldImage) {
            worldImage.style.animation = 'worldPulse 6s ease-in-out infinite, worldRotateEntry 3s ease-out';
        }
    }

    /**
     * Add special vehicle effects based on vehicle type
     */
    addVehicleSpecialEffect(vehicleContainer, section) {
        const vehicleImage = vehicleContainer.querySelector('.vehicle-image');
        if (!vehicleImage) return;

        // Determine vehicle type from section classes or content
        if (section.id === 'services-section-air' || vehicleContainer.classList.contains('air-vehicle')) {
            this.addAirVehicleEffect(vehicleImage);
        } else if (section.id === 'services-section-sea' || vehicleContainer.classList.contains('sea-vehicle')) {
            this.addSeaVehicleEffect(vehicleImage);
        } else if (vehicleContainer.classList.contains('land-truck-vehicle')) {
            this.addTruckVehicleEffect(vehicleImage);
        } else if (vehicleContainer.classList.contains('land-van-vehicle')) {
            this.addVanVehicleEffect(vehicleImage);
        }
    }

    /**
     * Air vehicle specific effects
     */
    addAirVehicleEffect(vehicleImage) {
        vehicleImage.style.animation = 'vehicleFloat 4s ease-in-out infinite, airPlaneSpecial 8s linear infinite, airPlaneEntry 2s ease-out';
    }

    /**
     * Sea vehicle specific effects
     */
    addSeaVehicleEffect(vehicleImage) {
        vehicleImage.style.animation = 'vehicleFloat 4.5s ease-in-out infinite 1s, seaWave 6s ease-in-out infinite, seaEntry 2.5s ease-out';
    }

    /**
     * Truck vehicle specific effects
     */
    addTruckVehicleEffect(vehicleImage) {
        vehicleImage.style.animation = 'vehicleFloat 4.2s ease-in-out infinite 0.5s, truckRoll 7s linear infinite, truckEntry 2s ease-out';
    }

    /**
     * Van vehicle specific effects
     */
    addVanVehicleEffect(vehicleImage) {
        vehicleImage.style.animation = 'vehicleFloat 5s ease-in-out infinite 2s, landBounce 4s ease-in-out infinite, vanEntry 2.2s ease-out';
    }

    /**
     * Enhance active containers with special effects
     */
    enhanceActiveContainers(section) {
        const activeContainers = section.querySelectorAll('.service-container.active-container');

        activeContainers.forEach(container => {
            // Add glow effect
            container.style.animation = 'activeGlow 2s ease-in-out infinite, containerSpecialEntry 1s ease-out';

            // Add floating particles around active container
            this.addFloatingParticles(container);
        });
    }

    /**
     * Add container hover effects
     */
    addContainerHoverEffect(container) {
        const containerImage = container.querySelector('.container-image');

        container.addEventListener('mouseenter', () => {
            containerImage.style.transform = 'scale(1.18) rotate(2deg) translateY(-8px)';
            containerImage.style.filter = 'drop-shadow(0 30px 60px rgba(0, 0, 0, 0.3))';

            // Add temporary glow
            this.addTemporaryGlow(container);
        });

        container.addEventListener('mouseleave', () => {
            containerImage.style.transform = '';
            containerImage.style.filter = '';
        });
    }

    /**
     * Add temporary glow effect on hover
     */
    addTemporaryGlow(element) {
        element.style.boxShadow = '0 0 30px rgba(0, 92, 185, 0.4)';

        setTimeout(() => {
            element.style.boxShadow = '';
        }, 500);
    }

    /**
     * Create particle effect for dramatic entrance
     */
    createParticleEffect(section) {
        const particleContainer = document.createElement('div');
        particleContainer.className = 'particle-container';
        particleContainer.style.cssText = `
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            z-index: 1;
        `;

        // Create particles
        for (let i = 0; i < 15; i++) {
            const particle = document.createElement('div');
            particle.className = 'particle';
            particle.style.cssText = `
                position: absolute;
                width: ${Math.random() * 4 + 2}px;
                height: ${Math.random() * 4 + 2}px;
                background: ${Math.random() > 0.5 ? '#005cb9' : '#ff6a00'};
                border-radius: 50%;
                top: ${Math.random() * 100}%;
                left: ${Math.random() * 100}%;
                opacity: 0;
                animation: particleFloat ${3 + Math.random() * 2}s ease-out forwards;
                animation-delay: ${Math.random() * 2}s;
            `;

            particleContainer.appendChild(particle);
        }

        section.appendChild(particleContainer);

        // Remove particles after animation
        setTimeout(() => {
            if (particleContainer.parentNode) {
                particleContainer.parentNode.removeChild(particleContainer);
            }
        }, 6000);
    }

    /**
     * Add floating particles around active containers
     */
    addFloatingParticles(container) {
        const particleCount = 5;

        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.className = 'floating-particle';
            particle.style.cssText = `
                position: absolute;
                width: 3px;
                height: 3px;
                background: #ff6a00;
                border-radius: 50%;
                top: ${20 + Math.random() * 60}%;
                left: ${20 + Math.random() * 60}%;
                opacity: 0.7;
                animation: floatAround ${4 + Math.random() * 2}s ease-in-out infinite;
                animation-delay: ${Math.random() * 2}s;
                pointer-events: none;
                z-index: 6;
            `;

            container.appendChild(particle);
        }

        // Remove floating particles after some time
        setTimeout(() => {
            const particles = container.querySelectorAll('.floating-particle');
            particles.forEach(particle => {
                if (particle.parentNode) {
                    particle.parentNode.removeChild(particle);
                }
            });
        }, 10000);
    }

    /**
     * Bind additional events
     */
    bindEvents() {
        // Handle window resize
        let resizeTimeout;
        window.addEventListener('resize', () => {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(() => {
                this.handleResize();
            }, 250);
        }, { passive: true });

        // Handle page visibility change
        document.addEventListener('visibilitychange', () => {
            if (document.hidden) {
                this.pauseAnimations();
            } else {
                this.resumeAnimations();
            }
        });

        // Performance monitoring
        //this.monitorPerformance();
    }

    /**
     * Handle window resize
     */
    handleResize() {
        // Recalculate positions for mobile/desktop switches
        const servicesSections = document.querySelectorAll('.services-section');
        servicesSections.forEach(section => {
            if (window.innerWidth <= 991) {
                section.classList.add('mobile-layout');
            } else {
                section.classList.remove('mobile-layout');
            }
        });
    }

    /**
     * Pause animations when tab is not visible
     */
    pauseAnimations() {
        const animatedElements = document.querySelectorAll('[style*="animation"]');
        animatedElements.forEach(element => {
            element.style.animationPlayState = 'paused';
        });
    }

    /**
     * Resume animations when tab becomes visible
     */
    resumeAnimations() {
        const animatedElements = document.querySelectorAll('[style*="animation"]');
        animatedElements.forEach(element => {
            element.style.animationPlayState = 'running';
        });
    }

    /**
     * Monitor performance and adjust animations accordingly
     */
    monitorPerformance() {
        if ('connection' in navigator) {
            const connection = navigator.connection;

            if (connection.effectiveType === 'slow-2g' || connection.effectiveType === '2g') {
                this.reducedAnimations();
            }
        }

        // Check for reduced motion preference
        if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
            this.disableAnimations();
        }
    }

    /**
     * Reduce animations for low performance devices
     */
    //reducedAnimations() {
    //    const style = document.createElement('style');
    //    style.textContent = `
    //        .services-section * {
    //            animation-duration: 0.5s !important;
    //            transition-duration: 0.3s !important;
    //        }
    //    `;
    //    document.head.appendChild(style);
    //}

    /**
     * Disable animations for accessibility
     */
    disableAnimations() {
        const style = document.createElement('style');
        style.textContent = `
            .services-section *,
            .services-section *::before,
            .services-section *::after {
                animation: none !important;
                transition: none !important;
            }
        `;
        document.head.appendChild(style);
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
        this.isInitialized = false;

        console.log('🧹 Services Animation Controller destroyed');
    }
}

// CSS Keyframes for additional animations - Inject into document
const additionalKeyframes = `
<style>
@keyframes particleFloat {
    0% {
        opacity: 0;
        transform: translateY(0) scale(0);
    }
    50% {
        opacity: 1;
        transform: translateY(-50px) scale(1);
    }
    100% {
        opacity: 0;
        transform: translateY(-100px) scale(0);
    }
}

@keyframes floatAround {
    0%, 100% {
        transform: translate(0, 0);
    }
    25% {
        transform: translate(10px, -10px);
    }
    50% {
        transform: translate(-10px, -15px);
    }
    75% {
        transform: translate(15px, 5px);
    }
}

@keyframes worldRotateEntry {
    0% {
        transform: rotate(-90deg) scale(0.5);
        opacity: 0;
    }
    100% {
        transform: rotate(0deg) scale(1);
        opacity: 1;
    }
}

@keyframes airPlaneEntry {
    0% {
        transform: translateX(-200px) rotate(-45deg) scale(0.7);
        opacity: 0;
    }
    100% {
        transform: translateX(0) rotate(0deg) scale(1);
        opacity: 1;
    }
}

@keyframes seaEntry {
    0% {
        transform: translateY(100px) scale(0.8);
        opacity: 0;
    }
    100% {
        transform: translateY(0) scale(1);
        opacity: 1;
    }
}

@keyframes truckEntry {
    0% {
        transform: translateX(-150px) scale(0.9);
        opacity: 0;
    }
    100% {
        transform: translateX(0) scale(1);
        opacity: 1;
    }
}

@keyframes vanEntry {
    0% {
        transform: translateY(80px) rotate(10deg) scale(0.8);
        opacity: 0;
    }
    100% {
        transform: translateY(0) rotate(0deg) scale(1);
        opacity: 1;
    }
}

@keyframes containerSpecialEntry {
    0% {
        transform: scale(0.5) rotate(180deg);
        opacity: 0;
    }
    50% {
        transform: scale(1.2) rotate(0deg);
        opacity: 0.8;
    }
    100% {
        transform: scale(1) rotate(0deg);
        opacity: 1;
    }
}
</style>
`;

// Initialize when DOM is ready
let servicesAnimationController;

document.addEventListener('DOMContentLoaded', () => {
    // Inject additional CSS
    document.head.insertAdjacentHTML('beforeend', additionalKeyframes);

    // Initialize animation controller
    servicesAnimationController = new ServicesAnimationController();
});

// Export for potential external use
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ServicesAnimationController;
} else if (typeof window !== 'undefined') {
    window.ServicesAnimationController = ServicesAnimationController;
}