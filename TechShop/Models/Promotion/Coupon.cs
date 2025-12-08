using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Coupons")]
    public class Coupon
    {
        [Key]
        public int CouponID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Mã giảm giá")]
        public string CouponCode { get; set; }

        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Loại giảm giá")]
        public string DiscountType { get; set; } // 'Percentage' hoặc 'FixedAmount'

        [Required]

        [Display(Name = "Giá trị giảm")]
        public decimal DiscountValue { get; set; }


        [Display(Name = "Giá trị đơn tối thiểu")]
        public decimal MinOrderAmount { get; set; }


        [Display(Name = "Giảm tối đa")]
        public decimal? MaxDiscountAmount { get; set; }

        [Display(Name = "Số lần sử dụng tối đa")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Đã sử dụng")]
        public int UsedCount { get; set; }

        [Required]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public virtual ICollection<CouponUsage> CouponUsages { get; set; }

        // Computed properties
        [NotMapped]
        public bool IsValid
        {
            get
            {
                var now = DateTime.Now;
                return IsActive
                    && now >= StartDate
                    && now <= EndDate
                    && (!UsageLimit.HasValue || UsedCount < UsageLimit.Value);
            }
        }

        [NotMapped]
        public int RemainingUsage
        {
            get
            {
                if (!UsageLimit.HasValue) return int.MaxValue;
                return Math.Max(0, UsageLimit.Value - UsedCount);
            }
        }
    }

}