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
            staggerDelay: 600, // Daha uzun gecikme
            animationDuration: 2600 // Daha uzun süre
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
        // Keep an ordered list of sections for push choreography
        this.sections = Array.from(document.querySelectorAll('.services-section'));
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

        // Simplified: vehicles enter from right and stay; previous exits left by CSS clone choreography
        this.performPushTransition(section);

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
     * Perform vehicle push choreography between consecutive sections
     * - Clones previous section's vehicle into current section
     * - Animates current vehicle pushing previous one out
     */
    performPushTransition(currentSection) {
        if (!this.sections || this.sections.length === 0) return;

        const currentIndex = this.sections.indexOf(currentSection);
        if (currentIndex <= 0) return; // First services-section does not push anything

        const previousSection = this.sections[currentIndex - 1];
        const previousVehicle = previousSection.querySelector('.vehicle-container');
        const currentVehicle = currentSection.querySelector('.vehicle-container');
        const currentLeft = currentSection.querySelector('.services-left');

        if (!previousVehicle || !currentVehicle || !currentLeft) return;

        // Clone previous vehicle so current vehicle can replace it while clone exits to the left
        const clone = previousVehicle.cloneNode(true);
        clone.classList.add('vehicle-clone', 'push-target', 'animate-in');
        clone.classList.remove('animation-ready');

        // Ensure clean animation classes on the clone image too
        const cloneImg = clone.querySelector('.vehicle-image');
        if (cloneImg) {
            cloneImg.style.willChange = 'transform';
        }

        // Insert clone under current vehicle so current can pass over it
        currentLeft.appendChild(clone);

        // Prepare incoming vehicle (from right to center)
        currentVehicle.classList.add('push-incoming');

        // Force reflow before starting transitions
        // eslint-disable-next-line no-unused-expressions
        currentVehicle.offsetHeight; // reflow

        // Start push after a short delay so the section's base animations can settle
        setTimeout(() => {
            currentVehicle.classList.add('do-push');
            clone.classList.add('pushed-out');

            const cleanup = () => {
                currentVehicle.classList.remove('push-incoming', 'do-push');
                if (clone && clone.parentNode) clone.parentNode.removeChild(clone);
                currentVehicle.removeEventListener('transitionend', cleanup);
            };

            // Cleanup after transition completes
            currentVehicle.addEventListener('transitionend', cleanup);
        }, this.config.staggerDelay * 2);

        // Containers switch effect (visual emphasis change)
        const containersWrapper = currentSection.querySelector('.containers-wrapper');
        if (containersWrapper) {
            containersWrapper.classList.add('containers-switch');
            setTimeout(() => containersWrapper.classList.remove('containers-switch'), this.config.animationDuration + this.config.staggerDelay);
        }
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