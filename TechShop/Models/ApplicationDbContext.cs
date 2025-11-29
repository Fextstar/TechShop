using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace TechShop.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() : base("TechShopConnection")
        {
            // TẮT Database Initializer - QUAN TRỌNG!
            Database.SetInitializer<ApplicationDbContext>(null);

            // Tắt lazy loading và proxy
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;

            // TẮT VALIDATION để tránh lỗi decimal
            Configuration.ValidateOnSaveEnabled = false;
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
            // User - Role
            modelBuilder.Entity<User>()
                .HasRequired(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .WillCascadeOnDelete(false);

            // Category - ParentCategory
            modelBuilder.Entity<Category>()
                .HasOptional(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryID)
                .WillCascadeOnDelete(false);

            // Product - Category, Brand, Supplier
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasRequired(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasOptional(p => p.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SupplierID)
                .WillCascadeOnDelete(false);

            // ProductImage - Product
            modelBuilder.Entity<ProductImage>()
                .HasRequired(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductID)
                .WillCascadeOnDelete(true);

            // ProductReview - Product & User
            modelBuilder.Entity<ProductReview>()
                .HasRequired(pr => pr.Product)
                .WithMany(p => p.ProductReviews)
                .HasForeignKey(pr => pr.ProductID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<ProductReview>()
                .HasRequired(pr => pr.User)
                .WithMany(u => u.ProductReviews)
                .HasForeignKey(pr => pr.UserID)
                .WillCascadeOnDelete(false);

            // ShoppingCart - User
            modelBuilder.Entity<ShoppingCart>()
                .HasRequired(sc => sc.User)
                .WithMany(u => u.ShoppingCarts)
                .HasForeignKey(sc => sc.UserID)
                .WillCascadeOnDelete(true);

            // CartItem - ShoppingCart & Product
            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.ShoppingCart)
                .WithMany(sc => sc.CartItems)
                .HasForeignKey(ci => ci.CartID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<CartItem>()
                .HasRequired(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductID)
                .WillCascadeOnDelete(false);

            // Order - User & Status
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StatusID)
                .WillCascadeOnDelete(false);

            // OrderDetail - Order & Product
            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderID)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<OrderDetail>()
                .HasRequired(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductID)
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