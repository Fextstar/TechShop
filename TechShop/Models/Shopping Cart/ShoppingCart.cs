using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("ShoppingCarts")]
    public class ShoppingCart
    {
        [Key]
        public int CartID { get; set; }

        [Required]
        [Display(Name = "Người dùng")]
        public int UserID { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        public virtual ICollection<CartItem> CartItems { get; set; }

        // Computed properties
        [NotMapped]
        public decimal TotalAmount
        {
            get
            {
                return CartItems?.Sum(item => item.TotalPrice) ?? 0;
            }
        }

        [NotMapped]
        public int TotalItems
        {
            get
            {
                return CartItems?.Sum(item => item.Quantity) ?? 0;
            }
        }
    }


    // Model cho Session-based Cart (không dùng database)
    public class SessionCartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageURL { get; set; }

        public decimal TotalPrice
        {
            get { return Price * Quantity; }
        }
    }
}