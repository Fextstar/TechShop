// ==========================================
// CART.JS - Xử lý giỏ hàng toàn cục
// ==========================================

// ==========================================
// THÊM SẢN PHẨM VÀO GIỎ HÀNG
// ==========================================
function addToCart(productID, quantity = 1) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { productId: productID, quantity: quantity },
        success: function (data) {
            if (data.success) {
                // Cập nhật số lượng giỏ hàng
                updateCartCount(data.cartCount);

                // Hiển thị thông báo
                showNotification('success', data.message);

                // Thêm hiệu ứng cho icon giỏ hàng
                animateCartIcon();
            } else {
                showNotification('error', data.message);
            }
        },
        error: function () {
            showNotification('error', 'Có lỗi xảy ra, vui lòng thử lại');
        }
    });
}

// ==========================================
// CẬP NHẬT SỐ LƯỢNG SẢN PHẨM
// ==========================================
function updateQuantity(productID, quantity) {
    quantity = parseInt(quantity);

    if (quantity < 1) {
        removeFromCart(productID);
        return;
    }

    $.ajax({
        url: '/Cart/UpdateQuantity',
        type: 'POST',
        data: { productId: productID, quantity: quantity },
        success: function (data) {
            if (data.success) {
                // Cập nhật số lượng trong input
                $('#quantity-' + productID).val(quantity);

                // Cập nhật tổng tiền của sản phẩm
                $('#total-' + productID).text(data.itemTotal.toLocaleString('vi-VN') + ' đ');

                // Cập nhật tổng tiền giỏ hàng
                updateCartSummary(data);

                // Cập nhật số lượng icon giỏ hàng
                updateCartCount(data.cartCount);
            } else {
                showNotification('error', data.message);
            }
        },
        error: function () {
            showNotification('error', 'Có lỗi xảy ra, vui lòng thử lại');
        }
    });
}

// ==========================================
// XÓA SẢN PHẨM KHỎI GIỎ HÀNG
// ==========================================
function removeFromCart(productID) {
    if (!confirm('Bạn có chắc muốn xóa sản phẩm này?')) {
        return;
    }

    $.ajax({
        url: '/Cart/RemoveFromCart',
        type: 'POST',
        data: { productId: productID },
        success: function (data) {
            if (data.success) {
                // Xóa dòng sản phẩm với hiệu ứng
                $('#cart-item-' + productID).fadeOut(300, function () {
                    $(this).remove();

                    // Kiểm tra giỏ hàng trống
                    if ($('tbody tr').length == 0) {
                        location.reload();
                    }
                });

                // Cập nhật tổng tiền
                updateCartSummary(data);

                // Cập nhật số lượng icon
                updateCartCount(data.cartCount);

                showNotification('success', 'Đã xóa sản phẩm khỏi giỏ hàng');
            }
        },
        error: function () {
            showNotification('error', 'Có lỗi xảy ra, vui lòng thử lại');
        }
    });
}

// ==========================================
// XÓA TOÀN BỘ GIỎ HÀNG
// ==========================================
function clearCart() {
    if (confirm('Bạn có chắc muốn xóa toàn bộ giỏ hàng?')) {
        window.location.href = '/Cart/ClearCart';
    }
}

// ==========================================
// ÁP DỤNG MÃ GIẢM GIÁ
// ==========================================
function applyCoupon() {
    var couponCode = $('#couponCode').val().trim();

    if (!couponCode) {
        showNotification('error', 'Vui lòng nhập mã giảm giá');
        return;
    }

    $.ajax({
        url: '/Cart/ApplyCoupon',
        type: 'POST',
        data: { couponCode: couponCode },
        success: function (data) {
            if (data.success) {
                showNotification('success', data.message);
                // Reload để hiển thị mã đã áp dụng
                setTimeout(function () {
                    location.reload();
                }, 1000);
            } else {
                showNotification('error', data.message);
            }
        },
        error: function () {
            showNotification('error', 'Có lỗi xảy ra, vui lòng thử lại');
        }
    });
}

// ==========================================
// XÓA MÃ GIẢM GIÁ
// ==========================================
function removeCoupon() {
    $.ajax({
        url: '/Cart/RemoveCoupon',
        type: 'POST',
        success: function (data) {
            if (data.success) {
                location.reload();
            }
        }
    });
}

// ==========================================
// CẬP NHẬT TỔNG TIỀN GIỎ HÀNG
// ==========================================
function updateCartSummary(data) {
    // Cập nhật tạm tính
    $('#subtotal').text(data.totalAmount.toLocaleString('vi-VN') + ' đ');

    // Tính phí ship (miễn phí nếu >= 500k)
    var shippingFee = data.totalAmount >= 500000 ? 0 : 30000;

    if (shippingFee == 0) {
        $('#shipping-fee').html('<span class="text-success">Miễn phí</span>');
    } else {
        $('#shipping-fee').text(shippingFee.toLocaleString('vi-VN') + ' đ');
    }

    // Lấy discount từ session (nếu có)
    var discountAmount = parseFloat($('#discount-amount').text().replace(/[^\d]/g, '')) || 0;

    // Tính tổng cuối
    var finalAmount = data.totalAmount + shippingFee - discountAmount;
    $('#final-total').text(finalAmount.toLocaleString('vi-VN') + ' đ');

    // Cập nhật thông báo miễn phí ship
    updateShippingNotice(data.totalAmount);
}

// ==========================================
// CẬP NHẬT THÔNG BÁO MIỄN PHÍ SHIP
// ==========================================
function updateShippingNotice(totalAmount) {
    var notice = $('#shipping-notice');

    if (totalAmount >= 500000) {
        notice.removeClass('alert-info').addClass('alert-success');
        notice.html('<i class="fas fa-truck"></i> Đơn hàng được miễn phí vận chuyển');
    } else {
        var remaining = 500000 - totalAmount;
        notice.removeClass('alert-success').addClass('alert-info');
        notice.html('<i class="fas fa-info-circle"></i> Mua thêm ' +
            remaining.toLocaleString('vi-VN') + ' đ để miễn phí ship');
    }
}

// ==========================================
// CẬP NHẬT SỐ LƯỢNG ICON GIỎ HÀNG
// ==========================================
function updateCartCount(count) {
    $('#cart-count').text(count);
}

// ==========================================
// HIỆU ỨNG ICON GIỎ HÀNG
// ==========================================
function animateCartIcon() {
    var cartIcon = $('.fa-shopping-cart').closest('.nav-link');
    cartIcon.addClass('animate__animated animate__rubberBand');

    setTimeout(function () {
        cartIcon.removeClass('animate__animated animate__rubberBand');
    }, 1000);
}

// ==========================================
// HIỂN THỊ THÔNG BÁO
// ==========================================
function showNotification(type, message) {
    var className = type === 'success' ? 'alert-success' : 'alert-danger';
    var icon = type === 'success' ? 'fa-check-circle' : 'fa-exclamation-circle';

    var html = `
        <div class="alert ${className} alert-dismissible fade show cart-notification" 
             style="position: fixed; top: 80px; right: 20px; z-index: 9999; 
                    min-width: 300px; box-shadow: 0 4px 12px rgba(0,0,0,0.15);">
            <i class="fas ${icon}"></i> ${message}
            <button type="button" class="close" data-dismiss="alert">&times;</button>
        </div>
    `;

    // Xóa thông báo cũ
    $('.cart-notification').remove();

    // Thêm thông báo mới
    $('body').append(html);

    // Tự động ẩn sau 3 giây
    setTimeout(function () {
        $('.cart-notification').fadeOut(300, function () {
            $(this).remove();
        });
    }, 3000);
}

// ==========================================
// TẢI SỐ LƯỢNG GIỎ HÀNG KHI LOAD TRANG
// ==========================================
$(document).ready(function () {
    // Load cart count từ server
    $.ajax({
        url: '/Cart/GetCartCount',
        type: 'GET',
        success: function (data) {
            if (data && data.count !== undefined) {
                updateCartCount(data.count);
            }
        }
    });

    // Auto-hide alerts - CHỈ ẨN NOTIFICATION, KHÔNG ẨN PERSISTENT ALERT
    setTimeout(function () {
        // Chỉ ẩn các alert không có class 'persistent-alert'
        $('.alert:not(.cart-notification):not(.persistent-alert)').fadeOut('slow');
    }, 5000);
});