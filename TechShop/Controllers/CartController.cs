using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TechShop.Models;
using System.Data.Entity;

namespace TechShop.Controllers
{
    public class CartController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ==========================================
        // HELPER: Lấy giỏ hàng từ Session
        // ==========================================
        private List<SessionCartItem> GetCart()
        {
            var cart = Session["Cart"] as List<SessionCartItem>;
            if (cart == null)
            {
                cart = new List<SessionCartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // ==========================================
        // GET: Cart/Index - Hiển thị giỏ hàng
        // ==========================================
        public ActionResult Index()
        {
            var cart = GetCart();

            // Tính tổng tiền
            decimal totalAmount = cart.Sum(item => item.TotalPrice);

            // Tính phí ship (miễn phí nếu >= 500k)
            decimal shippingFee = totalAmount >= 500000 ? 0 : 30000;

            // Kiểm tra mã giảm giá từ Session
            decimal discountAmount = Session["DiscountAmount"] != null
                ? (decimal)Session["DiscountAmount"]
                : 0;

            ViewBag.TotalAmount = totalAmount;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.FinalAmount = totalAmount + shippingFee - discountAmount;

            return View(cart);
        }

        // ==========================================
        // POST: Cart/AddToCart - Thêm sản phẩm vào giỏ
        // ==========================================
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                // Tìm sản phẩm
                var product = db.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefault(p => p.ProductID == productId);

                if (product == null || !product.IsActive)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh"
                    });
                }

                // Kiểm tra tồn kho
                if (product.StockQuantity < quantity)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho"
                    });
                }

                var cart = GetCart();
                var existingItem = cart.FirstOrDefault(c => c.ProductID == productId);

                if (existingItem != null)
                {
                    // Kiểm tra tồn kho trước khi tăng
                    int newQuantity = existingItem.Quantity + quantity;
                    if (product.StockQuantity < newQuantity)
                    {
                        return Json(new
                        {
                            success = false,
                            message = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho"
                        });
                    }
                    existingItem.Quantity = newQuantity;
                }
                else
                {
                    // Thêm mới
                    var primaryImage = product.ProductImages?
                        .FirstOrDefault(i => i.IsPrimary);

                    cart.Add(new SessionCartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Price = product.DisplayPrice, // Ưu tiên giá giảm
                        Quantity = quantity,
                        ImageURL = primaryImage?.ImageURL ?? "/Content/Images/no-image.jpg"
                    });
                }

                Session["Cart"] = cart;

                return Json(new
                {
                    success = true,
                    message = "Đã thêm sản phẩm vào giỏ hàng",
                    cartCount = cart.Sum(c => c.Quantity),
                    cartTotal = cart.Sum(c => c.TotalPrice)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        // ==========================================
        // POST: Cart/UpdateQuantity - Cập nhật số lượng
        // ==========================================
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var cart = GetCart();
                var cartItem = cart.FirstOrDefault(c => c.ProductID == productId);

                if (cartItem == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Sản phẩm không có trong giỏ hàng"
                    });
                }

                if (quantity <= 0)
                {
                    // Xóa sản phẩm nếu quantity = 0
                    cart.Remove(cartItem);
                }
                else
                {
                    // Kiểm tra tồn kho
                    var product = db.Products.Find(productId);
                    if (product != null && product.StockQuantity >= quantity)
                    {
                        cartItem.Quantity = quantity;
                    }
                    else
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Số lượng vượt quá tồn kho"
                        });
                    }
                }

                Session["Cart"] = cart;

                return Json(new
                {
                    success = true,
                    cartCount = cart.Sum(c => c.Quantity),
                    totalAmount = cart.Sum(item => item.TotalPrice),
                    itemTotal = cartItem?.TotalPrice ?? 0
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        // ==========================================
        // POST: Cart/RemoveFromCart - Xóa sản phẩm khỏi giỏ
        // ==========================================
        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            try
            {
                var cart = GetCart();
                var cartItem = cart.FirstOrDefault(c => c.ProductID == productId);

                if (cartItem != null)
                {
                    cart.Remove(cartItem);
                    Session["Cart"] = cart;
                }

                return Json(new
                {
                    success = true,
                    message = "Đã xóa sản phẩm khỏi giỏ hàng",
                    cartCount = cart.Sum(c => c.Quantity),
                    totalAmount = cart.Sum(item => item.TotalPrice)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                });
            }
        }

        // ==========================================
        // GET: Cart/ClearCart - Xóa toàn bộ giỏ hàng
        // ==========================================
        public ActionResult ClearCart()
        {
            Session["Cart"] = null;
            Session["DiscountAmount"] = null;
            Session["CouponCode"] = null;

            TempData["Info"] = "Đã xóa toàn bộ giỏ hàng";
            return RedirectToAction("Index");
        }

        // ==========================================
        // GET: Cart/GetCartCount - Lấy số lượng sản phẩm (AJAX)
        // ==========================================
        public ActionResult GetCartCount()
        {
            var cart = GetCart();
            return Json(new
            {
                count = cart.Sum(c => c.Quantity)
            }, JsonRequestBehavior.AllowGet);
        }

        // ==========================================
        // POST: Cart/ApplyCoupon - Áp dụng mã giảm giá
        // ==========================================
        [HttpPost]
        public ActionResult ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                return Json(new
                {
                    success = false,
                    message = "Vui lòng nhập mã giảm giá"
                });
            }

            // Tìm mã giảm giá
            var coupon = db.Coupons.FirstOrDefault(c =>
                c.CouponCode.ToLower() == couponCode.ToLower() &&
                c.IsActive &&
                c.StartDate <= DateTime.Now &&
                c.EndDate >= DateTime.Now
            );

            if (coupon == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Mã giảm giá không hợp lệ hoặc đã hết hạn"
                });
            }

            // Kiểm tra số lần sử dụng
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            {
                return Json(new
                {
                    success = false,
                    message = "Mã giảm giá đã hết lượt sử dụng"
                });
            }

            var cart = GetCart();
            var totalAmount = cart.Sum(item => item.TotalPrice);

            // Kiểm tra giá trị đơn hàng tối thiểu
            if (totalAmount < coupon.MinOrderAmount)
            {
                return Json(new
                {
                    success = false,
                    message = $"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0} đ để sử dụng mã này"
                });
            }

            // Tính số tiền giảm
            decimal discountAmount = 0;

            if (coupon.DiscountType == "Percentage")
            {
                discountAmount = totalAmount * coupon.DiscountValue / 100;

                // Giới hạn giảm tối đa
                if (coupon.MaxDiscountAmount.HasValue &&
                    discountAmount > coupon.MaxDiscountAmount.Value)
                {
                    discountAmount = coupon.MaxDiscountAmount.Value;
                }
            }
            else // FixedAmount
            {
                discountAmount = coupon.DiscountValue;
            }

            // Lưu vào Session
            Session["CouponCode"] = couponCode;
            Session["DiscountAmount"] = discountAmount;
            Session["CouponID"] = coupon.CouponID;

            decimal shippingFee = totalAmount >= 500000 ? 0 : 30000;
            decimal finalAmount = totalAmount + shippingFee - discountAmount;

            return Json(new
            {
                success = true,
                message = "Áp dụng mã giảm giá thành công!",
                discountAmount = discountAmount,
                finalAmount = finalAmount
            });
        }

        // ==========================================
        // POST: Cart/RemoveCoupon - Xóa mã giảm giá
        // ==========================================
        [HttpPost]
        public ActionResult RemoveCoupon()
        {
            Session["CouponCode"] = null;
            Session["DiscountAmount"] = null;
            Session["CouponID"] = null;

            return Json(new
            {
                success = true,
                message = "Đã xóa mã giảm giá"
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}