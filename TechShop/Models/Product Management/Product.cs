using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

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

        // ================== PRICE ==================

        [Required(ErrorMessage = "Giá không được để trống")]
        [Display(Name = "Giá gốc")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Display(Name = "Giá khuyến mãi")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn hoặc bằng 0")]
        public decimal? DiscountPrice { get; set; }

        // ================== STOCK ==================

        [Display(Name = "Số lượng tồn kho")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int StockQuantity { get; set; }

        [Display(Name = "Mức tồn kho tối thiểu")]
        public int MinStockLevel { get; set; }

        // ================== OTHER ==================

        [Display(Name = "Trọng lượng (kg)")]
        public decimal? Weight { get; set; }

        [Display(Name = "Bảo hành (tháng)")]
        public int Warranty { get; set; }

        [Display(Name = "Hoạt động")]
        public bool IsActive { get; set; }

        [Display(Name = "Sản phẩm nổi bật")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Lượt xem")]
        public int ViewCount { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // ==========================================
        // NAVIGATION PROPERTIES
        // ==========================================

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

        // ==========================================
        // COMPUTED PROPERTIES (NOT MAPPED)
        // ==========================================

        /// <summary>
        /// Ảnh chính
        /// </summary>
        [NotMapped]
        public string PrimaryImageURL
        {
            get
            {
                if (ProductImages != null && ProductImages.Any())
                {
                    var primary = ProductImages.FirstOrDefault(i => i.IsPrimary);
                    if (primary != null)
                        return primary.ImageURL;

                    return ProductImages.OrderBy(i => i.DisplayOrder).First().ImageURL;
                }
                return "/Content/images/no-image.png";
            }
        }

        /// <summary>
        /// Có giảm giá không
        /// </summary>
        [NotMapped]
        public bool HasDiscount
        {
            get
            {
                return DiscountPrice.HasValue
                       && DiscountPrice.Value > 0
                       && DiscountPrice.Value < Price;
            }
        }

        /// <summary>
        /// Giá dùng cho xử lý logic
        /// </summary>
        [NotMapped]
        public decimal DisplayPrice
        {
            get
            {
                return HasDiscount ? DiscountPrice.Value : Price;
            }
        }

        /// <summary>
        /// Giá gốc hiển thị VNĐ (18.000.000 Đồng)
        /// </summary>
        [NotMapped]
        public string PriceVND
        {
            get
            {
                return string.Format(new CultureInfo("vi-VN"), "{0:N0} Đồng", Price);
            }
        }

        /// <summary>
        /// Giá khuyến mãi hiển thị VNĐ
        /// </summary>
        [NotMapped]
        public string DiscountPriceVND
        {
            get
            {
                if (DiscountPrice.HasValue)
                {
                    return string.Format(new CultureInfo("vi-VN"), "{0:N0} Đồng", DiscountPrice.Value);
                }
                return null;
            }
        }

        /// <summary>
        /// Giá hiển thị ưu tiên khuyến mãi (VNĐ)
        /// </summary>
        [NotMapped]
        public string DisplayPriceVND
        {
            get
            {
                var price = HasDiscount ? DiscountPrice.Value : Price;
                return string.Format(new CultureInfo("vi-VN"), "{0:N0} Đồng", price);
            }
        }

        /// <summary>
        /// % giảm giá
        /// </summary>
        [NotMapped]
        public int DiscountPercentage
        {
            get
            {
                if (HasDiscount)
                {
                    return (int)Math.Round(((Price - DiscountPrice.Value) / Price) * 100);
                }
                return 0;
            }
        }

        /// <summary>
        /// Trạng thái tồn kho
        /// </summary>
        [NotMapped]
        public string StockStatus
        {
            get
            {
                if (StockQuantity <= 0) return "Hết hàng";
                if (StockQuantity <= MinStockLevel) return "Sắp hết";
                return "Còn hàng";
            }
        }

        /// <summary>
        /// Đánh giá trung bình
        /// </summary>
        [NotMapped]
        public double AverageRating
        {
            get
            {
                if (ProductReviews != null && ProductReviews.Any(r => r.IsApproved))
                    return ProductReviews.Where(r => r.IsApproved).Average(r => r.Rating);
                return 0;
            }
        }

        /// <summary>
        /// Số lượng đánh giá
        /// </summary>
        [NotMapped]
        public int ReviewCount
        {
            get
            {
                return ProductReviews?.Count(r => r.IsApproved) ?? 0;
            }
        }

        // ==========================================
        // CONSTRUCTOR
        // ==========================================

        public Product()
        {
            ProductImages = new HashSet<ProductImage>();
            ProductReviews = new HashSet<ProductReview>();
            OrderDetails = new HashSet<OrderDetail>();
            CartItems = new HashSet<CartItem>();

            CreatedDate = DateTime.Now;
            IsActive = true;
            IsFeatured = false;
            ViewCount = 0;
            StockQuantity = 0;
            MinStockLevel = 5;
            Warranty = 12;
        }
    }
}
