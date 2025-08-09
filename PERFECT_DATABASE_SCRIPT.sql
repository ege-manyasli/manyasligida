-- ========================================
-- MANYASLI GIDA - PERFECT DATABASE SCRIPT
-- ========================================
-- Bu script tüm tabloları siler ve model'lara göre yeniden oluşturur
-- AZURE SQL DATABASE'DE ÇALIŞTIRILACAK

-- WARNING: TÜM VERİLER SİLİNECEK!
PRINT 'MANYASLI GIDA DATABASE RESET BAŞLIYOR...'

-- ========================================
-- 1. MEVCUT TABLOLARI SİL (Foreign Key sırası önemli)
-- ========================================
PRINT 'Mevcut tablolar siliniyor...'

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

PRINT 'Eski tablolar silindi.'

-- ========================================
-- 2. ANA TABLOLARI OLUŞTUR
-- ========================================
PRINT 'Yeni tablolar oluşturuluyor...'

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
    ShippingAddress nvarchar(500) NOT NULL,
    City nvarchar(50) NULL,
    PostalCode nvarchar(10) NULL,
    SubTotal decimal(18,2) NOT NULL,
    DiscountAmount decimal(18,2) NOT NULL,
    ShippingCost decimal(18,2) NOT NULL,
    TaxAmount decimal(18,2) NOT NULL,
    TotalAmount decimal(18,2) NOT NULL,
    OrderStatus nvarchar(50) NOT NULL DEFAULT 'Pending',
    PaymentStatus nvarchar(50) NOT NULL DEFAULT 'Pending',
    Notes nvarchar(1000) NULL,
    OrderDate datetime2 NOT NULL DEFAULT GETDATE(),
    ShippedDate datetime2 NULL,
    DeliveredDate datetime2 NULL,
    CONSTRAINT PK_Orders PRIMARY KEY (Id),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UQ_Orders_OrderNumber UNIQUE (OrderNumber)
);

-- OrderItems tablosu
CREATE TABLE OrderItems (
    Id int IDENTITY(1,1) NOT NULL,
    OrderId int NOT NULL,
    ProductId int NOT NULL,
    ProductName nvarchar(max) NOT NULL,
    Quantity int NOT NULL,
    UnitPrice decimal(18,2) NOT NULL,
    TotalPrice decimal(18,2) NOT NULL,
    CONSTRAINT PK_OrderItems PRIMARY KEY (Id),
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- ========================================
-- 4. SEPET TABLOLARI
-- ========================================

-- Carts tablosu
CREATE TABLE Carts (
    Id int IDENTITY(1,1) NOT NULL,
    UserId int NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt datetime2 NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CONSTRAINT PK_Carts PRIMARY KEY (Id),
    CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- CartItems tablosu
CREATE TABLE CartItems (
    Id int IDENTITY(1,1) NOT NULL,
    CartId int NULL,
    ProductId int NOT NULL,
    Quantity int NOT NULL,
    UnitPrice decimal(18,2) NOT NULL,
    TotalPrice decimal(18,2) NOT NULL,
    AddedAt datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_CartItems PRIMARY KEY (Id),
    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- ========================================
-- 5. OTURUM VE GÜVENLİK TABLOLARI
-- ========================================

-- UserSessions tablosu
CREATE TABLE UserSessions (
    Id int IDENTITY(1,1) NOT NULL,
    SessionId nvarchar(50) NOT NULL,
    UserId int NOT NULL,
    IpAddress nvarchar(45) NULL,
    UserAgent nvarchar(500) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    LastActivity datetime2 NOT NULL DEFAULT GETUTCDATE(),
    ExpiresAt datetime2 NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    DeviceInfo nvarchar(100) NULL,
    Location nvarchar(50) NULL,
    CONSTRAINT PK_UserSessions PRIMARY KEY (Id),
    CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UQ_UserSessions_SessionId UNIQUE (SessionId)
);

-- EmailVerifications tablosu
CREATE TABLE EmailVerifications (
    Id int IDENTITY(1,1) NOT NULL,
    Email nvarchar(max) NOT NULL,
    VerificationCode nvarchar(max) NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    ExpiresAt datetime2 NOT NULL,
    IsUsed bit NOT NULL DEFAULT 0,
    CONSTRAINT PK_EmailVerifications PRIMARY KEY (Id)
);

-- ========================================
-- 6. COOKIE CONSENT TABLOLARI
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
    CONSTRAINT PK_CookieCategories PRIMARY KEY (Id)
);

-- CookieConsents tablosu
CREATE TABLE CookieConsents (
    Id int IDENTITY(1,1) NOT NULL,
    SessionId nvarchar(50) NULL,
    UserId int NULL,
    IpAddress nvarchar(45) NULL,
    UserAgent nvarchar(500) NULL,
    ConsentDate datetime2 NOT NULL DEFAULT GETDATE(),
    ExpiryDate datetime2 NOT NULL,
    IsActive bit NOT NULL DEFAULT 1,
    IsAccepted bit NOT NULL,
    Preferences nvarchar(1000) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_CookieConsents PRIMARY KEY (Id),
    CONSTRAINT FK_CookieConsents_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- CookieConsentDetails tablosu
CREATE TABLE CookieConsentDetails (
    Id int IDENTITY(1,1) NOT NULL,
    CookieConsentId int NOT NULL,
    CookieCategoryId int NOT NULL,
    IsAccepted bit NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_CookieConsentDetails PRIMARY KEY (Id),
    CONSTRAINT FK_CookieConsentDetails_CookieConsents FOREIGN KEY (CookieConsentId) REFERENCES CookieConsents(Id) ON DELETE CASCADE,
    CONSTRAINT FK_CookieConsentDetails_CookieCategories FOREIGN KEY (CookieCategoryId) REFERENCES CookieCategories(Id)
);

-- ========================================
-- 7. MUHASEBE TABLOLARI
-- ========================================

-- Expenses tablosu
CREATE TABLE Expenses (
    Id int IDENTITY(1,1) NOT NULL,
    Description nvarchar(200) NOT NULL,
    Amount decimal(18,2) NOT NULL,
    Category nvarchar(100) NOT NULL,
    Date datetime2 NOT NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt datetime2 NULL,
    IsActive bit NOT NULL DEFAULT 1,
    CONSTRAINT PK_Expenses PRIMARY KEY (Id)
);

-- ========================================
-- 8. INDEX'LER VE PERFORMANS İYİLEŞTİRMELERİ
-- ========================================
PRINT 'Index''ler oluşturuluyor...'

-- Users indexes
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);

-- Products indexes
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Products_IsPopular ON Products(IsPopular);
CREATE INDEX IX_Products_IsNew ON Products(IsNew);
CREATE INDEX IX_Products_IsFeatured ON Products(IsFeatured);

-- Orders indexes
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_OrderStatus ON Orders(OrderStatus);
CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);

-- UserSessions indexes
CREATE INDEX IX_UserSessions_UserId ON UserSessions(UserId);
CREATE INDEX IX_UserSessions_SessionId ON UserSessions(SessionId);
CREATE INDEX IX_UserSessions_IsActive ON UserSessions(IsActive);
CREATE INDEX IX_UserSessions_ExpiresAt ON UserSessions(ExpiresAt);

-- Categories indexes
CREATE INDEX IX_Categories_IsActive ON Categories(IsActive);
CREATE INDEX IX_Categories_DisplayOrder ON Categories(DisplayOrder);

-- ========================================
-- 9. BAŞLANGIÇ VERİLERİ (SEED DATA)
-- ========================================
PRINT 'Başlangıç verileri ekleniyor...'

-- Admin kullanıcı
INSERT INTO Users (FirstName, LastName, Email, Phone, Password, IsActive, IsAdmin, EmailConfirmed, CreatedAt)
VALUES ('Admin', 'User', 'admin@manyasligida.com', '+90 555 000 0000', 'admin123', 1, 1, 1, GETDATE());

-- Test kullanıcısı (şifre: test123)
INSERT INTO Users (FirstName, LastName, Email, Phone, Password, IsActive, IsAdmin, EmailConfirmed, CreatedAt)
VALUES ('Test', 'User', 'test@manyasligida.com', '+90 555 111 1111', 'test123', 1, 0, 1, GETDATE());

-- Ege Manyaslı (şifre: ege123)
INSERT INTO Users (FirstName, LastName, Email, Phone, Password, IsActive, IsAdmin, EmailConfirmed, CreatedAt)
VALUES ('Ege', 'Manyaslı', 'ege@manyasligida.com', '+90 555 222 2222', 'ege123', 1, 1, 1, GETDATE());

-- Kategoriler
INSERT INTO Categories (Name, Description, IsActive, DisplayOrder, CreatedAt) VALUES
('Peynirler', 'Taze ve kaliteli peynir çeşitleri', 1, 1, GETDATE()),
('Süt Ürünleri', 'Günlük taze süt ürünleri', 1, 2, GETDATE()),
('Yoğurt', 'Doğal ve katkısız yoğurtlar', 1, 3, GETDATE()),
('Tereyağı', 'Ev yapımı tereyağları', 1, 4, GETDATE()),
('Özel Ürünler', 'Sezonluk ve özel ürünler', 1, 5, GETDATE());

-- Örnek ürünler
INSERT INTO Products (Name, Description, Price, StockQuantity, CategoryId, IsActive, IsPopular, IsNew, CreatedAt) VALUES
('Taze Beyaz Peynir 500g', 'Günlük taze beyaz peynir', 25.90, 100, 1, 1, 1, 0, GETDATE()),
('Kaşar Peyniri 400g', 'Olgun kaşar peyniri', 35.50, 50, 1, 1, 1, 0, GETDATE()),
('Tam Yağlı Süt 1L', 'Günlük taze tam yağlı süt', 8.90, 200, 2, 1, 0, 1, GETDATE()),
('Doğal Yoğurt 500g', 'Katkısız doğal yoğurt', 12.50, 75, 3, 1, 0, 1, GETDATE()),
('Köy Tereyağı 250g', 'El yapımı köy tereyağı', 28.90, 30, 4, 1, 1, 1, GETDATE());

-- Cookie kategorileri
INSERT INTO CookieCategories (Name, Description, IsRequired, IsActive, SortOrder, CreatedAt) VALUES
('Gerekli Çerezler', 'Sitenin çalışması için gerekli çerezler', 1, 1, 1, GETDATE()),
('Analitik Çerezler', 'Site kullanımını analiz eden çerezler', 0, 1, 2, GETDATE()),
('Pazarlama Çerezler', 'Kişiselleştirilmiş reklamlar için çerezler', 0, 1, 3, GETDATE());

-- FAQ
INSERT INTO FAQs (Question, Answer, Category, IsActive, DisplayOrder, CreatedAt) VALUES
('Ürünleriniz organik mi?', 'Evet, tüm ürünlerimiz organik ve doğaldır.', 'Ürünler', 1, 1, GETDATE()),
('Teslimat süresi ne kadar?', 'Sipariş verdikten sonra 1-2 iş günü içinde teslim edilir.', 'Teslimat', 1, 2, GETDATE()),
('İade koşulları nelerdir?', 'Ürün hasarlı gelirse 24 saat içinde iade edilebilir.', 'İade', 1, 3, GETDATE());

-- ========================================
-- 10. TAMAMLANMA MESAJI
-- ========================================
PRINT '========================================';
PRINT 'MANYASLI GIDA DATABASE BAŞARIYLA OLUŞTURULDU!';
PRINT '========================================';
PRINT 'OLUŞTURULAN TABLOLAR:';
PRINT '- Users (Kullanıcılar)';
PRINT '- Categories (Kategoriler)'; 
PRINT '- Products (Ürünler)';
PRINT '- Orders & OrderItems (Siparişler)';
PRINT '- Carts & CartItems (Sepetler)';
PRINT '- UserSessions (Oturumlar)';
PRINT '- EmailVerifications (E-posta Doğrulama)';
PRINT '- CookieConsents & CookieConsentDetails (Çerez Onayları)';
PRINT '- CookieCategories (Çerez Kategorileri)';
PRINT '- Blogs (Blog Yazıları)';
PRINT '- Galleries (Galeri)';
PRINT '- Videos (Videolar)';
PRINT '- FAQs (Sık Sorulan Sorular)';
PRINT '- ContactMessages (İletişim Mesajları)';
PRINT '- Expenses (Giderler)';
PRINT '';
PRINT 'BAŞLANGIÇ VERİLERİ:';
PRINT '- Admin: admin@manyasligida.com / admin123';
PRINT '- Test: test@manyasligida.com / test123';
PRINT '- Ege: ege@manyasligida.com / ege123';
PRINT '- 5 Kategori, 5 Ürün, 3 FAQ eklendi';
PRINT '';
PRINT 'SİTE ŞİMDİ ÇALIŞMAYA HAZIR! 🚀';
PRINT '========================================';

-- Son kontrol
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL  
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'CookieCategories', COUNT(*) FROM CookieCategories
UNION ALL
SELECT 'FAQs', COUNT(*) FROM FAQs
ORDER BY TableName;
