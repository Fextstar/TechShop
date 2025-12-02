using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Linq;
using System.Web.Mvc;
using TechShop.Models;

namespace TechShop.Controllers
{
    public class CheckoutController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Checkout
        public ActionResult Index()
        {
            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Tính tổng tiền
            decimal subtotal = cartItems.Sum(c => c.Price * c.Quantity);
            ViewBag.Subtotal = subtotal;

            decimal shippingFee = subtotal >= 500000 ? 0 : 30000;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Total = subtotal + shippingFee;

            ViewBag.CartItems = cartItems;

            // Lấy thông tin user nếu đã đăng nhập
            if (Session["UserID"] != null)
            {
                int userId = (int)Session["UserID"];
                var user = db.Users.Find(userId);
                ViewBag.User = user;
            }

            return View();
        }

        // POST: Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(Order order)
        {
            var cartItems = GetCartItems();
            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            // Tính tổng tiền
            decimal subtotal = cartItems.Sum(c => c.Price * c.Quantity);
            decimal shippingFee = subtotal >= 500000 ? 0 : 30000;

            // Tạo đơn hàng
            order.OrderCode = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");
            order.TotalAmount = subtotal;
            order.ShippingFee = shippingFee;
            order.FinalAmount = subtotal + shippingFee - order.DiscountAmount;
            order.OrderDate = DateTime.Now;
            order.StatusID = 1; // Chờ xác nhận

            if (Session["UserID"] != null)
            {
                order.UserID = (int)Session["UserID"];
            }
            else
            {
                // Tạo tài khoản guest
                var guestUser = new User
                {
                    Username = "guest_" + DateTime.Now.Ticks,
                    Email = order.CustomerEmail,
                    Password = "N/A",
                    FullName = order.CustomerName,
                    PhoneNumber = order.CustomerPhone,
                    RoleID = 4, // Customer
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };
                db.Users.Add(guestUser);
                db.SaveChanges();
                order.UserID = guestUser.UserID;
            }

            db.Orders.Add(order);
            db.SaveChanges();

            // Tạo chi tiết đơn hàng
            foreach (var item in cartItems)
            {
                var product = db.Products.Find(item.ProductID);

                var orderDetail = new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = item.ProductID,
                    ProductName = product.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    TotalPrice = item.Price * item.Quantity
                };
                db.OrderDetails.Add(orderDetail);

                // Giảm tồn kho
                product.StockQuantity -= item.Quantity;
            }

            db.SaveChanges();

            // Xóa giỏ hàng
            ClearCart();

            TempData["Success"] = "Đặt hàng thành công! Mã đơn hàng: " + order.OrderCode;
            return RedirectToAction("OrderSuccess", new { id = order.OrderID });
        }

        // GET: Checkout/OrderSuccess
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            ViewBag.OrderDetails = db.OrderDetails
                .Where(d => d.OrderID == id)
                .ToList();

            return View(order);
        }

        // Helper methods
        private System.Collections.Generic.List<CartItem> GetCartItems()
        {
            if (Session["UserID"] != null)
            {
                int userId = (int)Session["UserID"];
                var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserID == userId);
                if (cart != null)
                {
                    return db.CartItems.Where(c => c.CartID == cart.CartID).ToList();
                }
            }
            else
            {
                return Session["Cart"] as System.Collections.Generic.List<CartItem>
                    ?? new System.Collections.Generic.List<CartItem>();
            }
            return new System.Collections.Generic.List<CartItem>();
        }

        private void ClearCart()
        {
            if (Session["UserID"] != null)
            {
                int userId = (int)Session["UserID"];
                var cart = db.ShoppingCarts.FirstOrDefault(c => c.UserID == userId);
                if (cart != null)
                {
                    var items = db.CartItems.Where(c => c.CartID == cart.CartID);
                    db.CartItems.RemoveRange(items);
                    db.SaveChanges();
                }
            }
            else
            {
                Session.Remove("Cart");
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