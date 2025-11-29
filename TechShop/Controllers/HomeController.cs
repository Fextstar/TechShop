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
            try
            {
                // Đảm bảo validation đã tắt
                db.Configuration.ValidateOnSaveEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ProxyCreationEnabled = false;

                // Lấy sản phẩm nổi bật - KHÔNG dùng decimal trong query
                var featuredProducts = db.Products
                    .Where(p => p.IsActive)
                    .Where(p => p.IsFeatured == true || p.StockQuantity > 0)
                    .OrderByDescending(p => p.IsFeatured)
                    .ThenByDescending(p => p.CreatedDate)
                    .Take(8)
                    .ToList(); // ToList() trước để load data

                // SAU ĐÓ mới Include (tránh EF validate schema)
                foreach (var product in featuredProducts)
                {
                    // Load relationships manually nếu cần
                    db.Entry(product).Reference(p => p.Category).Load();
                    db.Entry(product).Reference(p => p.Brand).Load();
                    db.Entry(product).Collection(p => p.ProductImages).Load();
                }

                // Lấy danh mục chính
                ViewBag.Categories = db.Categories
                    .Where(c => c.IsActive && c.ParentCategoryID == null)
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                return View(featuredProducts);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, trả về view rỗng
                ViewBag.Error = "Đang có lỗi khi tải dữ liệu: " + ex.Message;
                ViewBag.Categories = new List<Category>();
                return View(new List<Product>());
            }
        }

        // Danh sách sản phẩm
        public ActionResult Products(int? categoryId, string search, string sortBy)
        {
            try
            {
                db.Configuration.ValidateOnSaveEnabled = false;

                var products = db.Products
                    .Where(p => p.IsActive)
                    .ToList(); // Load data trước

                // Load relationships
                foreach (var p in products)
                {
                    db.Entry(p).Reference(x => x.Category).Load();
                    db.Entry(p).Reference(x => x.Brand).Load();
                    db.Entry(p).Collection(x => x.ProductImages).Load();
                }

                // Lọc theo danh mục
                if (categoryId.HasValue)
                {
                    var category = db.Categories.Find(categoryId.Value);
                    if (category != null)
                    {
                        var categoryIds = db.Categories
                            .Where(c => c.CategoryID == categoryId.Value || c.ParentCategoryID == categoryId.Value)
                            .Select(c => c.CategoryID)
                            .ToList();

                        products = products.Where(p => categoryIds.Contains(p.CategoryID)).ToList();
                        ViewBag.CurrentCategory = category.CategoryName;
                    }
                }

                // Tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    products = products.Where(p =>
                        p.ProductName.ToLower().Contains(search) ||
                        (p.Description != null && p.Description.ToLower().Contains(search)) ||
                        p.Brand.BrandName.ToLower().Contains(search) ||
                        p.Category.CategoryName.ToLower().Contains(search)
                    ).ToList();
                    ViewBag.SearchTerm = search;
                }

                // Sắp xếp
                switch (sortBy)
                {
                    case "price_asc":
                        products = products.OrderBy(p => p.Price).ToList();
                        break;
                    case "price_desc":
                        products = products.OrderByDescending(p => p.Price).ToList();
                        break;
                    case "name":
                        products = products.OrderBy(p => p.ProductName).ToList();
                        break;
                    default:
                        products = products.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedDate).ToList();
                        break;
                }

                ViewBag.Categories = db.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                ViewBag.CurrentSort = sortBy;

                return View(products);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi: " + ex.Message;
                ViewBag.Categories = new List<Category>();
                return View(new List<Product>());
            }
        }

        // Chi tiết sản phẩm
        public ActionResult ProductDetail(int id)
        {
            try
            {
                db.Configuration.ValidateOnSaveEnabled = false;

                var product = db.Products.Find(id);
                if (product == null)
                {
                    return HttpNotFound();
                }

                // Load relationships
                db.Entry(product).Reference(p => p.Category).Load();
                db.Entry(product).Reference(p => p.Brand).Load();
                db.Entry(product).Reference(p => p.Supplier).Load();
                db.Entry(product).Collection(p => p.ProductImages).Load();
                db.Entry(product).Collection(p => p.ProductReviews).Load();

                // Load User cho mỗi review
                foreach (var review in product.ProductReviews)
                {
                    db.Entry(review).Reference(r => r.User).Load();
                }

                // Tăng lượt xem
                product.ViewCount++;
                db.SaveChanges();

                // Sản phẩm liên quan
                var relatedProducts = db.Products
                    .Where(p => p.CategoryID == product.CategoryID &&
                               p.ProductID != id &&
                               p.IsActive &&
                               p.StockQuantity > 0)
                    .OrderByDescending(p => p.IsFeatured)
                    .Take(4)
                    .ToList();

                foreach (var p in relatedProducts)
                {
                    db.Entry(p).Collection(x => x.ProductImages).Load();
                }

                ViewBag.RelatedProducts = relatedProducts;

                // Đánh giá đã duyệt
                ViewBag.ApprovedReviews = product.ProductReviews
                    .Where(r => r.IsApproved)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToList();

                return View(product);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Có lỗi: " + ex.Message;
                return HttpNotFound();
            }
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

            try
            {
                db.Configuration.ValidateOnSaveEnabled = false;

                var products = db.Products
                    .Where(p => p.IsActive && p.ProductName.Contains(term))
                    .Take(10)
                    .ToList();

                var results = products.Select(p => new
                {
                    id = p.ProductID,
                    name = p.ProductName,
                    price = p.Price,
                    discountPrice = p.DiscountPrice,
                    image = p.ProductImages.FirstOrDefault(i => i.IsPrimary) != null
                        ? p.ProductImages.FirstOrDefault(i => i.IsPrimary).ImageURL
                        : "/Content/Images/no-image.jpg"
                }).ToList();

                return Json(new { results = results }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { results = new List<object>() }, JsonRequestBehavior.AllowGet);
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