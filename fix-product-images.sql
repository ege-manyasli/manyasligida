-- Fix Product Images - Update sample data to use actual uploaded images
-- This script updates the sample product data to use images that actually exist

UPDATE Products 
SET ImageUrl = '/uploads/products/03d797a9-8bd3-41a2-a94d-48633bd2be86.jpg'
WHERE Id = 1 AND ImageUrl = '/img/ezine-tipi-sert-beyaz-peynir-650-gr.-52d9.jpg';

UPDATE Products 
SET ImageUrl = '/uploads/products/121127ce-5603-4f18-982d-49c65dae539a.jpg'
WHERE Id = 2 AND ImageUrl = '/img/taze-kasar-peyniri-1000-gr.-63a593.jpg';

UPDATE Products 
SET ImageUrl = '/uploads/products/149931be-9c48-4837-af05-a1db4b2a9607.jpg'
WHERE Id = 3 AND ImageUrl = '/img/mihalic-peyniri-350-gr.-122f.jpg';

UPDATE Products 
SET ImageUrl = '/uploads/products/2d4f31d9-106b-4edf-8fc9-47de151e7a70.jpg'
WHERE Id = 4 AND ImageUrl = '/img/dil-peyniri-400-gr.-5f1a.jpg';

UPDATE Products 
SET ImageUrl = '/uploads/products/3bee1a62-2591-4997-97c6-b6b3a5f660c7.jpg'
WHERE Id = 5 AND ImageUrl = '/img/biberli-sepet-peyniri-350-gr.-2e1f.jpg';

-- Update any other products that might have missing images
UPDATE Products 
SET ImageUrl = '/uploads/products/7533002c-784b-4bc6-94e2-5f8f06af0112.jpg'
WHERE ImageUrl LIKE '/img/%' AND Id > 5;

-- Set ImageUrls JSON for products that have main images
UPDATE Products 
SET ImageUrls = '["' + ImageUrl + '"]'
WHERE ImageUrl IS NOT NULL AND ImageUrls IS NULL;
