using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TechShop.Models;

namespace TechShop.Controllers
{
    public class OrderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ==========================================
        // HELPER: Lấy giỏ hàng từ Session
        // ==========================================
        private List<SessionCartItem> GetCart()
        {
            var cart = Session["Cart"] as List<SessionCartItem>;
            if (cart == null || !cart.Any())
            {
                return new List<SessionCartItem>();
            }
            return cart;
        }

        // ==========================================
        // GET: Order/Checkout - Trang thanh toán
        // ==========================================
        public ActionResult Checkout()
        {
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null)
            {
                TempData["Warning"] = "Vui lòng đăng nhập để tiếp tục thanh toán";
                return RedirectToAction("Login", "Account",
                    new { returnUrl = Url.Action("Checkout", "Order") });
            }

            // Lấy giỏ hàng
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Warning"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Tính tổng tiền
            decimal totalAmount = cart.Sum(item => item.TotalPrice);
            decimal shippingFee = totalAmount >= 500000 ? 0 : 30000;

            // Lấy mã giảm giá từ session (nếu có)
            decimal discountAmount = Session["DiscountAmount"] != null
                ? (decimal)Session["DiscountAmount"]
                : 0;

            decimal finalAmount = totalAmount + shippingFee - discountAmount;

            // Lấy thông tin user
            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);

            // Khởi tạo order với thông tin user
            var order = new Order
            {
                CustomerName = user.FullName,
                CustomerPhone = user.PhoneNumber,
                CustomerEmail = user.Email,
                ShippingAddress = user.Address ?? ""
            };

            // Truyền dữ liệu qua ViewBag
            ViewBag.Cart = cart;
            ViewBag.TotalAmount = totalAmount;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.DiscountAmount = discountAmount;
            ViewBag.FinalAmount = finalAmount;
            ViewBag.CouponCode = Session["CouponCode"];

            return View(order);
        }

        // ==========================================
        // POST: Order/Checkout - Xử lý đặt hàng
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(Order order)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            // QUAN TRỌNG: Xóa lỗi validation cho các field do hệ thống tạo
            ModelState.Remove("OrderCode");
            ModelState.Remove("OrderID");
            ModelState.Remove("UserID");
            ModelState.Remove("OrderDate");
            ModelState.Remove("StatusID");
            ModelState.Remove("TotalAmount");
            ModelState.Remove("ShippingFee");
            ModelState.Remove("DiscountAmount");
            ModelState.Remove("FinalAmount");
            ModelState.Remove("PaymentStatus");

            // Validate thông tin bắt buộc
            if (string.IsNullOrWhiteSpace(order.CustomerName))
            {
                ModelState.AddModelError("CustomerName", "Vui lòng nhập họ tên");
            }
            if (string.IsNullOrWhiteSpace(order.CustomerPhone))
            {
                ModelState.AddModelError("CustomerPhone", "Vui lòng nhập số điện thoại");
            }
            if (string.IsNullOrWhiteSpace(order.ShippingAddress))
            {
                ModelState.AddModelError("ShippingAddress", "Vui lòng nhập địa chỉ giao hàng");
            }
            if (string.IsNullOrWhiteSpace(order.PaymentMethod))
            {
                ModelState.AddModelError("PaymentMethod", "Vui lòng chọn phương thức thanh toán");
            }

            if (!ModelState.IsValid)
            {
                // Trả lại dữ liệu nếu validation fail
                ViewBag.Cart = cart;
                ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
                ViewBag.ShippingFee = ViewBag.TotalAmount >= 500000 ? 0 : 30000;
                ViewBag.DiscountAmount = Session["DiscountAmount"] ?? 0;
                ViewBag.FinalAmount = ViewBag.TotalAmount + ViewBag.ShippingFee - ViewBag.DiscountAmount;

                return View(order);
            }

            try
            {
                int userId = (int)Session["UserID"];

                // Tính toán số tiền
                decimal totalAmount = cart.Sum(item => item.TotalPrice);
                decimal shippingFee = totalAmount >= 500000 ? 0 : 30000;
                decimal discountAmount = Session["DiscountAmount"] != null
                    ? (decimal)Session["DiscountAmount"]
                    : 0;

                // TẠO DỮ LIỆU CHO CÁC FIELD BẮT BUỘC
                order.OrderCode = "DH" + DateTime.Now.ToString("yyMMddHHmmss");
                order.UserID = userId;
                order.StatusID = 1; // Chờ xác nhận
                order.OrderDate = DateTime.Now;
                order.TotalAmount = totalAmount;
                order.ShippingFee = shippingFee;
                order.DiscountAmount = discountAmount;
                order.FinalAmount = totalAmount + shippingFee - discountAmount;
                order.PaymentStatus = "Chưa thanh toán";

                // Lưu đơn hàng
                db.Orders.Add(order);
                db.SaveChanges();

                // Thêm chi tiết đơn hàng
                foreach (var item in cart)
                {
                    var product = db.Products.Find(item.ProductID);

                    // Kiểm tra tồn kho
                    if (product == null)
                    {
                        throw new Exception($"Sản phẩm {item.ProductName} không tồn tại");
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        throw new Exception($"Sản phẩm {item.ProductName} không đủ hàng trong kho");
                    }

                    var orderDetail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        ProductID = item.ProductID,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price,
                        TotalPrice = item.TotalPrice
                    };
                    db.OrderDetails.Add(orderDetail);

                    // Trừ tồn kho
                    product.StockQuantity -= item.Quantity;
                }

                // Lưu coupon usage nếu có
                if (Session["CouponID"] != null)
                {
                    var couponUsage = new CouponUsage
                    {
                        CouponID = (int)Session["CouponID"],
                        UserID = userId,
                        OrderID = order.OrderID,
                        DiscountAmount = discountAmount,
                        UsedDate = DateTime.Now
                    };
                    db.CouponUsages.Add(couponUsage);

                    // Cập nhật số lần sử dụng coupon
                    var coupon = db.Coupons.Find((int)Session["CouponID"]);
                    if (coupon != null)
                    {
                        coupon.UsedCount++;
                    }
                }

                // Ghi log hoạt động
                try
                {
                    var activityLog = new ActivityLog
                    {
                        UserID = userId,
                        Action = "Đặt hàng",
                        TableName = "Orders",
                        RecordID = order.OrderID,
                        NewValue = $"Đơn hàng {order.OrderCode} - Tổng tiền: {order.FinalAmount:N0} đ",
                        IPAddress = Request.UserHostAddress,
                        CreatedDate = DateTime.Now
                    };
                    db.ActivityLogs.Add(activityLog);
                }
                catch { } // Bỏ qua lỗi log

                db.SaveChanges();

                // Xóa giỏ hàng và coupon khỏi session
                Session["Cart"] = null;
                Session["CouponCode"] = null;
                Session["DiscountAmount"] = null;
                Session["CouponID"] = null;

                TempData["Success"] = "Đặt hàng thành công!";
                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;

                // Trả lại dữ liệu
                ViewBag.Cart = cart;
                ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
                ViewBag.ShippingFee = ViewBag.TotalAmount >= 500000 ? 0 : 30000;
                ViewBag.DiscountAmount = Session["DiscountAmount"] ?? 0;
                ViewBag.FinalAmount = ViewBag.TotalAmount + ViewBag.ShippingFee - ViewBag.DiscountAmount;

                return View(order);
            }
        }

        // ==========================================
        // GET: Order/OrderSuccess - Đặt hàng thành công
        // ==========================================
        public ActionResult OrderSuccess(int id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];

            var order = db.Orders
                .Include(o => o.Status)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // ==========================================
        // GET: Order/MyOrders - Đơn hàng của tôi
        // ==========================================
        public ActionResult MyOrders()
        {
            if (Session["UserID"] == null)
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem đơn hàng";
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            var orders = db.Orders
                .Include(o => o.Status)
                .Include(o => o.OrderDetails)
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // ==========================================
        // GET: Order/OrderDetail - Chi tiết đơn hàng
        // ==========================================
        public ActionResult OrderDetail(int id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            var order = db.Orders
                .Include(o => o.Status)
                .Include(o => o.OrderDetails.Select(od => od.Product.ProductImages))
                .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("MyOrders");
            }

            return View(order);
        }

        // ==========================================
        // POST: Order/CancelOrder - Hủy đơn hàng
        // ==========================================
        [HttpPost]
        public JsonResult CancelOrder(int id, string reason)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                int userId = (int)Session["UserID"];
                var order = db.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Chỉ cho phép hủy đơn ở trạng thái "Chờ xác nhận"
                if (order.StatusID != 1)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Chỉ có thể hủy đơn hàng đang chờ xác nhận"
                    });
                }

                order.StatusID = 6; // Đã hủy
                order.CancelledDate = DateTime.Now;
                order.CancelReason = reason;

                // Hoàn lại tồn kho
                foreach (var detail in order.OrderDetails)
                {
                    var product = db.Products.Find(detail.ProductID);
                    if (product != null)
                    {
                        product.StockQuantity += detail.Quantity;
                    }
                }

                // Ghi log
                try
                {
                    var activityLog = new ActivityLog
                    {
                        UserID = userId,
                        Action = "Hủy đơn hàng",
                        TableName = "Orders",
                        RecordID = order.OrderID,
                        NewValue = $"Lý do: {reason}",
                        IPAddress = Request.UserHostAddress,
                        CreatedDate = DateTime.Now
                    };
                    db.ActivityLogs.Add(activityLog);
                }
                catch { }

                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Đã hủy đơn hàng thành công"
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