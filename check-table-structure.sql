-- Mevcut tablo yapısını kontrol etme

-- HomeContents tablosunun mevcut yapısını göster
PRINT '=== HomeContents Tablosu Yapısı ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'HomeContents'
ORDER BY ORDINAL_POSITION;

PRINT '';

-- AboutContents tablosunun mevcut yapısını göster
PRINT '=== AboutContents Tablosu Yapısı ===';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AboutContents'
ORDER BY ORDINAL_POSITION;
