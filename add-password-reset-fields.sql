-- Azure SQL Database'e User tablosuna eksik password reset kolonlarını ekleme
-- Bu script şifre sıfırlama işleminin çalışması için gerekli kolonları ekler

-- PasswordResetToken kolonu ekleme
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PasswordResetToken')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [PasswordResetToken] NVARCHAR(MAX) NULL;
    PRINT 'PasswordResetToken kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'PasswordResetToken kolonu zaten mevcut.';
END

-- PasswordResetTokenExpiry kolonu ekleme
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PasswordResetTokenExpiry')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [PasswordResetTokenExpiry] DATETIME2 NULL;
    PRINT 'PasswordResetTokenExpiry kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'PasswordResetTokenExpiry kolonu zaten mevcut.';
END

-- PasswordResetCode kolonu ekleme
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PasswordResetCode')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [PasswordResetCode] NVARCHAR(MAX) NULL;
    PRINT 'PasswordResetCode kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'PasswordResetCode kolonu zaten mevcut.';
END

-- PasswordResetCodeExpiry kolonu ekleme
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'PasswordResetCodeExpiry')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [PasswordResetCodeExpiry] DATETIME2 NULL;
    PRINT 'PasswordResetCodeExpiry kolonu eklendi.';
END
ELSE
BEGIN
    PRINT 'PasswordResetCodeExpiry kolonu zaten mevcut.';
END

-- Mevcut kolonları kontrol etme
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Users' 
AND COLUMN_NAME IN ('PasswordResetToken', 'PasswordResetTokenExpiry', 'PasswordResetCode', 'PasswordResetCodeExpiry')
ORDER BY COLUMN_NAME;

PRINT 'Password reset kolonları kontrol edildi.';
