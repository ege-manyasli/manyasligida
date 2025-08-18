-- Check and Add GalleryImageUrls column to Products table
-- This script safely adds the GalleryImageUrls column if it doesn't exist

-- First, check if the column exists
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Products' 
    AND COLUMN_NAME = 'GalleryImageUrls'
)
BEGIN
    -- Add the column
    ALTER TABLE Products 
    ADD GalleryImageUrls NVARCHAR(MAX) NULL;
    
    PRINT 'GalleryImageUrls column added to Products table successfully.';
    
    -- Update existing products to have empty gallery images
    UPDATE Products 
    SET GalleryImageUrls = '[]' 
    WHERE GalleryImageUrls IS NULL;
    
    PRINT 'Existing products updated with empty gallery images.';
END
ELSE
BEGIN
    PRINT 'GalleryImageUrls column already exists in Products table.';
END

-- Verify the column was added
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Products' 
AND COLUMN_NAME = 'GalleryImageUrls';

PRINT 'GalleryImageUrls column setup verification completed.';

