using System;
using System.Data.Entity;
using System.IO;
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

        // ==========================================
        // KIỂM TRA QUYỀN ADMIN/MANAGER
        // ==========================================
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Account", action = "Login", returnUrl = Request.RawUrl }
                    )
                );
                return;
            }

            // Kiểm tra quyền Admin hoặc Manager
            int roleId = Session["RoleID"] != null ? (int)Session["RoleID"] : 0;
            if (roleId != 1 && roleId != 2) // 1 = Admin, 2 = Manager
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang này.";
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Home", action = "Index" }
                    )
                );
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        // ==========================================
        // DASHBOARD
        // ==========================================
        public ActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalProducts = db.Products.Count(),
                TotalOrders = db.Orders.Count(),
                TotalCustomers = db.Users.Count(),
                MonthlyRevenue = db.Orders
                    .Where(o => o.Status.StatusName == "Đã giao")
                    .Sum(o => (decimal?)o.FinalAmount) ?? 0,
                PendingOrders = db.Orders.Count(o => o.Status.StatusName == "Chờ xác nhận"),
                LowStockProducts = db.Products.Count(p => p.StockQuantity < p.MinStockLevel),

                RecentOrders = db.Orders
                    .Include(o => o.Status)
                    .Include(o => o.User)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToList()
            };

            return View(model);
        }

        // ==========================================
        // QUẢN LÝ SẢN PHẨM
        // ==========================================

        // GET: Admin/Products
        public ActionResult Products(string search, int? categoryId)
        {
            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                products = products.Where(p =>
                    p.ProductName.ToLower().Contains(search) ||
                    p.SKU.ToLower().Contains(search)
                );
            }

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
            }

            // Danh mục cho filter
            ViewBag.Categories = db.Categories.Where(c => c.IsActive).ToList();

            return View(products.OrderByDescending(p => p.CreatedDate).ToList());
        }

        // GET: Admin/CreateProduct
        public ActionResult CreateProduct()
        {
            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName");
            ViewBag.Brands = new SelectList(db.Brands.Where(b => b.IsActive), "BrandID", "BrandName");
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "SupplierID", "SupplierName");

            return View();
        }

        // POST: Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(Product model, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                db.Products.Add(model);
                db.SaveChanges();

                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    string folderPath = Server.MapPath("~/Content/Uploads/Products/");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                    string extension = Path.GetExtension(ImageFile.FileName);
                    string newFileName = $"{fileName}_{DateTime.Now.Ticks}{extension}";

                    string fullPath = Path.Combine(folderPath, newFileName);
                    ImageFile.SaveAs(fullPath);

                    var img = new ProductImage
                    {
                        ProductID = model.ProductID,
                        ImageURL = "/Content/Uploads/Products/" + newFileName,
                        IsPrimary = true,
                        CreatedDate = DateTime.Now
                    };

                    db.ProductImages.Add(img);
                    db.SaveChanges();
                }

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Products");
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName");
            ViewBag.Brands = new SelectList(db.Brands.Where(b => b.IsActive), "BrandID", "BrandName");
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "SupplierID", "SupplierName");

            return View(model);
        }

        // GET: Admin/EditProduct/5
        public ActionResult EditProduct(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Products");
            }

            var product = db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Products");
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.Brands = new SelectList(db.Brands.Where(b => b.IsActive), "BrandID", "BrandName", product.BrandID);
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "SupplierID", "SupplierName", product.SupplierID);

            return View(product);
        }

        // POST: Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(Product product, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = db.Products.Find(product.ProductID);
                    if (existingProduct == null)
                    {
                        TempData["Error"] = "Sản phẩm không tồn tại.";
                        return RedirectToAction("Products");
                    }

                    // Cập nhật thông tin sản phẩm
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

                    // Upload ảnh mới nếu có
                    if (ImageFile != null && ImageFile.ContentLength > 0)
                    {
                        string folderPath = Server.MapPath("~/Content/Uploads/Products/");
                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        string fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                        string extension = Path.GetExtension(ImageFile.FileName);
                        string newFileName = $"{fileName}_{DateTime.Now.Ticks}{extension}";

                        string fullPath = Path.Combine(folderPath, newFileName);
                        ImageFile.SaveAs(fullPath);

                        // Xóa ảnh cũ (nếu muốn)
                        var oldImages = db.ProductImages.Where(img => img.ProductID == product.ProductID).ToList();
                        foreach (var img in oldImages)
                        {
                            img.IsPrimary = false;
                        }

                        // Thêm ảnh mới
                        var newImage = new ProductImage
                        {
                            ProductID = product.ProductID,
                            ImageURL = "/Content/Uploads/Products/" + newFileName,
                            IsPrimary = true,
                            CreatedDate = DateTime.Now
                        };
                        db.ProductImages.Add(newImage);
                    }

                    db.SaveChanges();

                    TempData["Success"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Products");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName", product.CategoryID);
            ViewBag.Brands = new SelectList(db.Brands.Where(b => b.IsActive), "BrandID", "BrandName", product.BrandID);
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "SupplierID", "SupplierName", product.SupplierID);

            return View(product);
        }

        // POST: Admin/DeleteProduct
        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            try
            {
                var product = db.Products.Find(id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại." });
                }

                // Kiểm tra xem sản phẩm có trong đơn hàng nào không
                if (db.OrderDetails.Any(od => od.ProductID == id))
                {
                    // Không xóa, chỉ ẩn
                    product.IsActive = false;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Sản phẩm đã được ẩn (không xóa vì đã có trong đơn hàng)." });
                }

                // Xóa hình ảnh liên quan
                var images = db.ProductImages.Where(img => img.ProductID == id).ToList();
                foreach (var img in images)
                {
                    db.ProductImages.Remove(img);
                }

                // Xóa sản phẩm
                db.Products.Remove(product);
                db.SaveChanges();

                return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ==========================================
        // QUẢN LÝ ĐƠN HÀNG
        // ==========================================

        // GET: Admin/Orders
        public ActionResult Orders(string search, string status)
        {
            var orders = db.Orders
                .Include(o => o.Status)
                .Include(o => o.User)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                orders = orders.Where(o =>
                    o.OrderCode.ToLower().Contains(search) ||
                    o.CustomerName.ToLower().Contains(search) ||
                    o.CustomerPhone.Contains(search)
                );
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status.StatusName == status);
            }

            // Truyền danh sách trạng thái cho filter
            ViewBag.OrderStatuses = db.OrderStatuses.OrderBy(s => s.DisplayOrder).ToList();

            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        // GET: Admin/OrderDetail/5
        public ActionResult OrderDetail(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Orders");
            }

            var order = db.Orders
                .Include(o => o.Status)
                .Include(o => o.User)
                .Include(o => o.OrderDetails.Select(od => od.Product.ProductImages))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Orders");
            }

            // Truyền danh sách trạng thái cho dropdown
            ViewBag.OrderStatuses = db.OrderStatuses.OrderBy(s => s.DisplayOrder).ToList();

            return View(order);
        }

        // POST: Admin/UpdateOrderStatus
        [HttpPost]
        public JsonResult UpdateOrderStatus(int orderId, int statusId)
        {
            try
            {
                var order = db.Orders.Find(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Đơn hàng không tồn tại." });
                }

                order.StatusID = statusId;

                // Cập nhật ngày giao hàng/hủy nếu cần
                var status = db.OrderStatuses.Find(statusId);
                if (status != null)
                {
                    if (status.StatusName == "Đã giao")
                        order.DeliveredDate = DateTime.Now;
                    else if (status.StatusName == "Đã hủy")
                        order.CancelledDate = DateTime.Now;
                }

                db.SaveChanges();

                return Json(new { success = true, message = "Cập nhật trạng thái thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ==========================================
        // QUẢN LÝ NGƯỜI DÙNG
        // ==========================================

        // GET: Admin/Users
        public ActionResult Users(string search, int? roleId)
        {
            var users = db.Users
                .Include(u => u.Role)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = users.Where(u =>
                    u.Username.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.FullName.ToLower().Contains(search)
                );
            }

            // Lọc theo quyền
            if (roleId.HasValue)
            {
                users = users.Where(u => u.RoleID == roleId.Value);
            }

            // Truyền danh sách quyền cho filter
            ViewBag.Roles = db.Roles.ToList();

            return View(users.OrderByDescending(u => u.CreatedDate).ToList());
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        public JsonResult ToggleUserStatus(int id)
        {
            try
            {
                var user = db.Users.Find(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Người dùng không tồn tại." });
                }

                // Không cho phép khóa chính mình
                int currentUserId = Session["UserID"] != null ? (int)Session["UserID"] : 0;
                if (user.UserID == currentUserId)
                {
                    return Json(new { success = false, message = "Không thể khóa tài khoản của chính mình." });
                }

                user.IsActive = !user.IsActive;
                db.SaveChanges();

                string status = user.IsActive ? "kích hoạt" : "khóa";
                return Json(new { success = true, message = $"Đã {status} tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // ==========================================
        // THỐNG KÊ
        // ==========================================

        // GET: Admin/Statistics
        public ActionResult Statistics(string period = "month")
        {
            var model = new
            {
                Period = period,

                // Doanh thu theo thời gian
                RevenueData = GetRevenueByPeriod(period),

                // Top sản phẩm bán chạy
                TopProducts = db.OrderDetails
                    .GroupBy(od => new { od.ProductID, od.ProductName })
                    .Select(g => new {
                        ProductName = g.Key.ProductName,
                        TotalQuantity = g.Sum(od => od.Quantity),
                        TotalRevenue = g.Sum(od => od.TotalPrice)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(10)
                    .ToList(),

                // Thống kê theo danh mục
                CategoryStats = db.OrderDetails
                    .Include(od => od.Product.Category)
                    .GroupBy(od => od.Product.Category.CategoryName)
                    .Select(g => new {
                        CategoryName = g.Key,
                        TotalOrders = g.Count(),
                        TotalRevenue = g.Sum(od => od.TotalPrice)
                    })
                    .OrderByDescending(x => x.TotalRevenue)
                    .ToList(),

                // Thống kê đơn hàng theo trạng thái
                OrderStatusStats = db.Orders
                    .GroupBy(o => o.Status.StatusName)
                    .Select(g => new {
                        StatusName = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(o => o.FinalAmount)
                    })
                    .ToList(),

                // Thống kê tổng quan
                TotalRevenue = db.Orders
                    .Where(o => o.Status.StatusName == "Đã giao")
                    .Sum(o => (decimal?)o.FinalAmount) ?? 0,

                TotalOrders = db.Orders.Count(),

                CompletedOrders = db.Orders.Count(o => o.Status.StatusName == "Đã giao"),

                PendingOrders = db.Orders.Count(o => o.Status.StatusName == "Chờ xác nhận"),

                CancelledOrders = db.Orders.Count(o => o.Status.StatusName == "Đã hủy"),

                TotalCustomers = db.Users.Count(u => u.RoleID == 4), // Customer role

                TotalProducts = db.Products.Count(p => p.IsActive),

                LowStockProducts = db.Products.Count(p => p.StockQuantity <= p.MinStockLevel)
            };

            return View(model);
        }

        // Helper method - Lấy doanh thu theo khoảng thời gian
        private object GetRevenueByPeriod(string period)
        {
            var now = DateTime.Now;

            switch (period.ToLower())
            {
                case "week":
                    // 7 ngày gần nhất
                    return Enumerable.Range(0, 7)
                        .Select(i => now.AddDays(-i))
                        .OrderBy(d => d)
                        .Select(date => new {
                            Date = date.ToString("dd/MM"),
                            Revenue = db.Orders
                                .Where(o => o.Status.StatusName == "Đã giao"
                                    && DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(date))
                                .Sum(o => (decimal?)o.FinalAmount) ?? 0,
                            Orders = db.Orders
                                .Count(o => DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(date))
                        })
                        .ToList();

                case "year":
                    // 12 tháng trong năm
                    return Enumerable.Range(1, 12)
                        .Select(month => new {
                            Date = $"T{month}",
                            Revenue = db.Orders
                                .Where(o => o.Status.StatusName == "Đã giao"
                                    && o.OrderDate.Year == now.Year
                                    && o.OrderDate.Month == month)
                                .Sum(o => (decimal?)o.FinalAmount) ?? 0,
                            Orders = db.Orders
                                .Count(o => o.OrderDate.Year == now.Year
                                    && o.OrderDate.Month == month)
                        })
                        .ToList();

                default: // month
                    // 30 ngày gần nhất
                    return Enumerable.Range(0, 30)
                        .Select(i => now.AddDays(-i))
                        .OrderBy(d => d)
                        .Select(date => new {
                            Date = date.ToString("dd/MM"),
                            Revenue = db.Orders
                                .Where(o => o.Status.StatusName == "Đã giao"
                                    && DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(date))
                                .Sum(o => (decimal?)o.FinalAmount) ?? 0,
                            Orders = db.Orders
                                .Count(o => DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(date))
                        })
                        .ToList();
            }
        }

        // ==========================================
        // DISPOSE
        // ==========================================
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