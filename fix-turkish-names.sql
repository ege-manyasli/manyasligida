-- TÜRKÇE KARAKTER SORUNU DÜZELTİCİ SQL
-- Azure SQL Database'de çalıştır

-- Ege Manyaslı adını düzelt
UPDATE Users 
SET FirstName = 'Ege', LastName = 'Manyaslı'
WHERE Email = 'ege@manyasligida.com';

-- Diğer Türkçe karakterleri de düzelt (eğer varsa)
UPDATE Users 
SET 
    FirstName = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(FirstName, 
        '&#x131;', 'ı'), 
        '&#xfc;', 'ü'), 
        '&#xf6;', 'ö'), 
        '&#xe7;', 'ç'), 
        '&#x11f;', 'ğ'), 
        '&#x15f;', 'ş'),
    LastName = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LastName, 
        '&#x131;', 'ı'), 
        '&#xfc;', 'ü'), 
        '&#xf6;', 'ö'), 
        '&#xe7;', 'ç'), 
        '&#x11f;', 'ğ'), 
        '&#x15f;', 'ş')
WHERE 
    FirstName LIKE '%&#x%' OR 
    LastName LIKE '%&#x%';

-- Kontrol et
SELECT Id, FirstName, LastName, Email, FirstName + ' ' + LastName as FullName
FROM Users 
WHERE IsActive = 1;
