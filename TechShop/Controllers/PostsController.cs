using System;
using System.Linq;
using System.Web.Mvc;
using TechShop.Models;

namespace TechShop.Controllers
{
    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Posts - Hiển thị danh sách bài viết
        public ActionResult Index(int? page)
        {
            int pageSize = 9;
            int pageNumber = page ?? 1;

            var posts = db.Posts
                .Where(p => p.IsPublished == true)
                .OrderByDescending(p => p.PublishedDate ?? DateTime.MinValue)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalPosts = db.Posts.Count(p => p.IsPublished == true);
            int totalPages = (int)Math.Ceiling((double)totalPosts / pageSize);

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;

            return View(posts);
        }

        // GET: Posts/Detail/5 - Chi tiết bài viết
        public ActionResult Detail(int id)
        {
            var post = db.Posts.Find(id);

            if (!IsValidPost(post))
            {
                return HttpNotFound();
            }

            // Tăng lượt xem
            post.ViewCount += 1;
            db.SaveChanges();

            return View(post);
        }

        private bool IsValidPost(Post post)
        {
            return post != null && post.IsPublished == true;
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
