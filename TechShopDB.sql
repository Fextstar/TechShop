-- =============================================
-- DATABASE: Website Kinh Doanh Thiết Bị Máy Tính
-- Sinh viên: Nguyễn Hồng Phúc - MSSV: 2200010884
-- =============================================

-- Tạo Database
CREATE DATABASE TechShopDB;
GO

USE TechShopDB;
GO

-- =============================================
-- 1. BẢNG NGƯỜI DÙNG VÀ PHÂN QUYỀN
-- =============================================

-- Bảng Vai trò
CREATE TABLE Roles (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng Người dùng
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(255),
    RoleID INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME,
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- =============================================
-- 2. BẢNG DANH MỤC SẢN PHẨM
-- =============================================

-- Bảng Danh mục sản phẩm
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    ParentCategoryID INT NULL,
    ImageURL NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ParentCategoryID) REFERENCES Categories(CategoryID)
);

-- Bảng Thương hiệu
CREATE TABLE Brands (
    BrandID INT PRIMARY KEY IDENTITY(1,1),
    BrandName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    LogoURL NVARCHAR(255),
    Website NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng Nhà cung cấp
CREATE TABLE Suppliers (
    SupplierID INT PRIMARY KEY IDENTITY(1,1),
    SupplierName NVARCHAR(100) NOT NULL,
    ContactPerson NVARCHAR(100),
    Email NVARCHAR(100),
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- =============================================
-- 3. BẢNG SẢN PHẨM
-- =============================================

-- Bảng Sản phẩm
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(255) NOT NULL,
    CategoryID INT NOT NULL,
    BrandID INT NOT NULL,
    SupplierID INT,
    SKU NVARCHAR(50) UNIQUE,
    Description NVARCHAR(MAX),
    Specifications NVARCHAR(MAX),
    Price DECIMAL(18,2) NOT NULL,
    DiscountPrice DECIMAL(18,2),
    StockQuantity INT DEFAULT 0,
    MinStockLevel INT DEFAULT 5,
    Weight DECIMAL(10,2),
    Warranty INT DEFAULT 12, -- Tháng bảo hành
    IsActive BIT DEFAULT 1,
    IsFeatured BIT DEFAULT 0,
    ViewCount INT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME,
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (BrandID) REFERENCES Brands(BrandID),
    FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID)
);

-- Bảng Hình ảnh sản phẩm
CREATE TABLE ProductImages (
    ImageID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT NOT NULL,
    ImageURL NVARCHAR(255) NOT NULL,
    IsPrimary BIT DEFAULT 0,
    DisplayOrder INT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE
);

-- Bảng Đánh giá sản phẩm
CREATE TABLE ProductReviews (
    ReviewID INT PRIMARY KEY IDENTITY(1,1),
    ProductID INT NOT NULL,
    UserID INT NOT NULL,
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    ReviewTitle NVARCHAR(200),
    ReviewContent NVARCHAR(MAX),
    IsApproved BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- =============================================
-- 4. BẢNG GIỎ HÀNG
-- =============================================

-- Bảng Giỏ hàng
CREATE TABLE ShoppingCarts (
    CartID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME,
    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE
);

-- Bảng Chi tiết giỏ hàng
CREATE TABLE CartItems (
    CartItemID INT PRIMARY KEY IDENTITY(1,1),
    CartID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    Price DECIMAL(18,2) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CartID) REFERENCES ShoppingCarts(CartID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- =============================================
-- 5. BẢNG ĐỐN HÀNG
-- =============================================

-- Bảng Trạng thái đơn hàng
CREATE TABLE OrderStatus (
    StatusID INT PRIMARY KEY IDENTITY(1,1),
    StatusName NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    DisplayOrder INT DEFAULT 0
);

-- Bảng Đơn hàng
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    OrderCode NVARCHAR(20) NOT NULL UNIQUE,
    StatusID INT NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    ShippingFee DECIMAL(18,2) DEFAULT 0,
    FinalAmount DECIMAL(18,2) NOT NULL,
    PaymentMethod NVARCHAR(50),
    PaymentStatus NVARCHAR(50) DEFAULT N'Chưa thanh toán',
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerPhone NVARCHAR(20) NOT NULL,
    CustomerEmail NVARCHAR(100),
    ShippingAddress NVARCHAR(500) NOT NULL,
    Note NVARCHAR(500),
    OrderDate DATETIME DEFAULT GETDATE(),
    ShippedDate DATETIME,
    DeliveredDate DATETIME,
    CancelledDate DATETIME,
    CancelReason NVARCHAR(500),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (StatusID) REFERENCES OrderStatus(StatusID)
);

-- Bảng Chi tiết đơn hàng
CREATE TABLE OrderDetails (
    OrderDetailID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    ProductName NVARCHAR(255) NOT NULL,
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountPrice DECIMAL(18,2),
    TotalPrice DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- =============================================
-- 6. BẢNG KHUYẾN MÃI
-- =============================================

-- Bảng Mã giảm giá
CREATE TABLE Coupons (
    CouponID INT PRIMARY KEY IDENTITY(1,1),
    CouponCode NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    DiscountType NVARCHAR(20) NOT NULL, -- 'Percentage' hoặc 'FixedAmount'
    DiscountValue DECIMAL(18,2) NOT NULL,
    MinOrderAmount DECIMAL(18,2) DEFAULT 0,
    MaxDiscountAmount DECIMAL(18,2),
    UsageLimit INT,
    UsedCount INT DEFAULT 0,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng Sử dụng mã giảm giá
CREATE TABLE CouponUsage (
    UsageID INT PRIMARY KEY IDENTITY(1,1),
    CouponID INT NOT NULL,
    UserID INT NOT NULL,
    OrderID INT NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL,
    UsedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CouponID) REFERENCES Coupons(CouponID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);

-- =============================================
-- 7. BẢNG TIN TỨC VÀ NỘI DUNG
-- =============================================

-- Bảng Bài viết/Tin tức
CREATE TABLE Posts (
    PostID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255) NOT NULL,
    Slug NVARCHAR(255) NOT NULL UNIQUE,
    Content NVARCHAR(MAX),
    ThumbnailURL NVARCHAR(255),
    AuthorID INT NOT NULL,
    ViewCount INT DEFAULT 0,
    IsPublished BIT DEFAULT 0,
    PublishedDate DATETIME,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME,
    FOREIGN KEY (AuthorID) REFERENCES Users(UserID)
);

-- Bảng Banner quảng cáo
CREATE TABLE Banners (
    BannerID INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(200),
    ImageURL NVARCHAR(255) NOT NULL,
    LinkURL NVARCHAR(255),
    Position NVARCHAR(50), -- 'Homepage', 'Sidebar', etc.
    DisplayOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    StartDate DATETIME,
    EndDate DATETIME,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- =============================================
-- 8. BẢNG LIÊN HỆ VÀ HỖ TRỢ
-- =============================================

-- Bảng Liên hệ
CREATE TABLE Contacts (
    ContactID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(20),
    Subject NVARCHAR(255),
    Message NVARCHAR(MAX) NOT NULL,
    IsRead BIT DEFAULT 0,
    IsReplied BIT DEFAULT 0,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng Câu hỏi thường gặp
CREATE TABLE FAQs (
    FAQID INT PRIMARY KEY IDENTITY(1,1),
    Question NVARCHAR(500) NOT NULL,
    Answer NVARCHAR(MAX) NOT NULL,
    CategoryFAQ NVARCHAR(100),
    DisplayOrder INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- =============================================
-- 9. BẢNG CẤU HÌNH HỆ THỐNG
-- =============================================

-- Bảng Cấu hình website
CREATE TABLE SiteSettings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(MAX),
    Description NVARCHAR(255),
    UpdatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng Lịch sử hoạt động
CREATE TABLE ActivityLogs (
    LogID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT,
    Action NVARCHAR(255) NOT NULL,
    TableName NVARCHAR(100),
    RecordID INT,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    IPAddress NVARCHAR(50),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- =============================================
-- 10. THÊM DỮ LIỆU MẪU
-- =============================================

-- Thêm vai trò
INSERT INTO Roles (RoleName, Description) VALUES
(N'Admin', N'Quản trị viên hệ thống'),
(N'Manager', N'Quản lý cửa hàng'),
(N'Staff', N'Nhân viên bán hàng'),
(N'Customer', N'Khách hàng');

-- Thêm trạng thái đơn hàng
INSERT INTO OrderStatus (StatusName, Description, DisplayOrder) VALUES
(N'Chờ xác nhận', N'Đơn hàng mới, chờ xác nhận', 1),
(N'Đã xác nhận', N'Đơn hàng đã được xác nhận', 2),
(N'Đang chuẩn bị', N'Đang chuẩn bị hàng', 3),
(N'Đang giao', N'Đơn hàng đang được vận chuyển', 4),
(N'Đã giao', N'Đã giao hàng thành công', 5),
(N'Đã hủy', N'Đơn hàng đã bị hủy', 6),
(N'Trả hàng', N'Khách hàng trả hàng', 7);

-- Thêm danh mục sản phẩm chính
INSERT INTO Categories (CategoryName, Description, ParentCategoryID) VALUES
(N'Laptop', N'Máy tính xách tay các loại', NULL),
(N'Laptop Gaming', N'Laptop chuyên game hiệu năng cao', NULL),
(N'Main, CPU, VGA', N'Mainboard, CPU, Card đồ họa', NULL),
(N'Case, Nguồn, Tản', N'Vỏ case, nguồn máy tính, tản nhiệt', NULL),
(N'Ổ cứng, RAM, Thẻ nhớ', N'Ổ cứng SSD/HDD, RAM, thẻ nhớ', NULL),
(N'Loa, Micro, Webcam', N'Thiết bị âm thanh và camera', NULL),
(N'Màn hình', N'Màn hình máy tính các loại', NULL),
(N'Bàn phím', N'Bàn phím cơ, gaming, văn phòng', NULL),
(N'Chuột + Lót chuột', N'Chuột gaming, văn phòng và pad', NULL),
(N'Tai Nghe', N'Tai nghe gaming, tai nghe không dây', NULL),
(N'Ghế - Bàn', N'Ghế gaming, bàn gaming', NULL),
(N'Phần mềm, mạng', N'Phần mềm bản quyền, thiết bị mạng', NULL),
(N'Handheld, Console', N'Nintendo Switch, PS5, Xbox, Steam Deck', NULL),
(N'Phụ kiện', N'Hub, sạc, cáp và phụ kiện khác', NULL);

-- Thêm danh mục con cho Laptop
INSERT INTO Categories (CategoryName, Description, ParentCategoryID) VALUES
(N'Laptop Văn Phòng', N'Laptop phục vụ công việc văn phòng', 1),
(N'Laptop Đồ Họa', N'Laptop cho thiết kế đồ họa, render', 1),
(N'Laptop Mỏng Nhẹ', N'Laptop siêu mỏng, di động', 1);

-- Thêm danh mục con cho Main, CPU, VGA
INSERT INTO Categories (CategoryName, Description, ParentCategoryID) VALUES
(N'CPU - Bộ vi xử lý', N'Intel, AMD CPU', 4),
(N'Mainboard - Bo mạch chủ', N'Bo mạch chủ Intel, AMD', 4),
(N'VGA - Card màn hình', N'Card đồ họa NVIDIA, AMD', 4);

-- Thêm danh mục con cho Case, Nguồn, Tản
INSERT INTO Categories (CategoryName, Description, ParentCategoryID) VALUES
(N'Case - Vỏ máy tính', N'Vỏ case gaming, RGB', 5),
(N'PSU - Nguồn máy tính', N'Nguồn 80 Plus Bronze, Gold, Platinum', 5),
(N'Tản nhiệt khí', N'Tản nhiệt CPU bằng gió', 5),
(N'Tản nhiệt nước AIO', N'Tản nhiệt nước All-in-One', 5);

-- Thêm thương hiệu
INSERT INTO Brands (BrandName, Description) VALUES
(N'ASUS', N'Thương hiệu linh kiện và laptop hàng đầu'),
(N'MSI', N'Chuyên về gaming gear và laptop'),
(N'Dell', N'Thương hiệu laptop và PC uy tín'),
(N'HP', N'Hewlett-Packard - laptop và máy in'),
(N'Lenovo', N'Thương hiệu công nghệ Trung Quốc'),
(N'Acer', N'Laptop và màn hình giá tốt'),
(N'GIGABYTE', N'Mainboard, VGA và laptop gaming'),
(N'Intel', N'Vi xử lý hàng đầu thế giới'),
(N'AMD', N'Vi xử lý và card đồ họa'),
(N'NVIDIA', N'Card đồ họa GeForce RTX'),
(N'Kingston', N'RAM và ổ cứng SSD'),
(N'Corsair', N'RAM, nguồn, case và gaming gear'),
(N'Logitech', N'Thiết bị ngoại vi cao cấp'),
(N'Razer', N'Gaming gear cao cấp'),
(N'SteelSeries', N'Tai nghe và gaming gear'),
(N'Akko', N'Bàn phím cơ custom'),
(N'DareU', N'Gaming gear giá tốt'),
(N'Fuhlen', N'Gaming gear phổ thông'),
(N'Samsung', N'Màn hình và SSD'),
(N'LG', N'Màn hình gaming và văn phòng'),
(N'ViewSonic', N'Màn hình chuyên nghiệp'),
(N'Western Digital', N'Ổ cứng HDD và SSD'),
(N'Seagate', N'Ổ cứng HDD'),
(N'Sony', N'PlayStation Console'),
(N'Nintendo', N'Nintendo Switch'),
(N'Microsoft', N'Xbox Console và phần mềm');

-- Thêm cấu hình website
INSERT INTO SiteSettings (SettingKey, SettingValue, Description) VALUES
(N'SiteName', N'Computer Shop', N'Tên website'),
(N'SitePhone', N'0704449834', N'Số điện thoại liên hệ'),
(N'SiteEmail', N'support@computershop.vn', N'Email liên hệ'),
(N'SiteAddress', N'TP HCM, Việt Nam', N'Địa chỉ cửa hàng'),
(N'ShippingFee', N'30000', N'Phí vận chuyển mặc định'),
(N'FreeShippingAmount', N'500000', N'Miễn phí ship cho đơn từ số tiền này');

GO

-- =============================================
-- INDEXES ĐỂ TỐI ƯU HIỆU SUẤT
-- =============================================

CREATE INDEX IX_Products_CategoryID ON Products(CategoryID);
CREATE INDEX IX_Products_BrandID ON Products(BrandID);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Orders_UserID ON Orders(UserID);
CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
CREATE INDEX IX_OrderDetails_OrderID ON OrderDetails(OrderID);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_Username ON Users(Username);

GO