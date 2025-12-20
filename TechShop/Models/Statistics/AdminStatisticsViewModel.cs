using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TechShop.Models.Statistics
{
    public class RevenuePointViewModel
    {
        public string Date { get; set; }          // "dd/MM" hoặc "T1", "T2",...
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

    public class AdminStatisticsViewModel
    {
        public string Period { get; set; }

        public List<RevenuePointViewModel> RevenueData { get; set; }

        public List<TopProductViewModel> TopProducts { get; set; }

        public List<CategoryStatViewModel> CategoryStats { get; set; }

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

}