using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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

        [Required(ErrorMessage = "Giá không được để trống")]
        [Display(Name = "Giá gốc")]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        [DisplayFormat(DataFormatString = "{0:N0} đ", ApplyFormatInEditMode = false)]
        public decimal Price { get; set; }


        [Display(Name = "Giá khuyến mãi")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn 0")]
        public decimal? DiscountPrice { get; set; }

        [Display(Name = "Số lượng tồn kho")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int StockQuantity { get; set; }

        [Display(Name = "Mức tồn kho tối thiểu")]
        public int MinStockLevel { get; set; }

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

        /// <summary>
        /// Danh sách hình ảnh của sản phẩm
        /// </summary>
        public virtual ICollection<ProductImage> ProductImages { get; set; }

        /// <summary>
        /// Danh sách đánh giá sản phẩm
        /// </summary>
        public virtual ICollection<ProductReview> ProductReviews { get; set; }

        /// <summary>
        /// Danh sách chi tiết đơn hàng
        /// </summary>
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }

        /// <summary>
        /// Danh sách trong giỏ hàng
        /// </summary>
        public virtual ICollection<CartItem> CartItems { get; set; }

        // ==========================================
        // COMPUTED PROPERTIES (NOT MAPPED)
        // ==========================================

        /// <summary>
        /// Lấy URL ảnh chính của sản phẩm
        /// </summary>
        [NotMapped]
        public string PrimaryImageURL
        {
            get
            {
                if (ProductImages != null && ProductImages.Any())
                {
                    var primaryImage = ProductImages.FirstOrDefault(img => img.IsPrimary);
                    if (primaryImage != null)
                        return primaryImage.ImageURL;

                    // Nếu không có ảnh primary, lấy ảnh đầu tiên
                    var firstImage = ProductImages.OrderBy(img => img.DisplayOrder).FirstOrDefault();
                    if (firstImage != null)
                        return firstImage.ImageURL;
                }

                // Trả về ảnh mặc định nếu không có ảnh nào
                return "/Content/images/no-image.png";
            }
        }

        /// <summary>
        /// Kiểm tra có giảm giá không
        /// </summary>
        [NotMapped]
        public bool HasDiscount
        {
            get
            {
                return DiscountPrice.HasValue && DiscountPrice.Value > 0 && DiscountPrice.Value < Price;
            }
        }

        /// <summary>
        /// Giá hiển thị (ưu tiên giá khuyến mãi nếu có)
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
        /// Phần trăm giảm giá
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
                if (StockQuantity <= 0)
                    return "Hết hàng";
                else if (StockQuantity <= MinStockLevel)
                    return "Sắp hết";
                else
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
                {
                    return ProductReviews.Where(r => r.IsApproved).Average(r => r.Rating);
                }
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
                if (ProductReviews != null)
                {
                    return ProductReviews.Count(r => r.IsApproved);
                }
                return 0;
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