using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        public int OrderID { get; set; }

        [Required]
        [Display(Name = "Người dùng")]
        public int UserID { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã đơn hàng")]
        public string OrderCode { get; set; }

        [Required]
        [Display(Name = "Trạng thái")]
        public int StatusID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giảm giá")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Phí vận chuyển")]
        public decimal ShippingFee { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng thanh toán")]
        public decimal FinalAmount { get; set; }

        [StringLength(50)]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Tên khách hàng")]
        public string CustomerName { get; set; }

        [Required]
        [StringLength(20)]
        [Phone]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        [Display(Name = "Ngày đặt")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Ngày giao")]
        public DateTime? ShippedDate { get; set; }

        [Display(Name = "Ngày hoàn thành")]
        public DateTime? DeliveredDate { get; set; }

        [Display(Name = "Ngày hủy")]
        public DateTime? CancelledDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do hủy")]
        public string CancelReason { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("StatusID")]
        public virtual OrderStatus Status { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        // Computed properties
        [NotMapped]
        public int TotalItems
        {
            get
            {
                return OrderDetails?.Sum(d => d.Quantity) ?? 0;
            }
        }
    }
}