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
                PendingOrders = db.Orders.Count(o => o.Status.StatusName == "Chờ xử lý"),
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
                .Include(p => p.ProductImages)  // ← QUAN TRỌNG: Include ProductImages
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

        [HttpPost]
        public ActionResult CreateProduct(Product model, HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
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

                return RedirectToAction("Products");
            }

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
        public ActionResult EditProduct(Product product, string[] imageUrls, bool[] isPrimary, int[] existingImageIds)
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

                    // Xóa hình ảnh cũ không còn trong danh sách
                    var imagesToDelete = db.ProductImages
                        .Where(img => img.ProductID == product.ProductID)
                        .ToList();

                    if (existingImageIds != null)
                    {
                        imagesToDelete = imagesToDelete.Where(img => !existingImageIds.Contains(img.ImageID)).ToList();
                    }

                    foreach (var img in imagesToDelete)
                    {
                        db.ProductImages.Remove(img);
                    }

                    // Thêm hình ảnh mới
                    if (imageUrls != null && imageUrls.Length > 0)
                    {
                        for (int i = 0; i < imageUrls.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(imageUrls[i]))
                            {
                                var productImage = new ProductImage
                                {
                                    ProductID = product.ProductID,
                                    ImageURL = imageUrls[i],
                                    IsPrimary = isPrimary != null && isPrimary.Length > i && isPrimary[i],
                                    DisplayOrder = i,
                                    CreatedDate = DateTime.Now
                                };
                                db.ProductImages.Add(productImage);
                            }
                        }
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