using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            var sevenDaysAgo = DateTime.Now.AddDays(-7);
            var today = DateTime.Now;

            var model = new DashboardViewModel
            {
                // Tổng sản phẩm (tất cả)
                TotalProducts = db.Products.Count(),

                // Đơn hàng 7 ngày qua
                TotalOrders = db.Orders
                    .Count(o => o.OrderDate >= sevenDaysAgo && o.OrderDate <= today),

                // Khách hàng truy cập 7 ngày qua (users đã đặt hàng)
                TotalCustomers = db.Orders
                    .Where(o => o.OrderDate >= sevenDaysAgo && o.OrderDate <= today)
                    .Select(o => o.UserID)
                    .Distinct()
                    .Count(),

                // Doanh thu tháng này
                MonthlyRevenue = db.Orders
                    .Where(o => o.Status.StatusName == "Đã giao"
                             && o.OrderDate.Month == DateTime.Now.Month
                             && o.OrderDate.Year == DateTime.Now.Year)
                    .Sum(o => (decimal?)o.FinalAmount) ?? 0,

                // Đơn chờ xử lý
                PendingOrders = db.Orders
                    .Count(o => o.Status.StatusName == "Chờ xác nhận"),

                // Sản phẩm sắp hết hàng
                LowStockProducts = db.Products
                    .Count(p => p.StockQuantity < p.MinStockLevel),

                // Đơn hàng gần đây
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
        public ActionResult CreateProduct(Product model, HttpPostedFileBase ImageFile, string[] SpecKeys, string[] SpecValues)
        {
            if (ModelState.IsValid)
            {
                // ✅ XỬ LÝ THÔNG SỐ KỸ THUẬT (Bảng 2 cột)
                if (SpecKeys != null && SpecValues != null)
                {
                    var specs = new Dictionary<string, string>();

                    for (int i = 0; i < SpecKeys.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(SpecKeys[i]) &&
                            !string.IsNullOrWhiteSpace(SpecValues[i]))
                        {
                            specs[SpecKeys[i].Trim()] = SpecValues[i].Trim();
                        }
                    }

                    // Chuyển thành JSON và lưu vào Specifications
                    if (specs.Count > 0)
                    {
                        model.Specifications = JsonConvert.SerializeObject(specs);
                    }
                }

                model.CreatedDate = DateTime.Now;
                db.Products.Add(model);
                db.SaveChanges();

                // ✅ UPLOAD ẢNH
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
            var model = new AdminStatisticsViewModel
            {
                Period = period,

                RevenueData = GetRevenueByPeriod(period),

                TopProducts = db.OrderDetails
                    .GroupBy(od => od.Product.ProductName)
                    .Select(g => new TopProductViewModel
                    {
                        ProductName = g.Key,
                        TotalQuantity = g.Sum(x => x.Quantity),
                        TotalRevenue = g.Sum(x => x.TotalPrice)
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(10)
                    .ToList(),

                CategoryStats = db.OrderDetails
                    .Include(od => od.Product.Category)
                    .GroupBy(od => od.Product.Category.CategoryName)
                    .Select(g => new CategoryStatViewModel
                    {
                        CategoryName = g.Key,
                        TotalOrders = g.Count(),
                        TotalRevenue = g.Sum(x => x.TotalPrice)
                    })
                    .ToList(),

                OrderStatusStats = db.Orders
                    .GroupBy(o => o.Status.StatusName)
                    .Select(g => new OrderStatusStatViewModel
                    {
                        StatusName = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(o => o.FinalAmount)
                    })
                    .ToList(),

                TotalRevenue = db.Orders
                    .Where(o => o.Status.StatusName == "Đã giao")
                    .Sum(o => (decimal?)o.FinalAmount) ?? 0,

                TotalOrders = db.Orders.Count(),
                CompletedOrders = db.Orders.Count(o => o.Status.StatusName == "Đã giao"),
                PendingOrders = db.Orders.Count(o => o.Status.StatusName == "Chờ xác nhận"),
                CancelledOrders = db.Orders.Count(o => o.Status.StatusName == "Đã hủy"),
                TotalCustomers = db.Users.Count(u => u.RoleID == 4),
                TotalProducts = db.Products.Count(p => p.IsActive),
                LowStockProducts = db.Products.Count(p => p.StockQuantity <= p.MinStockLevel)
            };

            return View(model);
        }



        // Helper method - Lấy doanh thu theo khoảng thời gian
        private List<RevenuePointViewModel> GetRevenueByPeriod(string period)
        {
            var now = DateTime.Now;

            if (period == "year")
            {
                return Enumerable.Range(1, 12)
                    .Select(m => new RevenuePointViewModel
                    {
                        Date = $"T{m}",
                        Revenue = db.Orders
                            .Where(o => o.Status.StatusName == "Đã giao"
                                && o.OrderDate.Year == now.Year
                                && o.OrderDate.Month == m)
                            .Sum(o => (decimal?)o.FinalAmount) ?? 0,
                        Orders = db.Orders
                            .Count(o => o.OrderDate.Year == now.Year && o.OrderDate.Month == m)
                    }).ToList();
            }

            int days = period == "week" ? 7 : 30;

            return Enumerable.Range(0, days)
                .Select(i => now.AddDays(-i))
                .OrderBy(d => d)
                .Select(date => new RevenuePointViewModel
                {
                    Date = date.ToString("dd/MM"),
                    Revenue = db.Orders
                        .Where(o => o.Status.StatusName == "Đã giao"
                            && DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(date))
                        .Sum(o => (decimal?)o.FinalAmount) ?? 0,
                    Orders = db.Orders
                        .Count(o => DbFunctions.TruncateTime(o.OrderDate) == DbFunctions.TruncateTime(date))
                }).ToList();
        }



        // GET: Admin/Brands
        public ActionResult Brands()
        {
            var brands = db.Brands
                .OrderByDescending(b => b.CreatedDate)
                .ToList();

            return View(brands);
        }

        // POST: Admin/CreateBrand
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateBrand(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                return RedirectToAction("Brands");
            }

            brand.CreatedDate = DateTime.Now;
            brand.IsActive = true;

            db.Brands.Add(brand);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Thêm thương hiệu thành công";
            return RedirectToAction("Brands");
        }

        // GET: Admin/EditBrand/5
        public ActionResult EditBrand(int id)
        {
            var brand = db.Brands.Find(id);
            if (brand == null)
            {
                return HttpNotFound();
            }

            return View(brand);
        }

        // POST: Admin/EditBrand
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditBrand(Brand brand)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                return View(brand);
            }

            var existingBrand = db.Brands.Find(brand.BrandID);
            if (existingBrand == null)
            {
                return HttpNotFound();
            }

            existingBrand.BrandName = brand.BrandName;
            existingBrand.Description = brand.Description;
            existingBrand.LogoURL = brand.LogoURL;
            existingBrand.Website = brand.Website;
            existingBrand.IsActive = brand.IsActive;

            db.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật thương hiệu thành công";
            return RedirectToAction("Brands");
        }

        // POST: Admin/DeleteBrand/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteBrand(int id)
        {
            var brand = db.Brands.Find(id);
            if (brand == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thương hiệu";
                return RedirectToAction("Brands");
            }

            // ⚠️ Kiểm tra có sản phẩm hay không
            bool hasProducts = db.Products.Any(p => p.BrandID == id);
            if (hasProducts)
            {
                TempData["ErrorMessage"] = "Không thể xóa thương hiệu vì đang có sản phẩm sử dụng";
                return RedirectToAction("Brands");
            }

            db.Brands.Remove(brand);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Xóa thương hiệu thành công";
            return RedirectToAction("Brands");
        }


        // POST: Admin/ToggleBrandStatus/5
        [HttpPost]
        public ActionResult ToggleBrandStatus(int id)
        {
            var brand = db.Brands.Find(id);
            if (brand == null)
                return HttpNotFound();

            brand.IsActive = !brand.IsActive;
            db.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật trạng thái thương hiệu thành công";
            return RedirectToAction("Brands");
        }


        // =======================
        // READ – Danh sách danh mục
        // =======================
        public ActionResult Categories()
        {
            var categories = db.Categories
                               .OrderByDescending(c => c.CreatedDate)
                               .ToList();

            return View(categories);
        }

        // =======================
        // CREATE – GET
        // =======================
        public ActionResult CreateCategory()
        {
            ViewBag.ParentCategories = db.Categories
                .Where(c => c.ParentCategoryID == null)
                .ToList();

            return View();
        }

        // =======================
        // CREATE – POST
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                return RedirectToAction("CreateCategory");
            }

            category.CreatedDate = DateTime.Now;
            category.IsActive = true;

            db.Categories.Add(category);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Thêm danh mục thành công";
            return RedirectToAction("Categories");
        }

        // =======================
        // UPDATE – GET
        // =======================
        public ActionResult EditCategory(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null)
                return HttpNotFound();

            ViewBag.ParentCategories = db.Categories
                .Where(c => c.CategoryID != id)
                .ToList();

            return View(category);
        }

        // =======================
        // UPDATE – POST
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(Category model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                return RedirectToAction("EditCategory", new { id = model.CategoryID });
            }

            var category = db.Categories.Find(model.CategoryID);
            if (category == null)
                return HttpNotFound();

            category.CategoryName = model.CategoryName;
            category.Description = model.Description;
            category.ImageURL = model.ImageURL;
            category.ParentCategoryID = model.ParentCategoryID;
            category.IsActive = model.IsActive;

            db.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật danh mục thành công";
            return RedirectToAction("Categories");
        }

        // =======================
        // DELETE – AJAX
        // =======================
        [HttpPost]
        public JsonResult DeleteCategory(int id)
        {
            try
            {
                var category = db.Categories.Find(id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Danh mục không tồn tại" });
                }

                // ❗ Nếu có danh mục con
                bool hasChild = db.Categories.Any(c => c.ParentCategoryID == id);
                if (hasChild)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa danh mục đang có danh mục con"
                    });
                }

                // ❗ Nếu có sản phẩm
                bool hasProduct = db.Products.Any(p => p.CategoryID == id);
                if (hasProduct)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể xóa danh mục đang chứa sản phẩm"
                    });
                }

                db.Categories.Remove(category);
                db.SaveChanges();

                return Json(new { success = true, message = "Xóa danh mục thành công" });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
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