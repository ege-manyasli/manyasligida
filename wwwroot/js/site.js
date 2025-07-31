// Manyaslı Gıda - Site JavaScript

// Document ready function
$(document).ready(function() {
    // Initialize Bootstrap components
    initializeBootstrap();
    
    // Initialize navbar functionality
    initializeNavbar();
    
    // Initialize scroll effects
    initializeScrollEffects();
    
    // Initialize cart functionality
    initializeCart();
    
    // Initialize toast notifications
    initializeToasts();
});

// Bootstrap initialization
function initializeBootstrap() {
    // Initialize all tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Initialize all popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });
}

// Navbar functionality
function initializeNavbar() {
    // Mobile menu toggle
    $('.navbar-toggler').on('click', function() {
        $('.navbar-collapse').toggleClass('show');
    });
    
    // Close mobile menu when clicking on a link
    $('.navbar-nav .nav-link').on('click', function() {
        $('.navbar-collapse').removeClass('show');
    });
    
    // Close mobile menu when clicking outside
    $(document).on('click', function(e) {
        if (!$(e.target).closest('.navbar').length) {
            $('.navbar-collapse').removeClass('show');
        }
    });
    
    // Active link highlighting
    highlightActiveLink();
}

// Highlight active navigation link
function highlightActiveLink() {
    var currentPath = window.location.pathname;
    $('.navbar-nav .nav-link').each(function() {
        var linkPath = $(this).attr('href');
        if (linkPath && currentPath.includes(linkPath.replace('/Home', ''))) {
            $(this).addClass('active');
        }
    });
}

// Scroll effects
function initializeScrollEffects() {
    // Navbar scroll effect
    $(window).scroll(function() {
        var scrollTop = $(window).scrollTop();
        
        if (scrollTop > 50) {
            $('.header-fixed').addClass('scrolled');
        } else {
            $('.header-fixed').removeClass('scrolled');
        }
        
        // Parallax effect for hero section
        if ($('.hero-section').length) {
            var scrolled = $(window).scrollTop();
            var parallax = $('.hero-section');
            var speed = 0.5;
            parallax.css('transform', 'translateY(' + (scrolled * speed) + 'px)');
        }
    });
    
    // Smooth scrolling for anchor links
    $('a[href^="#"]').on('click', function(e) {
        e.preventDefault();
        var target = $(this.getAttribute('href'));
        if (target.length) {
            $('html, body').stop().animate({
                scrollTop: target.offset().top - 80
            }, 1000);
        }
    });
}

// Cart functionality
function initializeCart() {
    // Update cart count
    updateCartCount();
    
    // Add to cart functionality
    $('.add-to-cart-btn').on('click', function(e) {
        e.preventDefault();
        var productId = $(this).data('product-id');
        var quantity = $(this).data('quantity') || 1;
        addToCart(productId, quantity);
    });
}

// Update cart count
function updateCartCount() {
    $.get('/Cart/GetCartCount')
        .done(function(response) {
            $('.cart-count').text(response.cartItemCount || 0);
        })
        .fail(function() {
            console.log('Failed to update cart count');
        });
}

// Add to cart function
function addToCart(productId, quantity = 1) {
    var btn = $('[data-product-id="' + productId + '"]');
    var originalText = btn.html();
    
    btn.prop('disabled', true).html('<span class="loading"></span>');
    
    $.post('/Cart/AddToCart', { productId: productId, quantity: quantity })
        .done(function(response) {
            if (response.success) {
                updateCartCount();
                showToast(response.message, 'success');
            } else {
                showToast(response.message, 'error');
            }
        })
        .fail(function() {
            showToast('Bir hata oluştu', 'error');
        })
        .always(function() {
            btn.prop('disabled', false).html(originalText);
        });
}

// Toast notifications
function initializeToasts() {
    // Show TempData messages
    if (typeof tempDataSuccess !== 'undefined' && tempDataSuccess) {
        showToast(tempDataSuccess, 'success');
    }
    if (typeof tempDataError !== 'undefined' && tempDataError) {
        showToast(tempDataError, 'error');
    }
    if (typeof tempDataInfo !== 'undefined' && tempDataInfo) {
        showToast(tempDataInfo, 'info');
    }
}

// Show toast notification
function showToast(message, type = 'info') {
    var iconClass = '';
    var bgClass = '';
    
    switch(type) {
        case 'success':
            iconClass = 'fas fa-check-circle text-success';
            bgClass = 'bg-success text-white';
            break;
        case 'error':
            iconClass = 'fas fa-exclamation-circle text-danger';
            bgClass = 'bg-danger text-white';
            break;
        case 'warning':
            iconClass = 'fas fa-exclamation-triangle text-warning';
            bgClass = 'bg-warning text-dark';
            break;
        default:
            iconClass = 'fas fa-info-circle text-info';
            bgClass = 'bg-info text-white';
    }
    
    var toast = $(`
        <div class="toast" role="alert">
            <div class="toast-header ${bgClass}">
                <i class="${iconClass} me-2"></i>
                <strong class="me-auto">Bildirim</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `);
    
    $('.toast-container').append(toast);
    var bsToast = new bootstrap.Toast(toast[0]);
    bsToast.show();
    
    toast.on('hidden.bs.toast', function() {
        $(this).remove();
    });
}

// Utility functions
function formatPrice(price) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(price);
}

function formatDate(date) {
    return new Intl.DateTimeFormat('tr-TR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(date));
}

// Loading animation
function showLoading(element) {
    element.prop('disabled', true).html('<span class="loading"></span>');
}

function hideLoading(element, originalText) {
    element.prop('disabled', false).html(originalText);
}

// AJAX error handler
$(document).ajaxError(function(event, xhr, settings, error) {
    console.log('AJAX Error:', error);
    showToast('Bir hata oluştu. Lütfen tekrar deneyin.', 'error');
});
