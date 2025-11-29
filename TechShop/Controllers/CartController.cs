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

        // Lấy giỏ hàng từ Session
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

        // Hiển thị giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity = 1)
        {
            var product = db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductID == productId);

            if (product == null || !product.IsActive)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            if (product.StockQuantity < quantity)
            {
                return Json(new { success = false, message = "Số lượng tồn kho không đủ" });
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.ProductID == productId);

            if (cartItem != null)
            {
                // Kiểm tra tồn kho trước khi tăng số lượng
                if (product.StockQuantity < cartItem.Quantity + quantity)
                {
                    return Json(new { success = false, message = "Số lượng tồn kho không đủ" });
                }
                cartItem.Quantity += quantity;
            }
            else
            {
                // Thêm sản phẩm mới vào giỏ
                var primaryImage = product.ProductImages?.FirstOrDefault(i => i.IsPrimary);
                cart.Add(new SessionCartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Price = product.DisplayPrice, // Sử dụng giá sau giảm nếu có
                    Quantity = quantity,
                    ImageUrl = primaryImage?.ImageURL ?? "/Content/Images/no-image.jpg"
                });
            }

            Session["Cart"] = cart;
            return Json(new
            {
                success = true,
                message = "Đã thêm vào giỏ hàng",
                cartCount = cart.Sum(c => c.Quantity)
            });
        }

        // Cập nhật số lượng
        [HttpPost]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(c => c.ProductID == productId);

            if (cartItem != null)
            {
                if (quantity > 0)
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
                            message = "Số lượng vượt quá tồn kho",
                            cartCount = cart.Sum(c => c.Quantity)
                        });
                    }
                }
                else
                {
                    cart.Remove(cartItem);
                }
                Session["Cart"] = cart;
            }

            return Json(new
            {
                success = true,
                totalAmount = cart.Sum(item => item.TotalPrice),
                cartCount = cart.Sum(c => c.Quantity)
            });
        }

        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
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
                cartCount = cart.Sum(c => c.Quantity),
                totalAmount = cart.Sum(item => item.TotalPrice)
            });
        }

        // Xóa toàn bộ giỏ hàng
        public ActionResult ClearCart()
        {
            Session["Cart"] = null;
            return RedirectToAction("Index");
        }

        // Lấy số lượng sản phẩm trong giỏ
        public ActionResult GetCartCount()
        {
            var cart = GetCart();
            return Json(new { count = cart.Sum(c => c.Quantity) }, JsonRequestBehavior.AllowGet);
        }

        // Áp dụng mã giảm giá
        [HttpPost]
        public ActionResult ApplyCoupon(string couponCode)
        {
            if (string.IsNullOrEmpty(couponCode))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá" });
            }

            var coupon = db.Coupons.FirstOrDefault(c =>
                c.CouponCode == couponCode &&
                c.IsActive &&
                c.StartDate <= DateTime.Now &&
                c.EndDate >= DateTime.Now
            );

            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn" });
            }

            // Kiểm tra số lần sử dụng
            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết lượt sử dụng" });
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
                if (coupon.MaxDiscountAmount.HasValue && discountAmount > coupon.MaxDiscountAmount.Value)
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

            return Json(new
            {
                success = true,
                message = "Áp dụng mã giảm giá thành công",
                discountAmount = discountAmount,
                finalAmount = totalAmount - discountAmount
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