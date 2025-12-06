using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TechShop.Models;

namespace TechShop.Controllers
{
    /// <summary>
    /// Controller xử lý đăng nhập, đăng ký, đăng xuất
    /// File này đặt trong: Controllers/AccountController.cs
    /// </summary>
    public class AccountController : Controller
    {
        // Lấy connection string từ Web.config
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["TechShopConnection"].ConnectionString;

        // ==================== ĐĂNG NHẬP ====================

        // GET: Account/Login
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // Nếu đã đăng nhập, chuyển hướng về trang chủ
            if (Session["UserID"] != null)
            {
                return RedirectToHome();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Xác thực người dùng
                User user = AuthenticateUser(model.Username, model.Password);

                if (user != null)
                {
                    // Đăng nhập thành công
                    SetUserSession(user);
                    UpdateLastLogin(user.UserID);
                    LogActivity(user.UserID, "Đăng nhập hệ thống");

                    // Xử lý Remember Me
                    if (model.RememberMe)
                    {
                        FormsAuthentication.SetAuthCookie(model.Username, true);
                        
                        // Tạo cookie tùy chỉnh
                        HttpCookie rememberCookie = new HttpCookie("RememberMe");
                        rememberCookie["Username"] = model.Username;
                        rememberCookie.Expires = DateTime.Now.AddDays(30);
                        Response.Cookies.Add(rememberCookie);
                    }
                    else
                    {
                        FormsAuthentication.SetAuthCookie(model.Username, false);
                    }

                    TempData["SuccessMessage"] = $"Chào mừng {user.FullName}!";

                    // Chuyển hướng
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToHome();
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng!");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                LogError(ex);
                return View(model);
            }
        }

        // ==================== ĐĂNG KÝ ====================

        // GET: Account/Register
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            if (Session["UserID"] != null)
            {
                return RedirectToHome();
            }

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra username đã tồn tại
                if (IsUsernameExists(model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(model);
                }

                // Kiểm tra email đã tồn tại
                if (IsEmailExists(model.Email))
                {
                    ModelState.AddModelError("Email", "Email đã được đăng ký");
                    return View(model);
                }

                // Tạo tài khoản mới
                int userId = CreateUser(model);

                if (userId > 0)
                {
                    TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError("", "Đăng ký thất bại. Vui lòng thử lại.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                LogError(ex);
                return View(model);
            }
        }

        // ==================== ĐĂNG XUẤT ====================

        // GET: Account/Logout
        [HttpGet]
        public ActionResult Logout()
        {
            if (Session["UserID"] != null)
            {
                int userId = Convert.ToInt32(Session["UserID"]);
                LogActivity(userId, "Đăng xuất hệ thống");
            }

            // Xóa session
            Session.Clear();
            Session.Abandon();

            // Xóa Forms Authentication
            FormsAuthentication.SignOut();

            // Xóa cookies
            if (Request.Cookies["RememberMe"] != null)
            {
                HttpCookie cookie = new HttpCookie("RememberMe");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }

            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        // ==================== PRIVATE METHODS ====================

        /// <summary>
        /// Xác thực người dùng
        /// </summary>
        private User AuthenticateUser(string username, string password)
        {
            User user = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT u.UserID, u.Username, u.Email, u.FullName, 
                           u.PhoneNumber, u.Address, u.RoleID, r.RoleName, 
                           u.IsActive, u.CreatedDate, u.LastLogin
                    FROM Users u
                    INNER JOIN Roles r ON u.RoleID = r.RoleID
                    WHERE (u.Username = @Username OR u.Email = @Username) 
                    AND u.Password = @Password
                    AND u.IsActive = 1";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    
                    // TODO: Trong thực tế phải hash password (SHA256, BCrypt)
                    cmd.Parameters.AddWithValue("@Password", password);

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            user = new User
                            {
                                UserID = Convert.ToInt32(reader["UserID"]),
                                Username = reader["Username"].ToString(),
                                Email = reader["Email"].ToString(),
                                FullName = reader["FullName"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString(),
                                Address = reader["Address"]?.ToString(),
                                RoleID = Convert.ToInt32(reader["RoleID"]),
                                RoleName = reader["RoleName"].ToString(),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                CreatedDate = Convert.ToDateTime(reader["CreatedDate"])
                            };
                        }
                    }
                }
            }

            return user;
        }

        /// <summary>
        /// Kiểm tra username đã tồn tại
        /// </summary>
        private bool IsUsernameExists(string username)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// Kiểm tra email đã tồn tại
        /// </summary>
        private bool IsEmailExists(string email)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        /// <summary>
        /// Tạo user mới
        /// </summary>
        private int CreateUser(RegisterViewModel model)
        {
            int userId = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO Users (Username, Password, Email, FullName, PhoneNumber, Address, RoleID, IsActive, CreatedDate)
                    VALUES (@Username, @Password, @Email, @FullName, @PhoneNumber, @Address, 4, 1, GETDATE());
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    
                    // TODO: Hash password trước khi lưu
                    cmd.Parameters.AddWithValue("@Password", model.Password);
                    
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@FullName", model.FullName);
                    cmd.Parameters.AddWithValue("@PhoneNumber", model.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Address", (object)model.Address ?? DBNull.Value);

                    conn.Open();
                    userId = (int)cmd.ExecuteScalar();
                }
            }

            return userId;
        }

        /// <summary>
        /// Lưu thông tin user vào Session
        /// </summary>
        private void SetUserSession(User user)
        {
            Session["UserID"] = user.UserID;
            Session["Username"] = user.Username;
            Session["FullName"] = user.FullName;
            Session["Email"] = user.Email;
            Session["RoleID"] = user.RoleID;
            Session["RoleName"] = user.RoleName;
        }

        /// <summary>
        /// Cập nhật thời gian đăng nhập cuối
        /// </summary>
        private void UpdateLastLogin(int userId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE Users SET LastLogin = GETDATE() WHERE UserID = @UserID";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Ghi log hoạt động
        /// </summary>
        private void LogActivity(int userId, string action)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO ActivityLogs (UserID, Action, IPAddress, CreatedDate)
                        VALUES (@UserID, @Action, @IPAddress, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.Parameters.AddWithValue("@Action", action);
                        cmd.Parameters.AddWithValue("@IPAddress", GetUserIP());

                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Log Activity Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Lấy IP người dùng
        /// </summary>
        private string GetUserIP()
        {
            string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = Request.ServerVariables["REMOTE_ADDR"];
            }
            return ip ?? "Unknown";
        }

        /// <summary>
        /// Chuyển hướng về trang chủ dựa trên vai trò
        /// </summary>
        private ActionResult RedirectToHome()
        {
            if (Session["RoleName"] != null)
            {
                string roleName = Session["RoleName"].ToString();

                switch (roleName)
                {
                    case "Admin":
                    case "Manager":
                        return RedirectToAction("Index", "Admin");
                    case "Staff":
                        return RedirectToAction("Orders", "Admin");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Ghi log lỗi
        /// </summary>
        private void LogError(Exception ex)
        {
            try
            {
                string logPath = Server.MapPath("~/App_Data/ErrorLog.txt");
                string logMessage = $"[{DateTime.Now}] {ex.Message}\n{ex.StackTrace}\n\n";
                System.IO.File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}