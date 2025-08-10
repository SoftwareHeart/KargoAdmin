/**
 * Services Section Scroll Animations - MODERN VERSION
 * Aleris Global - Services Animations Controller
 * Author: Senior Frontend Developer
 * Version: 3.0 - Background video sabit, diğer elementler scroll ile animate
 */

class ServicesAnimationController {
    constructor() {
        this.isInitialized = false;
        this.animatedSections = new Set();
        this.observers = [];

        // Animation configuration
        this.config = {
            rootMargin: '-20% 0px -20% 0px',
            threshold: [0.3],
            staggerDelay: 200, // Her element arası gecikme
            animationDuration: 800
        };

        this.init();
    }

    /**
     * Initialize the animation controller
     */
    init() {
        if (this.isInitialized) return;

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
        this.prepareServicesSections();
        this.setupIntersectionObserver();
        this.isInitialized = true;
        console.log('🚀 Services Animation Controller v3.0 initialized!');
    }

    /**
     * Prepare all services sections for animation
     */
    prepareServicesSections() {
        const servicesSections = document.querySelectorAll('.services-section');

        servicesSections.forEach((section, index) => {
            section.setAttribute('data-section-id', `services-${index}`);

            // Background video'yu hemen göster (animasyon yok)
            const backgroundVideo = section.querySelector('.services-background-video');
            if (backgroundVideo) {
                backgroundVideo.style.opacity = '1';
                backgroundVideo.style.transition = 'none';
            }

            // Diğer elementleri hazırla
            this.prepareAnimationElements(section);
        });
    }

    /**
     * Prepare individual elements for animation
     */
    prepareAnimationElements(section) {
        // Animate edilecek elementler
        const animationElements = [
            { selector: '.world-services-container', delay: 0 },
            { selector: '.vehicle-container', delay: 1 },
            { selector: '.services-title', delay: 2 },
            { selector: '.service-container', delay: 3 },
            { selector: '.container-content', delay: 4 }
        ];

        animationElements.forEach(({ selector, delay }) => {
            const elements = section.querySelectorAll(selector);
            elements.forEach((element, index) => {
                element.setAttribute('data-animation-delay', delay + index * 0.5);
                element.classList.add('animation-ready');
            });
        });
    }

    /**
     * Setup Intersection Observer
     */
    setupIntersectionObserver() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting && entry.intersectionRatio >= 0.3) {
                    this.animateServicesSection(entry.target);
                }
            });
        }, {
            rootMargin: this.config.rootMargin,
            threshold: this.config.threshold
        });

        // Observe all services sections
        document.querySelectorAll('.services-section').forEach(section => {
            observer.observe(section);
        });

        this.observers.push(observer);
    }

    /**
     * Animate services section when it comes into view
     */
    animateServicesSection(section) {
        const sectionId = section.getAttribute('data-section-id');

        // Prevent multiple animations
        if (this.animatedSections.has(sectionId)) return;
        this.animatedSections.add(sectionId);

        console.log(`🎬 Animating section: ${sectionId}`);

        // Animate elements with stagger effect
        this.animateElementsWithStagger(section);

        // Stop observing this section
        this.observers.forEach(observer => observer.unobserve(section));
    }

    /**
     * Animate elements with stagger effect
     */
    animateElementsWithStagger(section) {
        const animationElements = section.querySelectorAll('.animation-ready');

        animationElements.forEach(element => {
            const delay = parseFloat(element.getAttribute('data-animation-delay')) * this.config.staggerDelay;

            setTimeout(() => {
                element.classList.add('animate-in');
                this.addSpecialEffects(element);
            }, delay);
        });
    }

    /**
     * Add special effects to specific elements
     */
    addSpecialEffects(element) {
        // World special effects
        if (element.classList.contains('world-services-container')) {
            setTimeout(() => {
                element.classList.add('world-pulse');
            }, 500);
        }

        // Vehicle movement effects
        if (element.classList.contains('vehicle-container')) {
            setTimeout(() => {
                element.classList.add('vehicle-float');
            }, 300);
        }

        // Container hover effects
        if (element.classList.contains('service-container')) {
            this.setupContainerInteractions(element);
        }
    }

    /**
     * Setup container hover interactions
     */
    setupContainerInteractions(container) {
        container.addEventListener('mouseenter', () => {
            container.classList.add('container-hover');
        });

        container.addEventListener('mouseleave', () => {
            container.classList.remove('container-hover');
        });
    }

    /**
     * Cleanup method
     */
    destroy() {
        this.observers.forEach(observer => observer.disconnect());
        this.observers = [];
        this.animatedSections.clear();
        this.isInitialized = false;
        console.log('🧹 Services Animation Controller destroyed');
    }
}

// Auto-initialize
let servicesAnimationController;

const initServicesAnimation = () => {
    if (!servicesAnimationController) {
        servicesAnimationController = new ServicesAnimationController();
    }
};

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initServicesAnimation);
} else {
    initServicesAnimation();
}

// Global access
window.ServicesAnimationController = ServicesAnimationController;