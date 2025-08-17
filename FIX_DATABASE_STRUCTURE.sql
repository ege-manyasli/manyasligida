-- ========================================
-- VERİTABANI YAPISINI DÜZELT
-- ========================================
-- Bu script eksik sütunları ekler

PRINT 'Veritabanı yapısı düzeltiliyor...'

-- Users tablosuna eksik sütunları ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'City')
BEGIN
    ALTER TABLE Users ADD City nvarchar(50) NULL;
    PRINT 'Users tablosuna City sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'PostalCode')
BEGIN
    ALTER TABLE Users ADD PostalCode nvarchar(10) NULL;
    PRINT 'Users tablosuna PostalCode sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'EmailConfirmed')
BEGIN
    ALTER TABLE Users ADD EmailConfirmed bit NOT NULL DEFAULT 0;
    PRINT 'Users tablosuna EmailConfirmed sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Address')
BEGIN
    ALTER TABLE Users ADD Address nvarchar(200) NULL;
    PRINT 'Users tablosuna Address sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'LastLoginAt')
BEGIN
    ALTER TABLE Users ADD LastLoginAt datetime2 NULL;
    PRINT 'Users tablosuna LastLoginAt sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'GoogleId')
BEGIN
    ALTER TABLE Users ADD GoogleId nvarchar(max) NULL;
    PRINT 'Users tablosuna GoogleId sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE Users ADD UpdatedAt datetime2 NULL;
    PRINT 'Users tablosuna UpdatedAt sütunu eklendi.'
END

-- Categories tablosuna eksik sütunları ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Categories' AND COLUMN_NAME = 'DisplayOrder')
BEGIN
    ALTER TABLE Categories ADD DisplayOrder int NOT NULL DEFAULT 0;
    PRINT 'Categories tablosuna DisplayOrder sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Categories' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE Categories ADD UpdatedAt datetime2 NULL;
    PRINT 'Categories tablosuna UpdatedAt sütunu eklendi.'
END

-- Products tablosuna eksik sütunları ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'OldPrice')
BEGIN
    ALTER TABLE Products ADD OldPrice decimal(18,2) NULL;
    PRINT 'Products tablosuna OldPrice sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ImageUrls')
BEGIN
    ALTER TABLE Products ADD ImageUrls nvarchar(max) NULL;
    PRINT 'Products tablosuna ImageUrls sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ThumbnailUrl')
BEGIN
    ALTER TABLE Products ADD ThumbnailUrl nvarchar(max) NULL;
    PRINT 'Products tablosuna ThumbnailUrl sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'IsFeatured')
BEGIN
    ALTER TABLE Products ADD IsFeatured bit NOT NULL DEFAULT 0;
    PRINT 'Products tablosuna IsFeatured sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'IsNew')
BEGIN
    ALTER TABLE Products ADD IsNew bit NOT NULL DEFAULT 0;
    PRINT 'Products tablosuna IsNew sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'IsPopular')
BEGIN
    ALTER TABLE Products ADD IsPopular bit NOT NULL DEFAULT 0;
    PRINT 'Products tablosuna IsPopular sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'SortOrder')
BEGIN
    ALTER TABLE Products ADD SortOrder int NOT NULL DEFAULT 0;
    PRINT 'Products tablosuna SortOrder sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Weight')
BEGIN
    ALTER TABLE Products ADD Weight nvarchar(max) NULL;
    PRINT 'Products tablosuna Weight sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'FatContent')
BEGIN
    ALTER TABLE Products ADD FatContent nvarchar(max) NULL;
    PRINT 'Products tablosuna FatContent sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Ingredients')
BEGIN
    ALTER TABLE Products ADD Ingredients nvarchar(max) NULL;
    PRINT 'Products tablosuna Ingredients sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'NutritionalInfo')
BEGIN
    ALTER TABLE Products ADD NutritionalInfo nvarchar(max) NULL;
    PRINT 'Products tablosuna NutritionalInfo sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'StorageInfo')
BEGIN
    ALTER TABLE Products ADD StorageInfo nvarchar(max) NULL;
    PRINT 'Products tablosuna StorageInfo sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ExpiryInfo')
BEGIN
    ALTER TABLE Products ADD ExpiryInfo nvarchar(max) NULL;
    PRINT 'Products tablosuna ExpiryInfo sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'AllergenInfo')
BEGIN
    ALTER TABLE Products ADD AllergenInfo nvarchar(max) NULL;
    PRINT 'Products tablosuna AllergenInfo sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'MetaTitle')
BEGIN
    ALTER TABLE Products ADD MetaTitle nvarchar(max) NULL;
    PRINT 'Products tablosuna MetaTitle sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'MetaDescription')
BEGIN
    ALTER TABLE Products ADD MetaDescription nvarchar(max) NULL;
    PRINT 'Products tablosuna MetaDescription sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'MetaKeywords')
BEGIN
    ALTER TABLE Products ADD MetaKeywords nvarchar(max) NULL;
    PRINT 'Products tablosuna MetaKeywords sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Slug')
BEGIN
    ALTER TABLE Products ADD Slug nvarchar(max) NULL;
    PRINT 'Products tablosuna Slug sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'PublishedAt')
BEGIN
    ALTER TABLE Products ADD PublishedAt datetime2 NULL;
    PRINT 'Products tablosuna PublishedAt sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE Products ADD UpdatedAt datetime2 NULL;
    PRINT 'Products tablosuna UpdatedAt sütunu eklendi.'
END

-- Blogs tablosuna eksik sütunları ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Blogs' AND COLUMN_NAME = 'PublishedAt')
BEGIN
    ALTER TABLE Blogs ADD PublishedAt datetime2 NULL;
    PRINT 'Blogs tablosuna PublishedAt sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Blogs' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE Blogs ADD UpdatedAt datetime2 NULL;
    PRINT 'Blogs tablosuna UpdatedAt sütunu eklendi.'
END

-- CookieCategories tablosuna eksik sütunları ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CookieCategories' AND COLUMN_NAME = 'SortOrder')
BEGIN
    ALTER TABLE CookieCategories ADD SortOrder int NOT NULL DEFAULT 0;
    PRINT 'CookieCategories tablosuna SortOrder sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CookieCategories' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE CookieCategories ADD UpdatedAt datetime2 NULL;
    PRINT 'CookieCategories tablosuna UpdatedAt sütunu eklendi.'
END

-- FAQs tablosuna eksik sütunları ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FAQs' AND COLUMN_NAME = 'DisplayOrder')
BEGIN
    ALTER TABLE FAQs ADD DisplayOrder int NOT NULL DEFAULT 0;
    PRINT 'FAQs tablosuna DisplayOrder sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'FAQs' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE FAQs ADD UpdatedAt datetime2 NULL;
    PRINT 'FAQs tablosuna UpdatedAt sütunu eklendi.'
END

-- Galleries tablosuna eksik sütunları ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Galleries' AND COLUMN_NAME = 'DisplayOrder')
BEGIN
    ALTER TABLE Galleries ADD DisplayOrder int NOT NULL DEFAULT 0;
    PRINT 'Galleries tablosuna DisplayOrder sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Galleries' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE Galleries ADD UpdatedAt datetime2 NULL;
    PRINT 'Galleries tablosuna UpdatedAt sütunu eklendi.'
END

-- Videos tablosuna eksik sütunları ekle
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'DisplayOrder')
BEGIN
    ALTER TABLE Videos ADD DisplayOrder int NOT NULL DEFAULT 0;
    PRINT 'Videos tablosuna DisplayOrder sütunu eklendi.'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Videos' AND COLUMN_NAME = 'UpdatedAt')
BEGIN
    ALTER TABLE Videos ADD UpdatedAt datetime2 NULL;
    PRINT 'Videos tablosuna UpdatedAt sütunu eklendi.'
END

PRINT 'Veritabanı yapısı düzeltme tamamlandı!'
