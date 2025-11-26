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
            // Disable lazy loading if needed
            // this.Configuration.LazyLoadingEnabled = false;
            // this.Configuration.ProxyCreationEnabled = false;
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
            // Configure relationships

            // User - Role (Many-to-One)
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

            // Product relationships
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

            // ProductImage - Product (Many-to-One)
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

            // ShoppingCart - User (One-to-Many)
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

            // Order - User & OrderStatus
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

            // Post - Author (User)
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

            // Configure decimal precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.DiscountPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Weight)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.FinalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }

        internal void Dispose()
        {
            throw new NotImplementedException();
        }

        internal void SaveChanges()
        {
            throw new NotImplementedException();
        }
    }
}