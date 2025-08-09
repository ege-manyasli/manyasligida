-- HİZLİ DATABASE CHECK SCRIPT
-- Bu script Azure'da çalıştırılacak

-- 1. Mevcut tabloları listele
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- 2. Users tablosunu kontrol et
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    SELECT 'Users table exists' as Status;
    SELECT COUNT(*) as UserCount FROM Users;
END
ELSE
BEGIN
    SELECT 'Users table MISSING!' as Status;
END

-- 3. UserSessions tablosunu kontrol et  
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserSessions')
BEGIN
    SELECT 'UserSessions table exists' as Status;
    SELECT COUNT(*) as SessionCount FROM UserSessions;
END
ELSE
BEGIN
    SELECT 'UserSessions table MISSING!' as Status;
END

-- 4. Products tablosunu kontrol et
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
BEGIN
    SELECT 'Products table exists' as Status;
    SELECT COUNT(*) as ProductCount FROM Products;
END
ELSE
BEGIN
    SELECT 'Products table MISSING!' as Status;
END
