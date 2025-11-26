using TechShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace TechShop.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Trang chủ
        public ActionResult Index()
        {
            // Lấy sản phẩm nổi bật (IsFeatured = true) hoặc sản phẩm mới nhất
            var featuredProducts = db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && (p.IsFeatured || p.StockQuantity > 0))
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedDate)
                .Take(8)
                .ToList();

            // Lấy danh mục chính (không có danh mục cha)
            ViewBag.Categories = db.Categories
                .Where(c => c.IsActive && !c.ParentCategoryID.HasValue)
                .OrderBy(c => c.CategoryName)
                .ToList();

            return View(featuredProducts);
        }

        // Danh sách sản phẩm
        public ActionResult Products(int? categoryId, string search, string sortBy)
        {
            var products = db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Lọc theo danh mục (bao gồm cả danh mục con)
            if (categoryId.HasValue)
            {
                var category = db.Categories.Find(categoryId.Value);
                if (category != null)
                {
                    // Lấy ID của danh mục và tất cả danh mục con
                    var categoryIds = db.Categories
                        .Where(c => c.CategoryID == categoryId.Value || c.ParentCategoryID == categoryId.Value)
                        .Select(c => c.CategoryID)
                        .ToList();

                    products = products.Where(p => categoryIds.Contains(p.CategoryID));
                    ViewBag.CurrentCategory = category.CategoryName;
                }
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(search) ||
                    p.Description.Contains(search) ||
                    p.Brand.BrandName.Contains(search) ||
                    p.Category.CategoryName.Contains(search)
                );
                ViewBag.SearchTerm = search;
            }

            // Sắp xếp
            switch (sortBy)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "name":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.CreatedDate);
                    break;
                default:
                    products = products.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedDate);
                    break;
            }

            // Lấy tất cả danh mục cho filter
            ViewBag.Categories = db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.CategoryName)
                .ToList();

            ViewBag.CurrentSort = sortBy;

            return View(products.ToList());
        }

        // Chi tiết sản phẩm
        public ActionResult ProductDetail(int id)
        {
            var product = db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Supplier)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductReviews.Select(r => r.User))
                .FirstOrDefault(p => p.ProductID == id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Tăng lượt xem
            product.ViewCount++;
            db.SaveChanges();

            // Sản phẩm liên quan (cùng danh mục)
            var relatedProducts = db.Products
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryID == product.CategoryID &&
                           p.ProductID != id &&
                           p.IsActive &&
                           p.StockQuantity > 0)
                .OrderByDescending(p => p.IsFeatured)
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            // Đánh giá đã duyệt
            ViewBag.ApprovedReviews = product.ProductReviews
                .Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            return View(product);
        }

        // Giới thiệu
        public ActionResult About()
        {
            return View();
        }

        // Liên hệ
        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.IsRead = false;
                contact.IsReplied = false;
                contact.CreatedDate = DateTime.Now;

                db.Contacts.Add(contact);
                db.SaveChanges();

                TempData["Message"] = "Cảm ơn bạn đã liên hệ. Chúng tôi sẽ phản hồi sớm nhất!";
                return RedirectToAction("Contact");
            }

            return View(contact);
        }

        // Tìm kiếm nhanh (Ajax)
        [HttpGet]
        public JsonResult QuickSearch(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Json(new { results = new List<object>() }, JsonRequestBehavior.AllowGet);
            }

            var products = db.Products
                .Include(p => p.ProductImages)
                .Where(p => p.IsActive && p.ProductName.Contains(term))
                .Take(10)
                .Select(p => new
                {
                    id = p.ProductID,
                    name = p.ProductName,
                    price = p.Price,
                    discountPrice = p.DiscountPrice,
                    image = p.ProductImages.FirstOrDefault(i => i.IsPrimary).ImageURL ?? "/Content/Images/no-image.jpg"
                })
                .ToList();

            return Json(new { results = products }, JsonRequestBehavior.AllowGet);
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