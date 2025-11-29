using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TechShop.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using TechShop.Models;

namespace TechShop.Controllers
{
    public class OrderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Trang thanh toán
        public ActionResult Checkout()
        {
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Order") });
            }

            // Lấy giỏ hàng
            var cart = Session["Cart"] as System.Collections.Generic.List<SessionCartItem>;
            if (cart == null || !cart.Any())
            {
                TempData["Message"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            // Tính tổng tiền
            ViewBag.TotalAmount = cart.Sum(item => item.TotalPrice);
            ViewBag.ShippingFee = 30000; // Phí ship cố định
            ViewBag.FinalAmount = ViewBag.TotalAmount + ViewBag.ShippingFee;

            // Lấy thông tin user
            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);

            var order = new Order
            {
                CustomerName = user.FullName,
                CustomerPhone = user.PhoneNumber,
                CustomerEmail = user.Email,
                ShippingAddress = user.Address
            };

            return View(order);
        }

        // Xử lý đặt hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(Order order, string paymentMethod)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = Session["Cart"] as System.Collections.Generic.List<SessionCartItem>;
            if (cart == null || !cart.Any())
            {
                TempData["Message"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    int userId = (int)Session["UserID"];

                    // Tạo mã đơn hàng
                    order.OrderCode = "DH" + DateTime.Now.ToString("yyMMddHHmmss");
                    order.UserID = userId;
                    order.StatusID = 1; // Chờ xác nhận
                    order.OrderDate = DateTime.Now;
                    order.PaymentMethod = paymentMethod;
                    order.PaymentStatus = "Chưa thanh toán";

                    // Tính tổng tiền
                    order.TotalAmount = cart.Sum(item => item.TotalPrice);
                    order.ShippingFee = 30000;
                    order.DiscountAmount = 0;
                    order.FinalAmount = order.TotalAmount + order.ShippingFee;

                    db.Orders.Add(order);
                    db.SaveChanges();

                    // Thêm chi tiết đơn hàng
                    foreach (var item in cart)
                    {
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

                        // Trừ số lượng tồn kho
                        var product = db.Products.Find(item.ProductID);
                        if (product != null)
                        {
                            product.StockQuantity -= item.Quantity;
                        }
                    }

                    db.SaveChanges();

                    // Xóa giỏ hàng
                    Session["Cart"] = null;

                    return RedirectToAction("OrderSuccess", new { id = order.OrderID });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            return View(order);
        }

        // Trang đặt hàng thành công
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders
                .Include("OrderDetails")
                .Include("Status")
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // Đơn hàng của tôi
        public ActionResult MyOrders()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            var orders = db.Orders
                .Include("Status")
                .Include("OrderDetails")
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // Chi tiết đơn hàng
        public ActionResult OrderDetail(int id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];
            var order = db.Orders
                .Include("Status")
                .Include("OrderDetails.Product")
                .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        // Hủy đơn hàng
        [HttpPost]
        public ActionResult CancelOrder(int id, string reason)
        {
            if (Session["UserID"] == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            int userId = (int)Session["UserID"];
            var order = db.Orders.FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Chỉ cho phép hủy đơn ở trạng thái "Chờ xác nhận"
            if (order.StatusID != 1)
            {
                return Json(new { success = false, message = "Không thể hủy đơn hàng ở trạng thái này" });
            }

            order.StatusID = 6; // Đã hủy
            order.CancelledDate = DateTime.Now;
            order.CancelReason = reason;

            // Hoàn lại số lượng tồn kho
            foreach (var detail in order.OrderDetails)
            {
                var product = db.Products.Find(detail.ProductID);
                if (product != null)
                {
                    product.StockQuantity += detail.Quantity;
                }
            }

            db.SaveChanges();

            return Json(new { success = true, message = "Đã hủy đơn hàng" });
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