/**
 * BilgeLojistik İK Yönetim Sistemi - Presentation Script
 * Interactive presentation navigation and animations
 */

// Global state
let currentSlide = 1;
const totalSlides = document.querySelectorAll('.slide').length;

// DOM elements
const slides = document.querySelectorAll('.slide');
const prevBtn = document.getElementById('prevBtn');
const nextBtn = document.getElementById('nextBtn');
const slideCounter = document.getElementById('slideCounter');

/**
 * Initialize presentation
 */
function initPresentation() {
    // Show first slide
    showSlide(1);

    // Setup event listeners
    setupEventListeners();

    // Update counter
    updateCounter();

    // Initialize animations
    initAnimations();
}

/**
 * Setup all event listeners
 */
function setupEventListeners() {
    // Button navigation
    prevBtn.addEventListener('click', () => navigateSlide(-1));
    nextBtn.addEventListener('click', () => navigateSlide(1));

    // Keyboard navigation
    document.addEventListener('keydown', handleKeyPress);

    // Touch/Swipe support
    let touchStartX = 0;
    let touchEndX = 0;

    document.addEventListener('touchstart', (e) => {
        touchStartX = e.changedTouches[0].screenX;
    });

    document.addEventListener('touchend', (e) => {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    });

    function handleSwipe() {
        const swipeThreshold = 50;
        const diff = touchStartX - touchEndX;

        if (Math.abs(diff) > swipeThreshold) {
            if (diff > 0) {
                // Swiped left - next slide
                navigateSlide(1);
            } else {
                // Swiped right - previous slide
                navigateSlide(-1);
            }
        }
    }
}

/**
 * Handle keyboard navigation
 */
function handleKeyPress(e) {
    switch(e.key) {
        case 'ArrowLeft':
        case 'ArrowUp':
        case 'PageUp':
            e.preventDefault();
            navigateSlide(-1);
            break;
        case 'ArrowRight':
        case 'ArrowDown':
        case 'PageDown':
        case ' ': // Spacebar
            e.preventDefault();
            navigateSlide(1);
            break;
        case 'Home':
            e.preventDefault();
            goToSlide(1);
            break;
        case 'End':
            e.preventDefault();
            goToSlide(totalSlides);
            break;
    }
}

/**
 * Navigate to relative slide (next/previous)
 */
function navigateSlide(direction) {
    const newSlide = currentSlide + direction;

    if (newSlide >= 1 && newSlide <= totalSlides) {
        goToSlide(newSlide);
    }
}

/**
 * Go to specific slide number
 */
function goToSlide(slideNumber) {
    if (slideNumber < 1 || slideNumber > totalSlides) return;

    // Hide current slide
    slides[currentSlide - 1].classList.remove('active');

    // Update current slide
    currentSlide = slideNumber;

    // Show new slide
    showSlide(currentSlide);

    // Update UI
    updateCounter();
    updateButtons();

    // Trigger animations for new slide
    triggerSlideAnimations(currentSlide);
}

/**
 * Show specific slide
 */
function showSlide(slideNumber) {
    slides[slideNumber - 1].classList.add('active');

    // Scroll to top of slide content
    const slideContent = slides[slideNumber - 1].querySelector('.slide-content');
    if (slideContent) {
        slideContent.scrollTop = 0;
    }
}

/**
 * Update slide counter display
 */
function updateCounter() {
    slideCounter.textContent = `${currentSlide} / ${totalSlides}`;
}

/**
 * Update navigation button states
 */
function updateButtons() {
    // Disable previous button on first slide
    prevBtn.disabled = currentSlide === 1;

    // Disable next button on last slide
    nextBtn.disabled = currentSlide === totalSlides;
}

/**
 * Initialize all animations
 */
function initAnimations() {
    // Stat counter animation
    observeStatCounters();

    // Progress bar animations
    observeProgressBars();
}

/**
 * Trigger animations when slide becomes active
 */
function triggerSlideAnimations(slideNumber) {
    const slide = slides[slideNumber - 1];

    // Animate stat counters
    const statNumbers = slide.querySelectorAll('.stat-number');
    statNumbers.forEach(animateCounter);

    // Animate progress bars
    const progressBars = slide.querySelectorAll('.progress-fill');
    progressBars.forEach(bar => {
        const width = bar.style.width;
        bar.style.width = '0%';
        setTimeout(() => {
            bar.style.width = width;
        }, 300);
    });
}

/**
 * Observe stat counters with Intersection Observer
 */
function observeStatCounters() {
    const statNumbers = document.querySelectorAll('.stat-number');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animateCounter(entry.target);
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    statNumbers.forEach(stat => observer.observe(stat));
}

/**
 * Animate counter from 0 to target value
 */
function animateCounter(element) {
    const target = parseInt(element.getAttribute('data-target'));
    if (!target || isNaN(target)) return;

    const duration = 2000; // 2 seconds
    const steps = 60;
    const stepValue = target / steps;
    const stepDuration = duration / steps;
    let current = 0;

    const timer = setInterval(() => {
        current += stepValue;

        if (current >= target) {
            element.textContent = target.toLocaleString();
            clearInterval(timer);
        } else {
            element.textContent = Math.floor(current).toLocaleString();
        }
    }, stepDuration);
}

/**
 * Observe progress bars
 */
function observeProgressBars() {
    const progressBars = document.querySelectorAll('.progress-fill');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const bar = entry.target;
                const width = bar.style.width;
                bar.style.width = '0%';
                setTimeout(() => {
                    bar.style.width = width;
                }, 300);
            }
        });
    }, { threshold: 0.5 });

    progressBars.forEach(bar => observer.observe(bar));
}

/**
 * Handle window resize
 */
function handleResize() {
    // Adjust layout if needed
    const slideContent = document.querySelector('.slide.active .slide-content');
    if (slideContent) {
        // Ensure slide is visible
        slideContent.scrollTop = 0;
    }
}

/**
 * Export functionality for buttons (if needed)
 */
function exportToExcel() {
    alert('Excel export fonksiyonu aktif edilecek. jsPDF ve xlsx kütüphaneleri ile entegre edilebilir.');
}

function exportToPDF() {
    alert('PDF export fonksiyonu aktif edilecek. jsPDF kütüphanesi ile entegre edilebilir.');
}

/**
 * Fullscreen toggle
 */
function toggleFullscreen() {
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen().catch(err => {
            console.log('Fullscreen error:', err);
        });
    } else {
        document.exitFullscreen();
    }
}

/**
 * Print presentation
 */
function printPresentation() {
    window.print();
}

// Add keyboard shortcut for fullscreen (F11 alternative: F)
document.addEventListener('keydown', (e) => {
    if (e.key === 'f' || e.key === 'F') {
        if (!e.ctrlKey && !e.altKey) {
            e.preventDefault();
            toggleFullscreen();
        }
    }

    // P for print
    if (e.key === 'p' || e.key === 'P') {
        if (!e.ctrlKey) {
            e.preventDefault();
            printPresentation();
        }
    }
});

// Window resize handler
let resizeTimeout;
window.addEventListener('resize', () => {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(handleResize, 250);
});

// Add export button handlers if buttons exist
document.addEventListener('DOMContentLoaded', () => {
    const excelBtn = document.querySelector('.export-btn.excel');
    const pdfBtn = document.querySelector('.export-btn.pdf');

    if (excelBtn) excelBtn.addEventListener('click', exportToExcel);
    if (pdfBtn) pdfBtn.addEventListener('click', exportToPDF);
});

/**
 * Create keyboard shortcuts help
 */
function showKeyboardShortcuts() {
    const shortcuts = `
    Klavye Kısayolları:

    ← / ↑ / PageUp     : Önceki slayt
    → / ↓ / PageDown / Space : Sonraki slayt
    Home               : İlk slayt
    End                : Son slayt
    F                  : Tam ekran
    P                  : Yazdır
    H                  : Yardım (bu mesaj)
    `;

    alert(shortcuts);
}

// H for help
document.addEventListener('keydown', (e) => {
    if (e.key === 'h' || e.key === 'H') {
        if (!e.ctrlKey && !e.altKey) {
            e.preventDefault();
            showKeyboardShortcuts();
        }
    }
});

/**
 * Auto-play functionality (optional)
 */
let autoPlayInterval = null;
const autoPlayDelay = 10000; // 10 seconds per slide

function startAutoPlay() {
    if (autoPlayInterval) return;

    autoPlayInterval = setInterval(() => {
        if (currentSlide < totalSlides) {
            navigateSlide(1);
        } else {
            stopAutoPlay();
        }
    }, autoPlayDelay);
}

function stopAutoPlay() {
    if (autoPlayInterval) {
        clearInterval(autoPlayInterval);
        autoPlayInterval = null;
    }
}

function toggleAutoPlay() {
    if (autoPlayInterval) {
        stopAutoPlay();
        console.log('Auto-play stopped');
    } else {
        startAutoPlay();
        console.log('Auto-play started');
    }
}

// A for auto-play toggle
document.addEventListener('keydown', (e) => {
    if (e.key === 'a' || e.key === 'A') {
        if (!e.ctrlKey && !e.altKey) {
            e.preventDefault();
            toggleAutoPlay();
        }
    }
});

// Stop auto-play on user interaction
['click', 'keydown', 'touchstart'].forEach(eventType => {
    document.addEventListener(eventType, (e) => {
        // Don't stop if it's the auto-play toggle key
        if (e.type === 'keydown' && (e.key === 'a' || e.key === 'A')) {
            return;
        }
        stopAutoPlay();
    }, { passive: true });
});

/**
 * Progress indicator (optional enhancement)
 */
function createProgressIndicator() {
    const indicator = document.createElement('div');
    indicator.className = 'progress-indicator';
    indicator.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 0%;
        height: 4px;
        background: linear-gradient(90deg, #2563eb 0%, #3b82f6 100%);
        transition: width 0.3s ease;
        z-index: 9999;
    `;
    document.body.appendChild(indicator);

    // Update progress
    function updateProgress() {
        const progress = ((currentSlide - 1) / (totalSlides - 1)) * 100;
        indicator.style.width = progress + '%';
    }

    // Initial update
    updateProgress();

    // Update on slide change
    const originalGoToSlide = goToSlide;
    goToSlide = function(slideNumber) {
        originalGoToSlide.call(this, slideNumber);
        updateProgress();
    };
}

/**
 * Console welcome message
 */
function showWelcomeMessage() {
    console.log('%c IconIK İK Yönetim Sistemi ', 'background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; font-size: 20px; font-weight: bold; padding: 10px;');
    console.log('%c 360° İnsan Kaynakları Süreç Yönetimi Platformu ', 'color: #2563eb; font-size: 14px; font-weight: bold;');
    console.log('%c LionSoft Technology tarafından geliştirilmiştir ', 'color: #64748b; font-size: 12px; font-style: italic;');
    console.log('');
    console.log('Klavye kısayolları için H tuşuna basın.');
    console.log('');
    console.log('- Toplam Slayt:', totalSlides);
}

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', () => {
    initPresentation();
    createProgressIndicator();
    showWelcomeMessage();

    console.log('✅ Sunum yüklendi ve hazır!');
});

// Prevent accidental page navigation
window.addEventListener('beforeunload', (e) => {
    if (currentSlide > 1) {
        e.preventDefault();
        e.returnValue = '';
    }
});

// Export functions for global access
window.presentationAPI = {
    goToSlide,
    navigateSlide,
    toggleAutoPlay,
    toggleFullscreen,
    printPresentation,
    showKeyboardShortcuts,
    getCurrentSlide: () => currentSlide,
    getTotalSlides: () => totalSlides
};
