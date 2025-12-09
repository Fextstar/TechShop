using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechShop.Models;
using TechShop.Models.User___Authentication;

namespace TechShop.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Kiểm tra quyền admin
        private bool IsAdmin()
        {
            if (Session["UserID"] == null) return false;
            var roleName = Session["RoleName"]?.ToString();
            return roleName == "Admin" || roleName == "Manager";
        }

        // Dashboard
        public ActionResult Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var model = new DashboardViewModel
            {
                TotalProducts = db.Products.Count(p => p.IsActive),
                TotalOrders = db.Orders.Count(),
                TotalCustomers = db.Users.Count(u => u.IsActive),
                MonthlyRevenue = db.Orders
                    .Where(o => o.Status.StatusName == "Đã giao"
                             && o.DeliveredDate.Value.Month == DateTime.Now.Month)
                    .Sum(o => (decimal?)o.FinalAmount) ?? 0,

                PendingOrders = db.Orders
                    .Count(o => o.Status.StatusName == "Chờ xác nhận"),

                LowStockProducts = db.Products
                    .Count(p => p.StockQuantity <= p.MinStockLevel && p.IsActive)
            };

            return View(model);
        }



        // ============ QUẢN LÝ SẢN PHẨM ============

        // Danh sách sản phẩm
        public ActionResult Products(string search, int? categoryId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search) || p.SKU.Contains(search));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
            }

            ViewBag.Categories = db.Categories.Where(c => c.IsActive).ToList();
            return View(products.OrderByDescending(p => p.CreatedDate).ToList());
        }

        // Tạo sản phẩm mới
        public ActionResult CreateProduct()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName");
            ViewBag.Brands = new SelectList(db.Brands.Where(b => b.IsActive), "BrandID", "BrandName");
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "SupplierID", "SupplierName");

            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(Product product, HttpPostedFileBase imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                product.CreatedDate = DateTime.Now;
                product.IsActive = true;
                product.ViewCount = 0;

                db.Products.Add(product);
                db.SaveChanges();

                // Upload hình ảnh
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    var fileName = product.ProductID + "_" + DateTime.Now.Ticks + System.IO.Path.GetExtension(imageFile.FileName);
                    var path = Server.MapPath("~/Content/Uploads/Products/");

                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }

                    imageFile.SaveAs(path + fileName);

                    var productImage = new ProductImage
                    {
                        ProductID = product.ProductID,
                        ImageURL = "/Content/Uploads/Products/" + fileName,
                        IsPrimary = true,
                        DisplayOrder = 0,
                        CreatedDate = DateTime.Now
                    };

                    db.ProductImages.Add(productImage);
                    db.SaveChanges();
                }

                TempData["Message"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Products");
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.Brands = new SelectList(db.Brands.Where(b => b.IsActive), "BrandID", "BrandName", product.BrandID);
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "SupplierID", "SupplierName", product.SupplierID);

            return View(product);
        }

        // Sửa sản phẩm
        public ActionResult EditProduct(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.Brands = new SelectList(db.Brands.Where(b => b.IsActive), "BrandID", "BrandName", product.BrandID);
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "SupplierID", "SupplierName", product.SupplierID);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(Product product)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                var existingProduct = db.Products.Find(product.ProductID);
                if (existingProduct == null) return HttpNotFound();

                existingProduct.ProductName = product.ProductName;
                existingProduct.CategoryID = product.CategoryID;
                existingProduct.BrandID = product.BrandID;
                existingProduct.SupplierID = product.SupplierID;
                existingProduct.SKU = product.SKU;
                existingProduct.Description = product.Description;
                existingProduct.Specifications = product.Specifications;
                existingProduct.Price = product.Price;
                existingProduct.DiscountPrice = product.DiscountPrice;
                existingProduct.StockQuantity = product.StockQuantity;
                existingProduct.MinStockLevel = product.MinStockLevel;
                existingProduct.Weight = product.Weight;
                existingProduct.Warranty = product.Warranty;
                existingProduct.IsActive = product.IsActive;
                existingProduct.IsFeatured = product.IsFeatured;
                existingProduct.UpdatedDate = DateTime.Now;

                db.SaveChanges();
                TempData["Message"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Products");
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.Brands = new SelectList(db.Brands.Where(b => b.IsActive), "BrandID", "BrandName", product.BrandID);
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "SupplierID", "SupplierName", product.SupplierID);

            return View(product);
        }

        // Xóa sản phẩm
        [HttpPost]
        public ActionResult DeleteProduct(int id)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            var product = db.Products.Find(id);
            if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm" });

            product.IsActive = false;
            db.SaveChanges();

            return Json(new { success = true, message = "Xóa sản phẩm thành công" });
        }

        // ============ QUẢN LÝ ĐƠN HÀNG ============

        public ActionResult Orders(string status, string search)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var orders = db.Orders
                .Include(o => o.User)
                .Include(o => o.Status)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status.StatusName == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o => o.OrderCode.Contains(search)
                                      || o.CustomerName.Contains(search)
                                      || o.CustomerPhone.Contains(search));
            }

            ViewBag.OrderStatuses = db.OrderStatuses.OrderBy(s => s.DisplayOrder).ToList();
            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        // Chi tiết đơn hàng admin
        public ActionResult OrderDetail(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var order = db.Orders
                .Include(o => o.User)
                .Include(o => o.Status)
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null) return HttpNotFound();

            ViewBag.OrderStatuses = new SelectList(db.OrderStatuses.OrderBy(s => s.DisplayOrder), "StatusID", "StatusName", order.StatusID);
            return View(order);
        }

        // Cập nhật trạng thái đơn hàng
        [HttpPost]
        public ActionResult UpdateOrderStatus(int orderId, int statusId)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            var order = db.Orders.Find(orderId);
            if (order == null) return Json(new { success = false, message = "Không tìm thấy đơn hàng" });

            var status = db.OrderStatuses.Find(statusId);
            if (status == null) return Json(new { success = false, message = "Trạng thái không hợp lệ" });

            order.StatusID = statusId;

            if (status.StatusName == "Đang giao" && !order.ShippedDate.HasValue)
            {
                order.ShippedDate = DateTime.Now;
            }
            else if (status.StatusName == "Đã giao" && !order.DeliveredDate.HasValue)
            {
                order.DeliveredDate = DateTime.Now;
                order.PaymentStatus = "Đã thanh toán";
            }
            else if (status.StatusName == "Đã hủy" && !order.CancelledDate.HasValue)
            {
                order.CancelledDate = DateTime.Now;
            }

            db.SaveChanges();
            return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        // ============ QUẢN LÝ NGƯỜI DÙNG ============

        public ActionResult Users(string search, int? roleId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var users = db.Users
                .Include(u => u.Role)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.Username.Contains(search)
                                    || u.FullName.Contains(search)
                                    || u.Email.Contains(search));
            }

            if (roleId.HasValue)
            {
                users = users.Where(u => u.RoleID == roleId.Value);
            }

            ViewBag.Roles = db.Roles.ToList();
            return View(users.OrderByDescending(u => u.CreatedDate).ToList());
        }

        // Khóa/Mở khóa user
        [HttpPost]
        public ActionResult ToggleUserStatus(int id)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền" });

            var user = db.Users.Find(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy người dùng" });

            user.IsActive = !user.IsActive;
            db.SaveChanges();

            return Json(new { success = true, isActive = user.IsActive });
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