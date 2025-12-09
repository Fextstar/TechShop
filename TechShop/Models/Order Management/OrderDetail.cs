using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechShop.Models
{
    [Table("OrderDetails")]
    public class OrderDetail
    {
        [Key]
        public int OrderDetailID { get; set; }

        [Required]
        [Display(Name = "Đơn hàng")]
        public int OrderID { get; set; }

        [Required]
        [Display(Name = "Sản phẩm")]
        public int ProductID { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }

        [Required]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required]
        [Display(Name = "Đơn giá")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Giá giảm")]
        public decimal? DiscountPrice { get; set; }

        [Required]
        [Display(Name = "Thành tiền")]
        public decimal TotalPrice { get; set; }

        // ==========================================
        // NAVIGATION PROPERTIES
        // ==========================================

        /// <summary>
        /// Đơn hàng chứa chi tiết này
        /// </summary>
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        /// <summary>
        /// Sản phẩm trong chi tiết đơn hàng
        /// </summary>
        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}