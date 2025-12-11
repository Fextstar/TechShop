using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechShop.Models;

namespace TechShop.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["UserID"] == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new { controller = "Account", action = "Login" })
                );
                return;
            }

            int roleId = (int)Session["RoleID"];
            if (roleId != 1 && roleId != 2)
            {
                TempData["Error"] = "Bạn không có quyền truy cập.";
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(new { controller = "Home", action = "Index" })
                );
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        // =======================
        // CREATE PRODUCT
        // =======================
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
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Lưu sản phẩm vào DB
            db.Products.Add(model);
            db.SaveChanges();

            // Nếu có upload ảnh
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                string folder = Server.MapPath("~/Content/Uploads/Products/");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(ImageFile.FileName)}";
                string filePath = Path.Combine(folder, fileName);
                ImageFile.SaveAs(filePath);

                db.ProductImages.Add(new ProductImage
                {
                    ProductID = model.ProductID,
                    ImageURL = "/Content/Uploads/Products/" + fileName,
                    IsPrimary = true,
                    CreatedDate = DateTime.Now
                });
                db.SaveChanges();
            }

            TempData["Success"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("Products");
        }

        // =======================
        // EDIT PRODUCT
        // =======================
        public ActionResult EditProduct(int id)
        {
            var product = db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductID == id);

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryID", "CategoryName");
            ViewBag.Brands = new SelectList(db.Brands.Where(b => b.IsActive), "BrandID", "BrandName");
            ViewBag.Suppliers = new SelectList(db.Suppliers.Where(s => s.IsActive), "SupplierID", "SupplierName");

            return View(product);
        }

        [HttpPost]
        public ActionResult EditProduct(Product model, HttpPostedFileBase[] NewImages, int[] keepImageIds, int? primaryImage)
        {
            var product = db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefault(p => p.ProductID == model.ProductID);

            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Products");
            }

            // Cập nhật thông tin sản phẩm
            db.Entry(product).CurrentValues.SetValues(model);

            // Xóa ảnh cũ không giữ lại
            var oldImages = product.ProductImages.ToList();

            foreach (var img in oldImages)
            {
                if (keepImageIds == null || !keepImageIds.Contains(img.ImageID))
                {
                    // Xóa file vật lý
                    string filePath = Server.MapPath(img.ImageURL);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    db.ProductImages.Remove(img);
                }
            }

            // Upload ảnh mới
            if (NewImages != null)
            {
                foreach (var file in NewImages)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        string folder = Server.MapPath("~/Content/Uploads/Products/");
                        if (!Directory.Exists(folder))
                            Directory.CreateDirectory(folder);

                        string fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        string filePath = Path.Combine(folder, fileName);
                        file.SaveAs(filePath);

                        db.ProductImages.Add(new ProductImage
                        {
                            ProductID = product.ProductID,
                            ImageURL = "/Content/Uploads/Products/" + fileName,
                            IsPrimary = false,
                            CreatedDate = DateTime.Now
                        });
                    }
                }
            }

            // Đặt ảnh chính
            foreach (var img in product.ProductImages)
            {
                img.IsPrimary = (primaryImage.HasValue && img.ImageID == primaryImage.Value);
            }

            db.SaveChanges();
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("Products");
        }

        // =======================
        // DELETE
        // =======================
        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
                return Json(new { success = false });

            var images = db.ProductImages.Where(i => i.ProductID == id).ToList();
            foreach (var img in images)
            {
                string filePath = Server.MapPath(img.ImageURL);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                db.ProductImages.Remove(img);
            }

            db.Products.Remove(product);
            db.SaveChanges();

            return Json(new { success = true });
        }
    }
}
