using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("CartItems")]
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }

        [Required]
        [Display(Name = "Giỏ hàng")]
        public int CartID { get; set; }

        [Required]
        [Display(Name = "Sản phẩm")]
        public int ProductID { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Display(Name = "Ngày thêm")]
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        [ForeignKey("CartID")]
        public virtual ShoppingCart ShoppingCart { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product Product { get; set; }

        // Computed property
        [NotMapped]
        public decimal TotalPrice
        {
            get
            {
                return Price * Quantity;
            }
        }
    }
}