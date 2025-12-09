using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Services.Description;

namespace TechShop.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Required]
        [Display(Name = "Vai trò")]
        public int RoleID { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Đăng nhập lần cuối")]
        public DateTime? LastLogin { get; set; }

        // Navigation properties
        [ForeignKey("RoleID")]
        public virtual Role Role { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
        public virtual ICollection<ProductReview> ProductReviews { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
    }
}
