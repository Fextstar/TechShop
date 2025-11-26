using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

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
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá giảm")]
        public decimal? DiscountPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng giá")]
        public decimal TotalPrice { get; set; }

        // Navigation properties
        [ForeignKey("OrderID")]
        public virtual Order Order { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }
    }
}