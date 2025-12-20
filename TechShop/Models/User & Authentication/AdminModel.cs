using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TechShop.Models.Statistics;

namespace TechShop.Models.User___Authentication
{
    /// <summary>
    /// ViewModel cho Dashboard
    /// File: Models/AdminModels.cs
    /// </summary>
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockProducts { get; set; }
        public List<Order> RecentOrders { get; set; }
    }

    /// <summary>
    /// ViewModel cho Product
    /// </summary>
    public class ProductViewModel
    {
        public int ProductID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(255, ErrorMessage = "Tên sản phẩm không quá 255 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Danh mục")]
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thương hiệu")]
        [Display(Name = "Thương hiệu")]
        public int BrandID { get; set; }

        [Display(Name = "Nhà cung cấp")]
        public int? SupplierID { get; set; }

        [StringLength(50, ErrorMessage = "Mã SKU không quá 50 ký tự")]
        [Display(Name = "Mã SKU")]
        public string SKU { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Display(Name = "Thông số kỹ thuật")]
        public string Specifications { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        [Display(Name = "Giá gốc")]
        public decimal Price { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn 0")]
        [Display(Name = "Giá khuyến mãi")]
        public decimal? DiscountPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số lượng tồn kho")]
        public int StockQuantity { get; set; }

        [Display(Name = "Mức tồn tối thiểu")]
        public int MinStockLevel { get; set; } = 5;

        [Display(Name = "Trọng lượng (kg)")]
        public decimal? Weight { get; set; }

        [Display(Name = "Bảo hành (tháng)")]
        public int Warranty { get; set; } = 12;

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Nổi bật")]
        public bool IsFeatured { get; set; }

        // Thông tin bổ sung (không lưu DB)
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string SupplierName { get; set; }
    }

    /// <summary>
    /// ViewModel cho Order
    /// </summary>
    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public string OrderCode { get; set; }
        public int UserID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public int StatusID { get; set; }
        public string StatusName { get; set; }
        public string Note { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string CancelReason { get; set; }

        // Chi tiết đơn hàng
        public System.Collections.Generic.List<OrderDetailViewModel> OrderDetails { get; set; }
    }

    /// <summary>
    /// Chi tiết đơn hàng
    /// </summary>
    public class OrderDetailViewModel
    {
        public int OrderDetailID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? DiscountPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    /// <summary>
    /// ViewModel cho Category
    /// </summary>
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, ErrorMessage = "Tên danh mục không quá 100 ký tự")]
        [Display(Name = "Tên danh mục")]
        public string CategoryName { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        public string Description { get; set; }

        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryID { get; set; }

        [Display(Name = "Hình ảnh URL")]
        public string ImageURL { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;

        public string ParentCategoryName { get; set; }
    }

    /// <summary>
    /// ViewModel cho Brand
    /// </summary>
    public class BrandViewModel
    {
        public int BrandID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên thương hiệu")]
        [StringLength(100, ErrorMessage = "Tên thương hiệu không quá 100 ký tự")]
        [Display(Name = "Tên thương hiệu")]
        public string BrandName { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        public string Description { get; set; }

        [Display(Name = "Logo URL")]
        public string LogoURL { get; set; }

        [Display(Name = "Website")]
        public string Website { get; set; }

        [Display(Name = "Kích hoạt")]
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// ViewModel cho User Management
    /// </summary>
    public class UserManagementViewModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    /// <summary>
    /// ViewModel cho Statistics
    /// </summary>
    public class StatisticsViewModel
    {
        public string Period { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int ProductCount { get; set; }
        public int CustomerCount { get; set; }
    }

    /// <summary>
    /// ViewModel cho Statistics nâng cao (Admin - Charts & Reports)
    /// </summary>
    public class AdminStatisticsViewModel
    {
        public string Period { get; set; }

        // Biểu đồ doanh thu
        public List<RevenuePointViewModel> RevenueData { get; set; }

        // Top sản phẩm
        public List<TopProductViewModel> TopProducts { get; set; }

        // Thống kê theo danh mục
        public List<CategoryStatViewModel> CategoryStats { get; set; }

        // Trạng thái đơn hàng
        public List<OrderStatusStatViewModel> OrderStatusStats { get; set; }

        // Tổng quan
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
    }

    public class RevenuePointViewModel
    {
        public string Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class TopProductViewModel
    {
        public string ProductName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CategoryStatViewModel
    {
        public string CategoryName { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class OrderStatusStatViewModel
    {
        public string StatusName { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

}


