-- ===================================================================
-- MANYASLI GIDA - HOME CONTENTS TABLE CREATION SCRIPT
-- Bu script HomeContents tablosunu Azure SQL Database'e ekler
-- ===================================================================

USE [manyasligida];
GO

-- HomeContents tablosunu oluştur
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='HomeContents' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[HomeContents] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        
        -- HERO SECTION
        [HeroTitle] NVARCHAR(200) NOT NULL,
        [HeroSubtitle] NVARCHAR(500) NULL,
        [HeroDescription] NVARCHAR(MAX) NULL,
        [HeroVideoUrl] NVARCHAR(500) NULL,
        [HeroImageUrl] NVARCHAR(500) NULL,
        [HeroButtonText] NVARCHAR(100) NULL,
        [HeroSecondButtonText] NVARCHAR(100) NULL,
        
        -- FEATURES SECTION
        [FeaturesTitle] NVARCHAR(200) NULL,
        [FeaturesSubtitle] NVARCHAR(500) NULL,
        [FeatureItems] NVARCHAR(MAX) NULL, -- JSON: [{"title":"", "description":"", "icon":"", "color":""}]
        
        -- POPULAR PRODUCTS SECTION
        [ProductsTitle] NVARCHAR(200) NULL,
        [ProductsSubtitle] NVARCHAR(500) NULL,
        [ShowPopularProducts] BIT NOT NULL DEFAULT 1,
        [MaxProductsToShow] INT NOT NULL DEFAULT 8,
        
        -- ABOUT SECTION
        [AboutTitle] NVARCHAR(200) NULL,
        [AboutContent] NVARCHAR(MAX) NULL,
        [AboutImageUrl] NVARCHAR(500) NULL,
        [AboutButtonText] NVARCHAR(100) NULL,
        [AboutFeatures] NVARCHAR(MAX) NULL, -- JSON: [{"title":"", "value":""}]
        
        -- STATS SECTION
        [StatsTitle] NVARCHAR(200) NULL,
        [StatsSubtitle] NVARCHAR(500) NULL,
        [StatsItems] NVARCHAR(MAX) NULL, -- JSON: [{"title":"", "value":"", "icon":"", "suffix":""}]
        
        -- BLOG SECTION
        [BlogTitle] NVARCHAR(200) NULL,
        [BlogSubtitle] NVARCHAR(500) NULL,
        [ShowLatestBlogs] BIT NOT NULL DEFAULT 1,
        [MaxBlogsToShow] INT NOT NULL DEFAULT 3,
        
        -- NEWSLETTER SECTION
        [NewsletterTitle] NVARCHAR(200) NULL,
        [NewsletterDescription] NVARCHAR(500) NULL,
        [NewsletterButtonText] NVARCHAR(100) NULL,
        
        -- STYLING & COLORS
        [HeroBackgroundColor] NVARCHAR(200) NULL,
        [PrimaryColor] NVARCHAR(50) NULL,
        [SecondaryColor] NVARCHAR(50) NULL,
        
        -- META
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT [PK_HomeContents] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT '✅ HomeContents tablosu başarıyla oluşturuldu!';
END
ELSE
BEGIN
    PRINT '⚠️ HomeContents tablosu zaten mevcut!';
END

-- Default Home Content ekle
IF NOT EXISTS (SELECT * FROM [HomeContents] WHERE [IsActive] = 1)
BEGIN
    INSERT INTO [HomeContents] (
        [HeroTitle], [HeroSubtitle], [HeroDescription], 
        [HeroVideoUrl], [HeroImageUrl], [HeroButtonText], [HeroSecondButtonText],
        [FeaturesTitle], [FeaturesSubtitle], [FeatureItems],
        [ProductsTitle], [ProductsSubtitle], [ShowPopularProducts], [MaxProductsToShow],
        [AboutTitle], [AboutContent], [AboutImageUrl], [AboutButtonText], [AboutFeatures],
        [StatsTitle], [StatsSubtitle], [StatsItems],
        [BlogTitle], [BlogSubtitle], [ShowLatestBlogs], [MaxBlogsToShow],
        [NewsletterTitle], [NewsletterDescription], [NewsletterButtonText],
        [HeroBackgroundColor], [PrimaryColor], [SecondaryColor],
        [IsActive], [CreatedAt], [UpdatedAt]
    ) 
    VALUES (
        N'Kaliteli Lezzetin Adresi',
        N'Uzman ellerden sofralarınıza uzanan kaliteli süt ürünleri',
        N'Taze ve güvenilir üretimle sağlığınız için en iyisi.',
        N'~/video/9586240-uhd_4096_2160_25fps.mp4',
        N'~/img/manyasli-gida.png',
        N'Ürünleri Keşfet',
        N'Hakkımızda',
        
        N'Neden Bizi Tercih Etmelisiniz?',
        N'Kalite, güven ve lezzet bir arada',
        N'[{"Title":"Kaliteli Üretim","Description":"Modern tesislerde hijyenik koşullarda üretim","Icon":"fas fa-award","Color":"primary"},{"Title":"Taze Ürünler","Description":"Her gün taze olarak üretilen süt ürünleri","Icon":"fas fa-leaf","Color":"success"},{"Title":"Güvenilir Marka","Description":"38 yıldır süren güven ve kalite anlayışı","Icon":"fas fa-shield-alt","Color":"info"},{"Title":"Hızlı Teslimat","Description":"Siparişlerinizi hızlı ve güvenli şekilde teslim ediyoruz","Icon":"fas fa-shipping-fast","Color":"warning"}]',
        
        N'Popüler Ürünlerimiz',
        N'En çok tercih edilen lezzetler',
        1,
        8,
        
        N'Manyaslı Süt Ürünleri Ailesi',
        N'1985''ten beri Balıkesir''in bereketli topraklarında üretim yaparak, kaliteli süt ürünlerini sofralarınıza taşıyoruz. Geleneksel lezzetleri modern teknoloji ile birleştirerek, her ürünümüzde kalite ve güven sunuyoruz.',
        N'~/img/carousel-1.jpg',
        N'Hikayemizi Keşfet',
        N'[{"Title":"Deneyim","Value":"38+ Yıl"},{"Title":"Müşteri","Value":"50.000+"},{"Title":"Ürün Çeşidi","Value":"100+"},{"Title":"İl","Value":"25+"}]',
        
        N'Güvenin Rakamları',
        N'38 yıldır devam eden kalite yolculuğumuz',
        N'[{"Title":"Mutlu Müşteri","Value":"50000","Icon":"fas fa-users","Suffix":"+","Color":"primary"},{"Title":"Yıllık Deneyim","Value":"38","Icon":"fas fa-calendar","Suffix":"+","Color":"success"},{"Title":"Ürün Çeşidi","Value":"100","Icon":"fas fa-boxes","Suffix":"+","Color":"info"},{"Title":"Şehir","Value":"25","Icon":"fas fa-map-marker-alt","Suffix":"+","Color":"warning"}]',
        
        N'Son Haberler & Blog',
        N'Sektörden son gelişmeler ve öneriler',
        1,
        3,
        
        N'Haberdar Ol!',
        N'Yeni ürünler, kampanyalar ve özel fırsatlardan ilk sen haberdar ol.',
        N'Abone Ol',
        
        N'linear-gradient(135deg, var(--primary-color) 0%, var(--secondary-color) 100%)',
        N'#8B4513',
        N'#D2691E',
        
        1,
        GETDATE(),
        GETDATE()
    );
    
    PRINT '✅ Varsayılan Home Content başarıyla eklendi!';
END
ELSE
BEGIN
    PRINT '⚠️ Home Content zaten mevcut!';
END

-- Index oluştur (performans için)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_HomeContents_IsActive_UpdatedAt')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HomeContents_IsActive_UpdatedAt] 
    ON [dbo].[HomeContents] ([IsActive] ASC, [UpdatedAt] DESC);
    
    PRINT '✅ HomeContents performans index''i oluşturuldu!';
END

PRINT '';
PRINT '🎉 HOME CONTENTS TABLOSU HAZIR!';
PRINT '📋 Admin panelden Ana Sayfa bölümünden içerikleri yönetebilirsiniz.';
PRINT '';

-- Tablo bilgilerini göster
SELECT 
    COUNT(*) as 'Toplam Kayıt',
    COUNT(CASE WHEN IsActive = 1 THEN 1 END) as 'Aktif Kayıt'
FROM [HomeContents];

GO
