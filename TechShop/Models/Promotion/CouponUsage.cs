using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("CouponUsage")]
    public class CouponUsage
    {
        [Key]
        public int UsageID { get; set; }

        [Required]
        [Display(Name = "Mã giảm giá")]
        public int CouponID { get; set; }

        [Required]
        [Display(Name = "Người dùng")]
        public int UserID { get; set; }

        [Required]
        [Display(Name = "Đơn hàng")]
        public int OrderID { get; set; }

        [Required]

        [Display(Name = "Số tiền giảm")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Ngày sử dụng")]
        public DateTime UsedDate { get; set; }

        // Navigation properties
        [ForeignKey("CouponID")]
        public virtual Coupon Coupon { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }
    }
}