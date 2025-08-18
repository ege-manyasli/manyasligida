-- Add GalleryImageUrls column to Products table
-- This script adds support for gallery images in JSON format

-- Check if column doesn't exist before adding
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'GalleryImageUrls')
BEGIN
    ALTER TABLE Products 
    ADD GalleryImageUrls NVARCHAR(MAX) NULL;
    
    PRINT 'GalleryImageUrls column added to Products table successfully.';
END
ELSE
BEGIN
    PRINT 'GalleryImageUrls column already exists in Products table.';
END

-- Update existing products to have empty gallery images if needed
UPDATE Products 
SET GalleryImageUrls = '[]' 
WHERE GalleryImageUrls IS NULL;

PRINT 'GalleryImageUrls column setup completed.';

