using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace TechShop.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("TechShopConnection")
        {
            // TẮT Database Initializer
            Database.SetInitializer<ApplicationDbContext>(null);

            // Tắt lazy loading và proxy
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
            Configuration.ValidateOnSaveEnabled = true;
        }

        // Users & Roles
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }

        // Categories & Brands
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        // Products
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }

        // Shopping Cart
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // Orders
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // Coupons
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CouponUsage> CouponUsages { get; set; }

        // Content
        public DbSet<Post> Posts { get; set; }
        public DbSet<Banner> Banners { get; set; }

        // Support
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<FAQ> FAQs { get; set; }

        // System
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // ===== CẤU HÌNH DECIMAL - QUAN TRỌNG =====
            // Thiết lập tất cả decimal thành decimal(18,2)
            modelBuilder.Properties<decimal>()
                .Configure(c => c.HasPrecision(18, 2));

            // Loại bỏ quy ước đặt tên bảng số nhiều
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // ===== CẤU HÌNH QUAN HỆ =====

            // User - Role
            modelBuilder.Entity<User>()
                .HasRequired(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .WillCascadeOnDelete(false);

            // Category - ParentCategory (Self-referencing)
            modelBuilder.Entity<Category>()
                .HasOptional(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryID)
                .WillCascadeOnDelete(false);

            // Product - Category
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID)
                .WillCascadeOnDelete(false);

            // Product - Brand
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandID)
                .WillCascadeOnDelete(false);

            // Product - Supplier
            modelBuilder.Entity<Product>()
                .HasOptional(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierID)
                .WillCascadeOnDelete(false);

            // ProductImage - Product (Cascade Delete)
            modelBuilder.Entity<ProductImage>()
                .HasRequired(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductID)
                .WillCascadeOnDelete(true);

            // ProductReview - Product (Cascade Delete)
            modelBuilder.Entity<ProductReview>()
                .HasRequired(pr => pr.Product)
                .WithMany(p => p.ProductReviews)
                .HasForeignKey(pr => pr.ProductID)
                .WillCascadeOnDelete(true);

            // ProductReview - User
            modelBuilder.Entity<ProductReview>()
                .HasRequired(pr => pr.User)
                .WithMany(u => u.ProductReviews)
                .HasForeignKey(pr => pr.UserID)
                .WillCascadeOnDelete(false);

            // ShoppingCart - User (Cascade Delete)
            modelBuilder.Entity<ShoppingCart>()
                .HasRequired(sc => sc.User)
                .WithMany(u => u.ShoppingCarts)
                .HasForeignKey(sc => sc.UserID)
                .WillCascadeOnDelete(true);

            // CartItem - ShoppingCart (Cascade Delete)
            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.ShoppingCart)
                .WithMany(sc => sc.CartItems)
                .HasForeignKey(ci => ci.CartID)
                .WillCascadeOnDelete(true);

            // CartItem - Product
            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductID)
                .WillCascadeOnDelete(false);

            // Order - User
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserID)
                .WillCascadeOnDelete(false);

            // Order - Status
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StatusID)
                .WillCascadeOnDelete(false);

            // OrderDetail - Order (Cascade Delete)
            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderID)
                .WillCascadeOnDelete(true);

            // OrderDetail - Product
            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductID)
                .WillCascadeOnDelete(false);

            // CouponUsage - Coupon
            modelBuilder.Entity<CouponUsage>()
                .HasRequired(cu => cu.Coupon)
                .WithMany()
                .HasForeignKey(cu => cu.CouponID)
                .WillCascadeOnDelete(false);

            // CouponUsage - User
            modelBuilder.Entity<CouponUsage>()
                .HasRequired(cu => cu.User)
                .WithMany()
                .HasForeignKey(cu => cu.UserID)
                .WillCascadeOnDelete(false);

            // CouponUsage - Order
            modelBuilder.Entity<CouponUsage>()
                .HasRequired(cu => cu.Order)
                .WithMany()
                .HasForeignKey(cu => cu.OrderID)
                .WillCascadeOnDelete(false);

            // Post - Author
            modelBuilder.Entity<Post>()
                .HasRequired(p => p.Author)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.AuthorID)
                .WillCascadeOnDelete(false);

            // ActivityLog - User
            modelBuilder.Entity<ActivityLog>()
                .HasOptional(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserID)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }
    }
}