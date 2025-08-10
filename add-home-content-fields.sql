-- HomeContents tablosuna eksik alanları ekleme

-- HeroButtonUrl alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'HeroButtonUrl')
BEGIN
    ALTER TABLE HomeContents ADD HeroButtonUrl NVARCHAR(500) NULL;
    PRINT 'HeroButtonUrl alanı eklendi.';
END

-- AboutSubtitle alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'AboutSubtitle')
BEGIN
    ALTER TABLE HomeContents ADD AboutSubtitle NVARCHAR(500) NULL;
    PRINT 'AboutSubtitle alanı eklendi.';
END

-- AboutDescription alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'AboutDescription')
BEGIN
    ALTER TABLE HomeContents ADD AboutDescription NVARCHAR(MAX) NULL;
    PRINT 'AboutDescription alanı eklendi.';
END

-- ServicesTitle alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'ServicesTitle')
BEGIN
    ALTER TABLE HomeContents ADD ServicesTitle NVARCHAR(200) NULL;
    PRINT 'ServicesTitle alanı eklendi.';
END

-- ServicesSubtitle alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'ServicesSubtitle')
BEGIN
    ALTER TABLE HomeContents ADD ServicesSubtitle NVARCHAR(500) NULL;
    PRINT 'ServicesSubtitle alanı eklendi.';
END

-- ServicesDescription alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'ServicesDescription')
BEGIN
    ALTER TABLE HomeContents ADD ServicesDescription NVARCHAR(MAX) NULL;
    PRINT 'ServicesDescription alanı eklendi.';
END

-- ContactTitle alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'ContactTitle')
BEGIN
    ALTER TABLE HomeContents ADD ContactTitle NVARCHAR(200) NULL;
    PRINT 'ContactTitle alanı eklendi.';
END

-- ContactSubtitle alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'ContactSubtitle')
BEGIN
    ALTER TABLE HomeContents ADD ContactSubtitle NVARCHAR(500) NULL;
    PRINT 'ContactSubtitle alanı eklendi.';
END

-- ContactDescription alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'ContactDescription')
BEGIN
    ALTER TABLE HomeContents ADD ContactDescription NVARCHAR(MAX) NULL;
    PRINT 'ContactDescription alanı eklendi.';
END

-- ContactPhone alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'ContactPhone')
BEGIN
    ALTER TABLE HomeContents ADD ContactPhone NVARCHAR(50) NULL;
    PRINT 'ContactPhone alanı eklendi.';
END

-- ContactEmail alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'ContactEmail')
BEGIN
    ALTER TABLE HomeContents ADD ContactEmail NVARCHAR(100) NULL;
    PRINT 'ContactEmail alanı eklendi.';
END

-- ContactAddress alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'HomeContents' AND COLUMN_NAME = 'ContactAddress')
BEGIN
    ALTER TABLE HomeContents ADD ContactAddress NVARCHAR(500) NULL;
    PRINT 'ContactAddress alanı eklendi.';
END

PRINT 'HomeContents tablosu güncelleme tamamlandı.';
