-- SiteSettings tablosunu oluşturma
-- Bu script admin panelinde dinamik site ayarları için gerekli tabloyu oluşturur

-- SiteSettings tablosunu oluştur
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SiteSettings]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[SiteSettings](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Phone] [nvarchar](20) NOT NULL,
        [Email] [nvarchar](100) NOT NULL,
        [Address] [nvarchar](500) NOT NULL,
        [WorkingHours] [nvarchar](100) NOT NULL,
        [FacebookUrl] [nvarchar](500) NULL,
        [InstagramUrl] [nvarchar](500) NULL,
        [TwitterUrl] [nvarchar](500) NULL,
        [YoutubeUrl] [nvarchar](500) NULL,
        [SiteTitle] [nvarchar](200) NOT NULL,
        [SiteDescription] [nvarchar](500) NOT NULL,
        [SiteKeywords] [nvarchar](500) NOT NULL,
        [LogoUrl] [nvarchar](500) NULL,
        [FaviconUrl] [nvarchar](500) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [UpdatedAt] [datetime2](7) NULL,
        [IsActive] [bit] NOT NULL,
        CONSTRAINT [PK_SiteSettings] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    
    PRINT 'SiteSettings tablosu oluşturuldu.';
END
ELSE
BEGIN
    PRINT 'SiteSettings tablosu zaten mevcut.';
END

-- Varsayılan site ayarlarını ekle
IF NOT EXISTS (SELECT * FROM [dbo].[SiteSettings] WHERE [IsActive] = 1)
BEGIN
    INSERT INTO [dbo].[SiteSettings] (
        [Phone], [Email], [Address], [WorkingHours], 
        [FacebookUrl], [InstagramUrl], [TwitterUrl], [YoutubeUrl],
        [SiteTitle], [SiteDescription], [SiteKeywords], 
        [LogoUrl], [FaviconUrl], [CreatedAt], [IsActive]
    ) VALUES (
        '+90 266 123 45 67',
        'info@manyasligida.com',
        '17 Eylül, Hal Cd. No:6 D:8, 10200 Bandırma/Balıkesir',
        'Pzt-Cmt: 08:00-18:00',
        '#',
        '#',
        '#',
        '#',
        'Manyaslı Süt Ürünleri',
        'Kaliteli ve taze süt ürünleri',
        'süt, peynir, yoğurt, manyas, gıda',
        '/logomanyasli.png',
        '/favicon.ico',
        GETDATE(),
        1
    );
    
    PRINT 'Varsayılan site ayarları eklendi.';
END
ELSE
BEGIN
    PRINT 'Site ayarları zaten mevcut.';
END

-- Tablo yapısını kontrol et
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'SiteSettings'
ORDER BY ORDINAL_POSITION;

PRINT 'SiteSettings tablosu kontrol edildi.';
