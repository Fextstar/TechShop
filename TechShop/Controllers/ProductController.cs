using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechShop.Models;
using PagedList;

namespace TechShop.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Product
        public ActionResult Index(int? categoryId, int? brandId, string search,
            decimal? minPrice, decimal? maxPrice, string sortBy, int? page)
        {
            var products = db.Products.Where(p => p.IsActive);

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryID == categoryId.Value);
                ViewBag.CurrentCategory = db.Categories.Find(categoryId.Value);
            }

            // Lọc theo thương hiệu
            if (brandId.HasValue)
            {
                products = products.Where(p => p.BrandID == brandId.Value);
                ViewBag.CurrentBrand = db.Brands.Find(brandId.Value);
            }

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(search) ||
                    p.Description.Contains(search));
                ViewBag.Search = search;
            }

            // Lọc theo giá
            if (minPrice.HasValue)
            {
                products = products.Where(p => p.Price >= minPrice.Value);
                ViewBag.MinPrice = minPrice;
            }
            if (maxPrice.HasValue)
            {
                products = products.Where(p => p.Price <= maxPrice.Value);
                ViewBag.MaxPrice = maxPrice;
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
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.CreatedDate);
                    break;
                default:
                    products = products.OrderByDescending(p => p.ViewCount);
                    break;
            }

            ViewBag.SortBy = sortBy;

            // Phân trang
            int pageSize = 12;
            int pageNumber = (page ?? 1);

            // Lấy danh sách danh mục và thương hiệu cho filter
            ViewBag.Categories = db.Categories
                .Where(c => c.IsActive && c.ParentCategoryID == null)
                .ToList();
            ViewBag.Brands = db.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.BrandName)
                .ToList();

            return View(products.ToPagedList(pageNumber, pageSize));
        }

        // GET: Product/Detail/5
        public ActionResult Detail(int id)
        {
            var product = db.Products.Find(id);
            if (product == null || !product.IsActive)
            {
                return HttpNotFound();
            }

            // Tăng lượt xem
            product.ViewCount++;
            db.SaveChanges();

            // Lấy hình ảnh sản phẩm
            ViewBag.Images = db.ProductImages
                .Where(i => i.ProductID == id)
                .OrderBy(i => i.DisplayOrder)
                .ToList();

            // Lấy đánh giá
            ViewBag.Reviews = db.ProductReviews
                .Where(r => r.ProductID == id && r.IsApproved)
                .OrderByDescending(r => r.CreatedDate)
                .ToList();

            // Tính điểm đánh giá trung bình
            var reviews = db.ProductReviews
                .Where(r => r.ProductID == id && r.IsApproved);
            if (reviews.Any())
            {
                ViewBag.AverageRating = reviews.Average(r => r.Rating);
                ViewBag.TotalReviews = reviews.Count();
            }

            // Sản phẩm liên quan
            ViewBag.RelatedProducts = db.Products
                .Where(p => p.IsActive &&
                       p.CategoryID == product.CategoryID &&
                       p.ProductID != id)
                .OrderByDescending(p => p.ViewCount)
                .Take(4)
                .ToList();

            return View(product);
        }

        // POST: Product/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddReview(ProductReview review)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra user đã đăng nhập
                if (Session["UserID"] == null)
                {
                    TempData["Error"] = "Vui lòng đăng nhập để đánh giá sản phẩm.";
                    return RedirectToAction("Login", "Account");
                }

                review.UserID = (int)Session["UserID"];
                review.CreatedDate = DateTime.Now;
                review.IsApproved = false; // Cần admin duyệt

                db.ProductReviews.Add(review);
                db.SaveChanges();

                TempData["Success"] = "Cảm ơn bạn đã đánh giá! Đánh giá của bạn đang chờ phê duyệt.";
                return RedirectToAction("Detail", new { id = review.ProductID });
            }

            return RedirectToAction("Detail", new { id = review.ProductID });
        }

        // GET: Product/Search (AJAX)
        public JsonResult Search(string term)
        {
            var products = db.Products
                .Where(p => p.IsActive && p.ProductName.Contains(term))
                .Take(10)
                .Select(p => new
                {
                    id = p.ProductID,
                    label = p.ProductName,
                    price = p.Price,
                    image = p.PrimaryImageURL
                })
                .ToList();

            return Json(products, JsonRequestBehavior.AllowGet);
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