using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TechShop.Models;

namespace TechShop.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Nếu đã đăng nhập, chuyển về trang chủ
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return View();
            }

            // Tìm user với username và IsActive = true
            var user = db.Users
                .Include(u => u.Role) // Include Role để lấy RoleName
                .FirstOrDefault(u => u.Username == username && u.IsActive == true);

            if (user == null)
            {
                ViewBag.Error = "Tài khoản không tồn tại hoặc đã bị khóa.";
                return View();
            }

            // Kiểm tra mật khẩu
            // TODO: Trong production, cần hash password và so sánh hash
            if (user.Password != password)
            {
                ViewBag.Error = "Mật khẩu không đúng.";
                return View();
            }

            // ==================================================
            // LƯU THÔNG TIN VÀO SESSION
            // ==================================================
            Session["UserID"] = user.UserID;
            Session["Username"] = user.Username;
            Session["FullName"] = user.FullName;
            Session["Email"] = user.Email;
            Session["RoleID"] = user.RoleID;
            Session["RoleName"] = user.Role?.RoleName ?? "Customer";

            // Cập nhật LastLogin
            user.LastLogin = DateTime.Now;
            db.SaveChanges();

            // Ghi log hoạt động
            try
            {
                var activityLog = new ActivityLog
                {
                    UserID = user.UserID,
                    Action = "Đăng nhập",
                    TableName = "Users",
                    RecordID = user.UserID,
                    IPAddress = Request.UserHostAddress,
                    CreatedDate = DateTime.Now
                };
                db.ActivityLogs.Add(activityLog);
                db.SaveChanges();
            }
            catch { } // Bỏ qua lỗi log

            // ==================================================
            // CHUYỂN HƯỚNG THEO ROLE
            // ==================================================
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Admin hoặc Manager -> Trang quản trị
            if (user.RoleID == 1 || user.RoleID == 2)
            {
                TempData["Success"] = $"Chào mừng {user.FullName}! Đăng nhập thành công.";
                return RedirectToAction("Index", "Admin");
            }

            // Customer -> Trang chủ
            TempData["Success"] = $"Chào mừng {user.FullName}! Đăng nhập thành công.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            // Ghi log trước khi xóa session
            if (Session["UserID"] != null)
            {
                try
                {
                    var userId = (int)Session["UserID"];
                    var activityLog = new ActivityLog
                    {
                        UserID = userId,
                        Action = "Đăng xuất",
                        TableName = "Users",
                        RecordID = userId,
                        IPAddress = Request.UserHostAddress,
                        CreatedDate = DateTime.Now
                    };
                    db.ActivityLogs.Add(activityLog);
                    db.SaveChanges();
                }
                catch { } // Bỏ qua lỗi log
            }

            // Xóa tất cả Session
            Session.Clear();
            Session.Abandon();

            // Xóa Authentication Cookie (nếu có)
            FormsAuthentication.SignOut();

            TempData["Info"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            // Nếu đã đăng nhập, chuyển về trang chủ
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra username đã tồn tại
                if (db.Users.Any(u => u.Username == user.Username))
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.";
                    return View(user);
                }

                // Kiểm tra email đã tồn tại
                if (db.Users.Any(u => u.Email == user.Email))
                {
                    ViewBag.Error = "Email đã được sử dụng. Vui lòng sử dụng email khác.";
                    return View(user);
                }

                // Kiểm tra mật khẩu xác nhận
                if (user.Password != confirmPassword)
                {
                    ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                    return View(user);
                }

                // TODO: Hash password trong production
                // user.Password = PasswordHelper.HashPassword(user.Password);

                // Thiết lập thông tin mặc định
                user.RoleID = 4; // Customer (theo database của bạn)
                user.IsActive = true;
                user.CreatedDate = DateTime.Now;

                try
                {
                    db.Users.Add(user);
                    db.SaveChanges();

                    TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại.";
                    // Log error: ex.Message
                    return View(user);
                }
            }

            return View(user);
        }

        // GET: Account/Profile
        [HttpGet]
        public ActionResult Profile()
        {
            if (Session["UserID"] == null)
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem hồ sơ.";
                return RedirectToAction("Login", new { returnUrl = Url.Action("Profile") });
            }

            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        // POST: Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(User model)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            // Cập nhật thông tin (không cho phép thay đổi Username, Email, RoleID)
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            try
            {
                db.SaveChanges();

                // Cập nhật lại Session
                Session["FullName"] = user.FullName;

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật hồ sơ.";
            }

            return View(user);
        }

        // GET: Account/ChangePassword
        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = (int)Session["UserID"];
            var user = db.Users.Find(userId);

            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra mật khẩu cũ
            // TODO: Trong production, so sánh hash
            if (user.Password != oldPassword)
            {
                ViewBag.Error = "Mật khẩu hiện tại không đúng.";
                return View();
            }

            // Kiểm tra mật khẩu mới
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ViewBag.Error = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return View();
            }

            // Kiểm tra xác nhận mật khẩu
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                return View();
            }

            try
            {
                // TODO: Hash password trong production
                user.Password = newPassword;
                db.SaveChanges();

                TempData["Success"] = "Đổi mật khẩu thành công!";
                return RedirectToAction("Profile");
            }
            catch (Exception)
            {
                ViewBag.Error = "Có lỗi xảy ra khi đổi mật khẩu.";
                return View();
            }
        }

        // GET: Account/Orders - Danh sách đơn hàng của user
        [HttpGet]
        public ActionResult Orders()
        {
            if (Session["UserID"] == null)
            {
                TempData["Warning"] = "Vui lòng đăng nhập để xem đơn hàng.";
                return RedirectToAction("Login", new { returnUrl = Url.Action("Orders") });
            }

            int userId = (int)Session["UserID"];

            var orders = db.Orders
                .Include(o => o.Status)  // Sửa: OrderStatus → Status
                .Include(o => o.OrderDetails)
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        // GET: Account/OrderDetail/5
        [HttpGet]
        public ActionResult OrderDetail(int? id)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (id == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Orders");
            }

            int userId = (int)Session["UserID"];

            var order = db.Orders
                .Include(o => o.Status)  // Sửa: OrderStatus → Status
                .Include(o => o.OrderDetails.Select(od => od.Product))
                .FirstOrDefault(o => o.OrderID == id && o.UserID == userId);

            if (order == null)
            {
                TempData["Error"] = "Đơn hàng không tồn tại hoặc không thuộc về bạn.";
                return RedirectToAction("Orders");
            }

            return View(order);
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

// ============================================
// BASE CONTROLLER - Để kiểm tra authentication
// Controllers/BaseController.cs
// ============================================
/*
using System.Web.Mvc;

namespace TechShop.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Kiểm tra session còn hiệu lực không
            if (Session["UserID"] == null)
            {
                var actionName = filterContext.ActionDescriptor.ActionName;
                var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                
                // Cho phép truy cập các action không cần login
                string[] allowedActions = { "Login", "Register", "Index" };
                
                if (!allowedActions.Contains(actionName))
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new System.Web.Routing.RouteValueDictionary(
                            new { controller = "Account", action = "Login", returnUrl = filterContext.HttpContext.Request.RawUrl }
                        )
                    );
                }
            }
            
            base.OnActionExecuting(filterContext);
        }
    }
}
*/