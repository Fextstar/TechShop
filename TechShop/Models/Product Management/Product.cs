using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(255)]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }

        [Required]
        [Display(Name = "Danh mục")]
        public int CategoryID { get; set; }

        [Required]
        [Display(Name = "Thương hiệu")]
        public int BrandID { get; set; }

        [Display(Name = "Nhà cung cấp")]
        public int? SupplierID { get; set; }

        [StringLength(50)]
        [Display(Name = "Mã SKU")]
        public string SKU { get; set; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Display(Name = "Thông số kỹ thuật")]
        [DataType(DataType.MultilineText)]
        public string Specifications { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm không được để trống")]
        [Display(Name = "Giá")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        [Column(TypeName = "decimal")]
        public decimal Price { get; set; }

        [Display(Name = "Giá giảm")]
        [Column(TypeName = "decimal")]
        public decimal? DiscountPrice { get; set; }

        [Display(Name = "Số lượng tồn")]
        public int StockQuantity { get; set; }

        [Display(Name = "Tồn kho tối thiểu")]
        public int MinStockLevel { get; set; }

        [Display(Name = "Trọng lượng (kg)")]
        [Column(TypeName = "decimal")]
        public decimal? Weight { get; set; }

        [Display(Name = "Bảo hành (tháng)")]
        public int Warranty { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; }

        [Display(Name = "Nổi bật")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation properties
        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }

        [ForeignKey("BrandID")]
        public virtual Brand Brand { get; set; }

        [ForeignKey("SupplierID")]
        public virtual Supplier Supplier { get; set; }

        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<ProductReview> ProductReviews { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<CartItem> CartItems { get; set; }

        // Computed property
        [NotMapped]
        public string PrimaryImageUrl
        {
            get
            {
                var primaryImage = ProductImages?.FirstOrDefault(i => i.IsPrimary);
                return primaryImage?.ImageURL ?? "/Content/Images/no-image.jpg";
            }
        }

        [NotMapped]
        public decimal DisplayPrice
        {
            get
            {
                return DiscountPrice ?? Price;
            }
        }

        [NotMapped]
        public bool HasDiscount
        {
            get
            {
                return DiscountPrice.HasValue && DiscountPrice.Value < Price;
            }
        }

        [NotMapped]
        public decimal DiscountPercentage
        {
            get
            {
                if (!HasDiscount) return 0;
                return Math.Round((Price - DiscountPrice.Value) / Price * 100, 0);
            }
        }
    }
}