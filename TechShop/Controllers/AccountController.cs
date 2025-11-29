using System;
using System.Linq;
using System.Web.Mvc;
using TechShop.Models;
using System.Data.Entity;

namespace TechShop.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Trang đăng nhập
        public ActionResult Login()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password, string returnUrl)
        {
            var user = db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Username == username);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập không tồn tại");
                return View();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Tài khoản đã bị khóa");
                return View();
            }

            // Mã hóa password để so sánh
            string hashedPassword = HashPassword(password);

            if (user.Password != hashedPassword && user.Password != password)
            {
                ModelState.AddModelError("", "Mật khẩu không đúng");
                return View();
            }

            // Đăng nhập thành công
            Session["UserID"] = user.UserID;
            Session["Username"] = user.Username;
            Session["FullName"] = user.FullName;
            Session["RoleID"] = user.RoleID;
            Session["RoleName"] = user.Role.RoleName;

            // Cập nhật LastLogin
            user.LastLogin = DateTime.Now;
            db.SaveChanges();

            // Log activity
            LogActivity(user.UserID, "Đăng nhập", "Users", user.UserID);

            // Chuyển hướng
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (user.Role.RoleName == "Admin" || user.Role.RoleName == "Manager")
            {
                return RedirectToAction("Index", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }

        // Trang đăng ký
        public ActionResult Register()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // Xử lý đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(User user, string confirmPassword)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra username đã tồn tại
                if (db.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(user);
                }

                // Kiểm tra email đã tồn tại
                if (db.Users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(user);
                }

                // Kiểm tra mật khẩu xác nhận
                if (user.Password != confirmPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu xác nhận không khớp");
                    return View(user);
                }

                // Mã hóa mật khẩu
                user.Password = HashPassword(user.Password);

                // Gán role Customer (RoleID = 4)
                var customerRole = db.Roles.FirstOrDefault(r => r.RoleName == "Customer");
                user.RoleID = customerRole?.RoleID ?? 4;

                user.IsActive = true;
                user.CreatedDate = DateTime.Now;

                db.Users.Add(user);
                db.SaveChanges();

                // Log activity
                LogActivity(user.UserID, "Đăng ký tài khoản", "Users", user.UserID);

                TempData["Message"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // Đăng xuất
        public ActionResult Logout()
        {
            int? userId = Session["UserID"] as int?;
            if (userId.HasValue)
            {
                LogActivity(userId.Value, "Đăng xuất", "Users", userId.Value);
            }

            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }

        // Thông tin tài khoản
        public new ActionResult Profile()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = (int)Session["UserID"];
            var user = db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserID == userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // Cập nhật thông tin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public new ActionResult Profile(User user)
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var existingUser = db.Users.Find(user.UserID);
                if (existingUser == null)
                {
                    return HttpNotFound();
                }

                // Kiểm tra email trùng
                if (db.Users.Any(u => u.Email == user.Email && u.UserID != user.UserID))
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng");
                    return View(user);
                }

                // Cập nhật thông tin (không cho phép đổi username và password ở đây)
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.Address = user.Address;

                db.SaveChanges();

                // Log activity
                LogActivity(user.UserID, "Cập nhật thông tin", "Users", user.UserID);

                Session["FullName"] = user.FullName;
                TempData["Message"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("Profile");
            }

            return View(user);
        }

        // Đổi mật khẩu
        public ActionResult ChangePassword()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }

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
                return HttpNotFound();
            }

            // Kiểm tra mật khẩu cũ
            string hashedOldPassword = HashPassword(oldPassword);
            if (user.Password != hashedOldPassword && user.Password != oldPassword)
            {
                ModelState.AddModelError("", "Mật khẩu cũ không đúng");
                return View();
            }

            // Kiểm tra mật khẩu mới
            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu mới không khớp");
                return View();
            }

            if (newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự");
                return View();
            }

            // Cập nhật mật khẩu
            user.Password = HashPassword(newPassword);
            db.SaveChanges();

            // Log activity
            LogActivity(userId, "Đổi mật khẩu", "Users", userId);

            TempData["Message"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile");
        }

        // Hàm mã hóa mật khẩu (SHA256)
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Log hoạt động
        private void LogActivity(int userId, string action, string tableName, int? recordId)
        {
            try
            {
                var log = new ActivityLog
                {
                    UserID = userId,
                    Action = action,
                    TableName = tableName,
                    RecordID = recordId,
                    IPAddress = Request.UserHostAddress,
                    CreatedDate = DateTime.Now
                };

                db.ActivityLogs.Add(log);
                db.SaveChanges();
            }
            catch
            {
                // Không ném exception nếu log thất bại
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