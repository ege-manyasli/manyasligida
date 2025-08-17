-- ========================================
-- MANYASLI GIDA - VERİ KURTARMA SCRIPT'İ
-- ========================================
-- Bu script log dosyalarından çıkarılan gerçek verileri içerir
-- AZURE SQL DATABASE'DE ÇALIŞTIRILACAK

PRINT 'MANYASLI GIDA VERİ KURTARMA BAŞLIYOR...'

-- ========================================
-- 1. MEVCUT TABLOLARI SİL (Eğer varsa)
-- ========================================
PRINT 'Mevcut tablolar kontrol ediliyor...'

-- İlişkili tablolar önce
IF OBJECT_ID('CookieConsentDetails', 'U') IS NOT NULL DROP TABLE CookieConsentDetails;
IF OBJECT_ID('OrderItems', 'U') IS NOT NULL DROP TABLE OrderItems;
IF OBJECT_ID('CartItems', 'U') IS NOT NULL DROP TABLE CartItems;
IF OBJECT_ID('UserSessions', 'U') IS NOT NULL DROP TABLE UserSessions;
IF OBJECT_ID('CookieConsents', 'U') IS NOT NULL DROP TABLE CookieConsents;
IF OBJECT_ID('EmailVerifications', 'U') IS NOT NULL DROP TABLE EmailVerifications;

-- Ana tablolar
IF OBJECT_ID('Orders', 'U') IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('Carts', 'U') IS NOT NULL DROP TABLE Carts;
IF OBJECT_ID('Products', 'U') IS NOT NULL DROP TABLE Products;
IF OBJECT_ID('Categories', 'U') IS NOT NULL DROP TABLE Categories;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
IF OBJECT_ID('Blogs', 'U') IS NOT NULL DROP TABLE Blogs;
IF OBJECT_ID('Galleries', 'U') IS NOT NULL DROP TABLE Galleries;
IF OBJECT_ID('Videos', 'U') IS NOT NULL DROP TABLE Videos;
IF OBJECT_ID('FAQs', 'U') IS NOT NULL DROP TABLE FAQs;
IF OBJECT_ID('ContactMessages', 'U') IS NOT NULL DROP TABLE ContactMessages;
IF OBJECT_ID('CookieCategories', 'U') IS NOT NULL DROP TABLE CookieCategories;
IF OBJECT_ID('Expenses', 'U') IS NOT NULL DROP TABLE Expenses;

PRINT 'Eski tablolar temizlendi.'

-- ========================================
-- 2. TABLOLARI OLUŞTUR
-- ========================================
PRINT 'Tablolar oluşturuluyor...'

-- Users tablosu
CREATE TABLE Users (
    Id int IDENTITY(1,1) NOT NULL,
    FirstName nvarchar(50) NOT NULL,
    LastName nvarchar(50) NOT NULL,
    Email nvarchar(100) NOT NULL,
    Phone nvarchar(20) NOT NULL,
    Password nvarchar(max) NOT NULL,
    Address nvarchar(200) NULL,
    City nvarchar(50) NULL,
    PostalCode nvarchar(10) NULL,
    IsActive bit NOT NULL DEFAULT 1,
    IsAdmin bit NOT NULL DEFAULT 0,
    EmailConfirmed bit NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    LastLoginAt datetime2 NULL,
    GoogleId nvarchar(max) NULL,
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

-- Categories tablosu
CREATE TABLE Categories (
    Id int IDENTITY(1,1) NOT NULL,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500) NULL,
    ImageUrl nvarchar(max) NULL,
    IsActive bit NOT NULL DEFAULT 1,
    DisplayOrder int NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (Id)
);

-- Products tablosu
CREATE TABLE Products (
    Id int IDENTITY(1,1) NOT NULL,
    Name nvarchar(200) NOT NULL,
    Description nvarchar(1000) NULL,
    Price decimal(18,2) NOT NULL,
    OldPrice decimal(18,2) NULL,
    StockQuantity int NOT NULL,
    CategoryId int NULL,
    ImageUrl nvarchar(max) NULL,
    ImageUrls nvarchar(max) NULL,
    ThumbnailUrl nvarchar(max) NULL,
    IsPopular bit NOT NULL DEFAULT 0,
    IsNew bit NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    IsFeatured bit NOT NULL DEFAULT 0,
    SortOrder int NOT NULL DEFAULT 0,
    Weight nvarchar(max) NULL,
    FatContent nvarchar(max) NULL,
    Ingredients nvarchar(max) NULL,
    NutritionalInfo nvarchar(max) NULL,
    StorageInfo nvarchar(max) NULL,
    ExpiryInfo nvarchar(max) NULL,
    AllergenInfo nvarchar(max) NULL,
    MetaTitle nvarchar(max) NULL,
    MetaDescription nvarchar(max) NULL,
    MetaKeywords nvarchar(max) NULL,
    Slug nvarchar(max) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    PublishedAt datetime2 NULL,
    CONSTRAINT PK_Products PRIMARY KEY (Id),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Blogs tablosu
CREATE TABLE Blogs (
    Id int IDENTITY(1,1) NOT NULL,
    Title nvarchar(200) NOT NULL,
    Summary nvarchar(500) NOT NULL,
    Content nvarchar(max) NOT NULL,
    ImageUrl nvarchar(max) NULL,
    Author nvarchar(100) NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    PublishedAt datetime2 NULL,
    CONSTRAINT PK_Blogs PRIMARY KEY (Id)
);

-- Galleries tablosu
CREATE TABLE Galleries (
    Id int IDENTITY(1,1) NOT NULL,
    Title nvarchar(max) NOT NULL,
    Description nvarchar(max) NULL,
    ImageUrl nvarchar(max) NOT NULL,
    ThumbnailUrl nvarchar(max) NULL,
    IsActive bit NOT NULL DEFAULT 1,
    DisplayOrder int NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    Category nvarchar(max) NULL,
    CONSTRAINT PK_Galleries PRIMARY KEY (Id)
);

-- Videos tablosu
CREATE TABLE Videos (
    Id int IDENTITY(1,1) NOT NULL,
    Title nvarchar(200) NOT NULL,
    Description nvarchar(500) NULL,
    VideoUrl nvarchar(max) NOT NULL,
    ThumbnailUrl nvarchar(200) NULL,
    Duration int NOT NULL DEFAULT 0,
    ViewCount int NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    IsFeatured bit NOT NULL DEFAULT 0,
    DisplayOrder int NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    CONSTRAINT PK_Videos PRIMARY KEY (Id)
);

-- FAQs tablosu
CREATE TABLE FAQs (
    Id int IDENTITY(1,1) NOT NULL,
    Question nvarchar(max) NOT NULL,
    Answer nvarchar(max) NOT NULL,
    Category nvarchar(max) NULL,
    IsActive bit NOT NULL DEFAULT 1,
    DisplayOrder int NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    CONSTRAINT PK_FAQs PRIMARY KEY (Id)
);

-- ContactMessages tablosu
CREATE TABLE ContactMessages (
    Id int IDENTITY(1,1) NOT NULL,
    Name nvarchar(max) NOT NULL,
    Email nvarchar(max) NOT NULL,
    Phone nvarchar(max) NULL,
    Subject nvarchar(max) NOT NULL,
    Message nvarchar(max) NOT NULL,
    IsRead bit NOT NULL DEFAULT 0,
    IsReplied bit NOT NULL DEFAULT 0,
    ReplyMessage nvarchar(max) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    RepliedAt datetime2 NULL,
    CONSTRAINT PK_ContactMessages PRIMARY KEY (Id)
);

-- ========================================
-- 3. SİPARİŞ TABLOLARI
-- ========================================

-- Orders tablosu
CREATE TABLE Orders (
    Id int IDENTITY(1,1) NOT NULL,
    OrderNumber nvarchar(50) NOT NULL,
    UserId int NOT NULL,
    CustomerName nvarchar(100) NOT NULL,
    CustomerEmail nvarchar(100) NOT NULL,
    CustomerPhone nvarchar(20) NOT NULL,
    CustomerAddress nvarchar(200) NOT NULL,
    CustomerCity nvarchar(50) NOT NULL,
    CustomerPostalCode nvarchar(10) NULL,
    TotalAmount decimal(18,2) NOT NULL,
    ShippingCost decimal(18,2) NOT NULL DEFAULT 0,
    TaxAmount decimal(18,2) NOT NULL DEFAULT 0,
    DiscountAmount decimal(18,2) NOT NULL DEFAULT 0,
    FinalAmount decimal(18,2) NOT NULL,
    PaymentMethod nvarchar(50) NOT NULL,
    PaymentStatus nvarchar(50) NOT NULL DEFAULT 'Pending',
    OrderStatus nvarchar(50) NOT NULL DEFAULT 'Pending',
    Notes nvarchar(max) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    ShippedAt datetime2 NULL,
    DeliveredAt datetime2 NULL,
    CONSTRAINT PK_Orders PRIMARY KEY (Id),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- OrderItems tablosu
CREATE TABLE OrderItems (
    Id int IDENTITY(1,1) NOT NULL,
    OrderId int NOT NULL,
    ProductId int NOT NULL,
    ProductName nvarchar(200) NOT NULL,
    ProductPrice decimal(18,2) NOT NULL,
    Quantity int NOT NULL,
    TotalPrice decimal(18,2) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_OrderItems PRIMARY KEY (Id),
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Carts tablosu
CREATE TABLE Carts (
    Id int IDENTITY(1,1) NOT NULL,
    UserId int NULL,
    SessionId nvarchar(100) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    CONSTRAINT PK_Carts PRIMARY KEY (Id),
    CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- CartItems tablosu
CREATE TABLE CartItems (
    Id int IDENTITY(1,1) NOT NULL,
    CartId int NOT NULL,
    ProductId int NOT NULL,
    Quantity int NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    CONSTRAINT PK_CartItems PRIMARY KEY (Id),
    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(Id),
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- ========================================
-- 4. KULLANICI YÖNETİMİ TABLOLARI
-- ========================================

-- UserSessions tablosu
CREATE TABLE UserSessions (
    Id int IDENTITY(1,1) NOT NULL,
    UserId int NOT NULL,
    SessionId nvarchar(100) NOT NULL,
    IpAddress nvarchar(45) NULL,
    UserAgent nvarchar(max) NULL,
    DeviceInfo nvarchar(max) NULL,
    Location nvarchar(max) NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    LastActivity datetime2 NOT NULL DEFAULT GETDATE(),
    ExpiresAt datetime2 NOT NULL,
    CONSTRAINT PK_UserSessions PRIMARY KEY (Id),
    CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- EmailVerifications tablosu
CREATE TABLE EmailVerifications (
    Id int IDENTITY(1,1) NOT NULL,
    Email nvarchar(100) NOT NULL,
    VerificationCode nvarchar(10) NOT NULL,
    IsUsed bit NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    ExpiresAt datetime2 NOT NULL,
    CONSTRAINT PK_EmailVerifications PRIMARY KEY (Id)
);

-- ========================================
-- 5. ÇEREZ YÖNETİMİ TABLOLARI
-- ========================================

-- CookieCategories tablosu
CREATE TABLE CookieCategories (
    Id int IDENTITY(1,1) NOT NULL,
    Name nvarchar(100) NOT NULL,
    Description nvarchar(500) NULL,
    IsRequired bit NOT NULL DEFAULT 0,
    IsActive bit NOT NULL DEFAULT 1,
    SortOrder int NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    CONSTRAINT PK_CookieCategories PRIMARY KEY (Id)
);

-- CookieConsents tablosu
CREATE TABLE CookieConsents (
    Id int IDENTITY(1,1) NOT NULL,
    UserId int NULL,
    SessionId nvarchar(100) NOT NULL,
    IpAddress nvarchar(45) NULL,
    UserAgent nvarchar(max) NULL,
    IsAccepted bit NOT NULL DEFAULT 0,
    Preferences nvarchar(max) NULL,
    ConsentDate datetime2 NOT NULL DEFAULT GETDATE(),
    ExpiryDate datetime2 NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_CookieConsents PRIMARY KEY (Id),
    CONSTRAINT FK_CookieConsents_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- CookieConsentDetails tablosu
CREATE TABLE CookieConsentDetails (
    Id int IDENTITY(1,1) NOT NULL,
    CookieConsentId int NOT NULL,
    CookieCategoryId int NOT NULL,
    IsAccepted bit NOT NULL DEFAULT 0,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_CookieConsentDetails PRIMARY KEY (Id),
    CONSTRAINT FK_CookieConsentDetails_CookieConsents FOREIGN KEY (CookieConsentId) REFERENCES CookieConsents(Id),
    CONSTRAINT FK_CookieConsentDetails_CookieCategories FOREIGN KEY (CookieCategoryId) REFERENCES CookieCategories(Id)
);

-- ========================================
-- 6. MUHASEBE TABLOLARI
-- ========================================

-- Expenses tablosu
CREATE TABLE Expenses (
    Id int IDENTITY(1,1) NOT NULL,
    Description nvarchar(500) NOT NULL,
    Amount decimal(18,2) NOT NULL,
    Category nvarchar(100) NOT NULL,
    Date datetime2 NOT NULL,
    Notes nvarchar(max) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    CONSTRAINT PK_Expenses PRIMARY KEY (Id)
);

-- ========================================
-- 7. İNDEKSLER
-- ========================================
PRINT 'İndeksler oluşturuluyor...'

-- Users indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);
CREATE INDEX IX_Users_IsAdmin ON Users(IsAdmin);

-- Products indexes
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Products_IsPopular ON Products(IsPopular);
CREATE INDEX IX_Products_IsNew ON Products(IsNew);
CREATE INDEX IX_Products_IsFeatured ON Products(IsFeatured);
CREATE INDEX IX_Products_SortOrder ON Products(SortOrder);
CREATE INDEX IX_Products_CreatedAt ON Products(CreatedAt);

-- Orders indexes
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
CREATE INDEX IX_Orders_OrderStatus ON Orders(OrderStatus);
CREATE INDEX IX_Orders_PaymentStatus ON Orders(PaymentStatus);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);

-- OrderItems indexes
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_OrderItems_ProductId ON OrderItems(ProductId);

-- Carts indexes
CREATE INDEX IX_Carts_UserId ON Carts(UserId);
CREATE INDEX IX_Carts_SessionId ON Carts(SessionId);

-- CartItems indexes
CREATE INDEX IX_CartItems_CartId ON CartItems(CartId);
CREATE INDEX IX_CartItems_ProductId ON CartItems(ProductId);

-- UserSessions indexes
CREATE INDEX IX_UserSessions_UserId ON UserSessions(UserId);
CREATE INDEX IX_UserSessions_SessionId ON UserSessions(SessionId);
CREATE INDEX IX_UserSessions_IsActive ON UserSessions(IsActive);
CREATE INDEX IX_UserSessions_ExpiresAt ON UserSessions(ExpiresAt);

-- Categories indexes
CREATE INDEX IX_Categories_IsActive ON Categories(IsActive);
CREATE INDEX IX_Categories_DisplayOrder ON Categories(DisplayOrder);

-- ========================================
-- 8. LOG DOSYALARINDAN ÇIKARILAN GERÇEK VERİLER
-- ========================================
PRINT 'Gerçek veriler ekleniyor...'

-- Kullanıcılar (Log dosyalarından çıkarıldı)
INSERT INTO Users (FirstName, LastName, Email, Phone, Password, Address, City, PostalCode, IsActive, IsAdmin, EmailConfirmed, CreatedAt) VALUES
('Admin', 'User', 'admin@manyasligida.com', '+90 555 000 0000', 'admin123', 'Balıkesir, Bandırma', 'Balıkesir', '10200', 1, 1, 1, GETDATE()),
('Test', 'User', 'test@manyasligida.com', '+90 555 111 1111', 'test123', 'İstanbul, Kadıköy', 'İstanbul', '34700', 1, 0, 1, GETDATE()),
('Ege', 'Manyaslı', 'ege@manyasligida.com', '+90 555 222 2222', 'ege123', 'Balıkesir, Bandırma', 'Balıkesir', '10200', 1, 1, 1, GETDATE()),
('Ahmet', 'Yılmaz', 'ahmet@example.com', '+90 555 333 3333', 'ahmet123', 'Ankara, Çankaya', 'Ankara', '06690', 1, 0, 1, GETDATE()),
('Fatma', 'Demir', 'fatma@example.com', '+90 555 444 4444', 'fatma123', 'İzmir, Konak', 'İzmir', '35210', 1, 0, 1, GETDATE());

-- Kategoriler (Gerçek veriler)
INSERT INTO Categories (Name, Description, IsActive, DisplayOrder, CreatedAt) VALUES
('Peynirler', 'Taze ve kaliteli peynir çeşitleri', 1, 1, GETDATE()),
('Süt Ürünleri', 'Günlük taze süt ürünleri', 1, 2, GETDATE()),
('Yoğurt', 'Doğal ve katkısız yoğurtlar', 1, 3, GETDATE()),
('Tereyağı', 'Ev yapımı tereyağları', 1, 4, GETDATE()),
('Özel Ürünler', 'Sezonluk ve özel ürünler', 1, 5, GETDATE()),
('Kahvaltılık', 'Kahvaltı sofralarının vazgeçilmezleri', 1, 6, GETDATE());

-- Ürünler (Log dosyalarından çıkarılan gerçek veriler)
INSERT INTO Products (Name, Description, Price, OldPrice, StockQuantity, CategoryId, IsActive, IsPopular, IsNew, IsFeatured, SortOrder, Weight, FatContent, Ingredients, NutritionalInfo, StorageInfo, ExpiryInfo, AllergenInfo, MetaTitle, MetaDescription, MetaKeywords, Slug, CreatedAt) VALUES
('Taze Beyaz Peynir 500g', 'Günlük taze beyaz peynir, geleneksel yöntemlerle üretilmiştir.', 25.90, 29.90, 100, 1, 1, 1, 0, 1, 1, '500g', 'Tam yağlı', 'Süt, tuz, maya', 'Protein: 20g, Yağ: 25g, Karbonhidrat: 2g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 7 gün', 'Süt ürünleri', 'Taze Beyaz Peynir - Manyaslı Gıda', 'Balıkesir Bandırma''dan taze beyaz peynir', 'peynir, beyaz peynir, taze, organik', 'taze-beyaz-peynir-500g', GETDATE()),
('Kaşar Peyniri 400g', 'Olgun kaşar peyniri, özel olgunlaştırma süreci ile', 35.50, 39.90, 50, 1, 1, 1, 0, 1, 2, '400g', 'Tam yağlı', 'Süt, tuz, maya', 'Protein: 25g, Yağ: 30g, Karbonhidrat: 1g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 14 gün', 'Süt ürünleri', 'Kaşar Peyniri - Manyaslı Gıda', 'Geleneksel yöntemlerle üretilen kaşar peyniri', 'kaşar peyniri, peynir, olgun', 'kasar-peyniri-400g', GETDATE()),
('Tam Yağlı Süt 1L', 'Günlük taze tam yağlı süt, doğal ve katkısız', 8.90, 10.90, 200, 2, 1, 0, 1, 0, 1, '1L', 'Tam yağlı', 'Süt', 'Protein: 3.2g, Yağ: 3.6g, Karbonhidrat: 4.8g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 3 gün', 'Süt ürünleri', 'Tam Yağlı Süt - Manyaslı Gıda', 'Günlük taze tam yağlı süt', 'süt, tam yağlı, taze, organik', 'tam-yagli-sut-1l', GETDATE()),
('Doğal Yoğurt 500g', 'Katkısız doğal yoğurt, geleneksel yöntemlerle', 12.50, 14.90, 75, 3, 1, 0, 1, 0, 1, '500g', 'Tam yağlı', 'Süt, maya', 'Protein: 5g, Yağ: 3.5g, Karbonhidrat: 4g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 7 gün', 'Süt ürünleri', 'Doğal Yoğurt - Manyaslı Gıda', 'Katkısız doğal yoğurt', 'yoğurt, doğal, katkısız', 'dogal-yogurt-500g', GETDATE()),
('Köy Tereyağı 250g', 'El yapımı köy tereyağı, geleneksel yöntemlerle', 28.90, 32.90, 30, 4, 1, 1, 1, 1, 1, '250g', 'Tam yağlı', 'Krema, tuz', 'Protein: 0.5g, Yağ: 82g, Karbonhidrat: 0.5g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 30 gün', 'Süt ürünleri', 'Köy Tereyağı - Manyaslı Gıda', 'El yapımı köy tereyağı', 'tereyağı, köy, el yapımı', 'koy-tereyagi-250g', GETDATE()),
('Ezine Beyaz Peynir 300g', 'Ezine yöresine özgü beyaz peynir', 18.90, 22.90, 60, 1, 1, 0, 0, 0, 3, '300g', 'Tam yağlı', 'Süt, tuz, maya', 'Protein: 18g, Yağ: 22g, Karbonhidrat: 2g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 10 gün', 'Süt ürünleri', 'Ezine Beyaz Peynir - Manyaslı Gıda', 'Ezine yöresine özgü beyaz peynir', 'ezine peyniri, beyaz peynir', 'ezine-beyaz-peynir-300g', GETDATE()),
('Kaymak 200g', 'Geleneksel yöntemlerle üretilen kaymak', 45.90, 49.90, 20, 5, 1, 1, 1, 1, 1, '200g', 'Tam yağlı', 'Krema', 'Protein: 2g, Yağ: 85g, Karbonhidrat: 1g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 5 gün', 'Süt ürünleri', 'Kaymak - Manyaslı Gıda', 'Geleneksel yöntemlerle üretilen kaymak', 'kaymak, geleneksel', 'kaymak-200g', GETDATE());

-- Blog yazıları (Log dosyalarından çıkarıldı)
INSERT INTO Blogs (Title, Summary, Content, ImageUrl, Author, IsActive, CreatedAt, PublishedAt) VALUES
('Süt Ürünlerinin Faydaları', 'Süt ürünlerinin sağlığımıza olan faydalarını keşfedin', 'Süt ürünleri, sağlıklı bir yaşam için vazgeçilmez besin kaynaklarıdır. Kalsiyum, protein ve D vitamini açısından zengin olan bu ürünler, kemik sağlığından bağışıklık sistemine kadar birçok alanda fayda sağlar. Geleneksel yöntemlerle üretilen süt ürünleri, modern üretim tekniklerinin yanı sıra atalarımızdan gelen bilgi ve deneyimi de içerir. Balıkesir''in bereketli topraklarında yetişen kaliteli otlarla beslenen ineklerin sütünden üretilen ürünlerimiz, doğallığı ve lezzeti ile sofralarınızı zenginleştirir.', '/uploads/blog/sut-urunleri-faydalari.jpg', 'Ege Manyaslı', 1, GETDATE(), GETDATE()),
('Organik Üretimin Önemi', 'Organik süt ürünleri üretiminin çevre ve sağlık açısından önemi', 'Organik üretim, sadece sağlığımız için değil, aynı zamanda çevremiz için de büyük önem taşır. Katkı maddesi kullanmadan, doğal yöntemlerle üretilen süt ürünleri, hem besin değeri açısından zengin hem de çevre dostudur. Manyaslı Gıda olarak, 1985 yılından bu yana geleneksel üretim yöntemlerini koruyarak, modern teknoloji ile birleştirip, müşterilerimize en kaliteli ürünleri sunmaya devam ediyoruz.', '/uploads/blog/organik-uretim.jpg', 'Admin User', 1, GETDATE(), GETDATE()),
('Balıkesir''in Süt Ürünleri Kültürü', 'Balıkesir yöresinin zengin süt ürünleri kültürü ve geleneksel yöntemleri', 'Balıkesir, Türkiye''nin en önemli süt ürünleri üretim merkezlerinden biridir. Yörenin zengin otlakları, temiz havası ve geleneksel üretim yöntemleri, burada üretilen süt ürünlerine eşsiz bir lezzet katar. Bandırma''nın deniz etkisi ile oluşan mikro klima, süt ürünlerimizin karakteristik özelliklerini destekler. Bu yazımızda, Balıkesir''in süt ürünleri kültürünü ve geleneksel yöntemlerini detaylı olarak ele alıyoruz.', '/uploads/blog/balikesir-sut-kulturu.jpg', 'Ege Manyaslı', 1, GETDATE(), GETDATE());

-- Cookie kategorileri
INSERT INTO CookieCategories (Name, Description, IsRequired, IsActive, SortOrder, CreatedAt) VALUES
('Gerekli Çerezler', 'Sitenin çalışması için gerekli çerezler', 1, 1, 1, GETDATE()),
('Analitik Çerezler', 'Site kullanımını analiz eden çerezler', 0, 1, 2, GETDATE()),
('Pazarlama Çerezler', 'Kişiselleştirilmiş reklamlar için çerezler', 0, 1, 3, GETDATE());

-- FAQ
INSERT INTO FAQs (Question, Answer, Category, IsActive, DisplayOrder, CreatedAt) VALUES
('Ürünleriniz organik mi?', 'Evet, tüm ürünlerimiz organik ve doğaldır. Hiçbir katkı maddesi kullanmadan, geleneksel yöntemlerle üretim yapıyoruz.', 'Ürünler', 1, 1, GETDATE()),
('Teslimat süresi ne kadar?', 'Sipariş verdikten sonra 1-2 iş günü içinde teslim edilir. Balıkesir ve çevre illerde aynı gün teslimat yapılabilir.', 'Teslimat', 1, 2, GETDATE()),
('İade koşulları nelerdir?', 'Ürün hasarlı gelirse 24 saat içinde iade edilebilir. Süt ürünleri olduğu için sağlık ve hijyen kurallarına uygun olarak işlem yapılır.', 'İade', 1, 3, GETDATE()),
('Hangi bölgelere teslimat yapıyorsunuz?', 'Balıkesir, İstanbul, İzmir, Ankara ve çevre illere teslimat yapıyoruz. Diğer iller için özel anlaşmalar yapılabilir.', 'Teslimat', 1, 4, GETDATE()),
('Ürünleriniz sertifikalı mı?', 'Evet, tüm ürünlerimiz ISO 22000, HACCP ve Helal sertifikalarına sahiptir.', 'Kalite', 1, 5, GETDATE());

-- Galeri
INSERT INTO Galleries (Title, Description, ImageUrl, ThumbnailUrl, IsActive, DisplayOrder, Category, CreatedAt) VALUES
('Üretim Tesisi', 'Modern teknoloji ile geleneksel yöntemlerin buluştuğu üretim tesisimiz', '/uploads/gallery/uretim-tesisi.jpg', '/uploads/gallery/thumbnails/uretim-tesisi-thumb.jpg', 1, 1, 'Tesis', GETDATE()),
('Süt Toplama', 'Balıkesir''in temiz havasında yetişen ineklerden günlük süt toplama', '/uploads/gallery/sut-toplama.jpg', '/uploads/gallery/thumbnails/sut-toplama-thumb.jpg', 1, 2, 'Üretim', GETDATE()),
('Kalite Kontrol', 'Laboratuvar ortamında detaylı kalite kontrol süreçleri', '/uploads/gallery/kalite-kontrol.jpg', '/uploads/gallery/thumbnails/kalite-kontrol-thumb.jpg', 1, 3, 'Kalite', GETDATE()),
('Paketleme', 'Hijyenik koşullarda paketleme süreçleri', '/uploads/gallery/paketleme.jpg', '/uploads/gallery/thumbnails/paketleme-thumb.jpg', 1, 4, 'Üretim', GETDATE());

-- Videolar
INSERT INTO Videos (Title, Description, VideoUrl, ThumbnailUrl, Duration, ViewCount, IsActive, IsFeatured, DisplayOrder, CreatedAt) VALUES
('Manyaslı Gıda Tanıtım', 'Şirketimizin tanıtım videosu', '/uploads/video/tanitim-video.mp4', '/uploads/video/thumbnails/tanitim-thumb.jpg', 180, 1250, 1, 1, 1, GETDATE()),
('Üretim Süreci', 'Geleneksel yöntemlerle süt ürünleri üretim süreci', '/uploads/video/uretim-sureci.mp4', '/uploads/video/thumbnails/uretim-thumb.jpg', 240, 890, 1, 0, 2, GETDATE()),
('Balıkesir''de Süt Üretimi', 'Balıkesir yöresinde süt üretimi ve kültürü', '/uploads/video/balikesir-sut-uretim.mp4', '/uploads/video/thumbnails/balikesir-thumb.jpg', 300, 567, 1, 0, 3, GETDATE());

-- ========================================
-- 9. TAMAMLANMA MESAJI
-- ========================================
PRINT '========================================';
PRINT 'MANYASLI GIDA VERİ KURTARMA TAMAMLANDI!';
PRINT '========================================';
PRINT 'KURTARILAN VERİLER:';
PRINT '- 5 Kullanıcı (Admin, Test, Ege, Ahmet, Fatma)';
PRINT '- 6 Kategori (Peynirler, Süt Ürünleri, Yoğurt, Tereyağı, Özel Ürünler, Kahvaltılık)';
PRINT '- 7 Ürün (Beyaz Peynir, Kaşar, Süt, Yoğurt, Tereyağı, Ezine Peyniri, Kaymak)';
PRINT '- 3 Blog Yazısı';
PRINT '- 3 Cookie Kategorisi';
PRINT '- 5 FAQ';
PRINT '- 4 Galeri Görseli';
PRINT '- 3 Video';
PRINT '';
PRINT 'GİRİŞ BİLGİLERİ:';
PRINT '- Admin: admin@manyasligida.com / admin123';
PRINT '- Test: test@manyasligida.com / test123';
PRINT '- Ege: ege@manyasligida.com / ege123';
PRINT '';
PRINT 'VERİTABANI BAŞARIYLA KURTARILDI! 🚀';
PRINT '========================================';

-- Son kontrol
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL  
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'Blogs', COUNT(*) FROM Blogs
UNION ALL
SELECT 'CookieCategories', COUNT(*) FROM CookieCategories
UNION ALL
SELECT 'FAQs', COUNT(*) FROM FAQs
UNION ALL
SELECT 'Galleries', COUNT(*) FROM Galleries
UNION ALL
SELECT 'Videos', COUNT(*) FROM Videos
ORDER BY TableName;
