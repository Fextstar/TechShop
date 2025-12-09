using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Giảm giá")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Phí vận chuyển")]
        public decimal ShippingFee { get; set; }

        [Required]
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
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; }

        [StringLength(100)]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string Note { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Ngày giao hàng")]
        public DateTime? ShippedDate { get; set; }

        [Display(Name = "Ngày hoàn thành")]
        public DateTime? DeliveredDate { get; set; }

        [Display(Name = "Ngày hủy")]
        public DateTime? CancelledDate { get; set; }

        [StringLength(500)]
        [Display(Name = "Lý do hủy")]
        public string CancelReason { get; set; }

        // ==========================================
        // NAVIGATION PROPERTIES
        // ==========================================

        /// <summary>
        /// User đặt hàng
        /// </summary>
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        /// <summary>
        /// Trạng thái đơn hàng
        /// QUAN TRỌNG: Phải khớp với tên bảng OrderStatus
        /// </summary>
        [ForeignKey("StatusID")]
        public virtual OrderStatus Status { get; set; }

        /// <summary>
        /// Danh sách chi tiết đơn hàng
        /// </summary>
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        /// <summary>
        /// Constructor - Khởi tạo collection
        /// </summary>
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
            OrderDate = DateTime.Now;
            PaymentStatus = "Chưa thanh toán";
        }
    }
}