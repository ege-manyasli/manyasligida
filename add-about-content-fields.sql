-- AboutContents tablosuna eksik alanları ekleme

-- StorySubtitle alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AboutContents' AND COLUMN_NAME = 'StorySubtitle')
BEGIN
    ALTER TABLE AboutContents ADD StorySubtitle NVARCHAR(500) NULL;
    PRINT 'StorySubtitle alanı eklendi.';
END

-- ValuesContent alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AboutContents' AND COLUMN_NAME = 'ValuesContent')
BEGIN
    ALTER TABLE AboutContents ADD ValuesContent NVARCHAR(MAX) NULL;
    PRINT 'ValuesContent alanı eklendi.';
END

-- RegionSubtitle alanını ekleme
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AboutContents' AND COLUMN_NAME = 'RegionSubtitle')
BEGIN
    ALTER TABLE AboutContents ADD RegionSubtitle NVARCHAR(500) NULL;
    PRINT 'RegionSubtitle alanı eklendi.';
END

PRINT 'AboutContents tablosu güncelleme tamamlandı.';
