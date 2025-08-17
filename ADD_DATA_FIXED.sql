-- ========================================
-- MANYASLI GIDA - VERİ EKLEME SCRIPT'İ (DÜZELTİLMİŞ)
-- ========================================
-- Bu script sadece veri ekler, tablo oluşturmaz
-- Önce FIX_DATABASE_STRUCTURE.sql çalıştırılmalı

PRINT 'MANYASLI GIDA VERİ EKLEME BAŞLIYOR...'

-- ========================================
-- VERİLERİ EKLE
-- ========================================
PRINT 'Veriler ekleniyor...'

-- Kullanıcılar (Log dosyalarından çıkarıldı)
INSERT INTO Users (FirstName, LastName, Email, Phone, Password, Address, City, PostalCode, IsActive, IsAdmin, EmailConfirmed, CreatedAt) VALUES
('Admin', 'User', 'admin@manyasligida.com', '+90 555 000 0000', 'admin123', 'Balıkesir, Bandırma', 'Balıkesir', '10200', 1, 1, 1, GETDATE()),
('Test', 'User', 'test@manyasligida.com', '+90 555 111 1111', 'test123', 'İstanbul, Kadıköy', 'İstanbul', '34700', 1, 0, 1, GETDATE()),
('Ege', 'Manyaslı', 'ege@manyasligida.com', '+90 555 222 2222', 'ege123', 'Balıkesir, Bandırma', 'Balıkesir', '10200', 1, 1, 1, GETDATE()),
('Ahmet', 'Yılmaz', 'ahmet@example.com', '+90 555 333 3333', 'ahmet123', 'Ankara, Çankaya', 'Ankara', '06690', 1, 0, 1, GETDATE()),
('Fatma', 'Demir', 'fatma@example.com', '+90 555 444 4444', 'fatma123', 'İzmir, Konak', 'İzmir', '35210', 1, 0, 1, GETDATE());

PRINT '5 kullanıcı eklendi.'

-- Kategoriler (Gerçek veriler)
INSERT INTO Categories (Name, Description, IsActive, DisplayOrder, CreatedAt) VALUES
('Peynirler', 'Taze ve kaliteli peynir çeşitleri', 1, 1, GETDATE()),
('Süt Ürünleri', 'Günlük taze süt ürünleri', 1, 2, GETDATE()),
('Yoğurt', 'Doğal ve katkısız yoğurtlar', 1, 3, GETDATE()),
('Tereyağı', 'Ev yapımı tereyağları', 1, 4, GETDATE()),
('Özel Ürünler', 'Sezonluk ve özel ürünler', 1, 5, GETDATE()),
('Kahvaltılık', 'Kahvaltı sofralarının vazgeçilmezleri', 1, 6, GETDATE());

PRINT '6 kategori eklendi.'

-- Ürünler (Log dosyalarından çıkarılan gerçek veriler)
INSERT INTO Products (Name, Description, Price, OldPrice, StockQuantity, CategoryId, IsActive, IsPopular, IsNew, IsFeatured, SortOrder, Weight, FatContent, Ingredients, NutritionalInfo, StorageInfo, ExpiryInfo, AllergenInfo, MetaTitle, MetaDescription, MetaKeywords, Slug, CreatedAt) VALUES
('Taze Beyaz Peynir 500g', 'Günlük taze beyaz peynir, geleneksel yöntemlerle üretilmiştir.', 25.90, 29.90, 100, 1, 1, 1, 0, 1, 1, '500g', 'Tam yağlı', 'Süt, tuz, maya', 'Protein: 20g, Yağ: 25g, Karbonhidrat: 2g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 7 gün', 'Süt ürünleri', 'Taze Beyaz Peynir - Manyaslı Gıda', 'Balıkesir Bandırma''dan taze beyaz peynir', 'peynir, beyaz peynir, taze, organik', 'taze-beyaz-peynir-500g', GETDATE()),
('Kaşar Peyniri 400g', 'Olgun kaşar peyniri, özel olgunlaştırma süreci ile', 35.50, 39.90, 50, 1, 1, 1, 0, 1, 2, '400g', 'Tam yağlı', 'Süt, tuz, maya', 'Protein: 25g, Yağ: 30g, Karbonhidrat: 1g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 14 gün', 'Süt ürünleri', 'Kaşar Peyniri - Manyaslı Gıda', 'Geleneksel yöntemlerle üretilen kaşar peyniri', 'kaşar peyniri, peynir, olgun', 'kasar-peyniri-400g', GETDATE()),
('Tam Yağlı Süt 1L', 'Günlük taze tam yağlı süt, doğal ve katkısız', 8.90, 10.90, 200, 2, 1, 0, 1, 0, 1, '1L', 'Tam yağlı', 'Süt', 'Protein: 3.2g, Yağ: 3.6g, Karbonhidrat: 4.8g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 3 gün', 'Süt ürünleri', 'Tam Yağlı Süt - Manyaslı Gıda', 'Günlük taze tam yağlı süt', 'süt, tam yağlı, taze, organik', 'tam-yagli-sut-1l', GETDATE()),
('Doğal Yoğurt 500g', 'Katkısız doğal yoğurt, geleneksel yöntemlerle', 12.50, 14.90, 75, 3, 1, 0, 1, 0, 1, '500g', 'Tam yağlı', 'Süt, maya', 'Protein: 5g, Yağ: 3.5g, Karbonhidrat: 4g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 7 gün', 'Süt ürünleri', 'Doğal Yoğurt - Manyaslı Gıda', 'Katkısız doğal yoğurt', 'yoğurt, doğal, katkısız', 'dogal-yogurt-500g', GETDATE()),
('Köy Tereyağı 250g', 'El yapımı köy tereyağı, geleneksel yöntemlerle', 28.90, 32.90, 30, 4, 1, 1, 1, 1, 1, '250g', 'Tam yağlı', 'Krema, tuz', 'Protein: 0.5g, Yağ: 82g, Karbonhidrat: 0.5g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 30 gün', 'Süt ürünleri', 'Köy Tereyağı - Manyaslı Gıda', 'El yapımı köy tereyağı', 'tereyağı, köy, el yapımı', 'koy-tereyagi-250g', GETDATE()),
('Ezine Beyaz Peynir 300g', 'Ezine yöresine özgü beyaz peynir', 18.90, 22.90, 60, 1, 1, 0, 0, 0, 3, '300g', 'Tam yağlı', 'Süt, tuz, maya', 'Protein: 18g, Yağ: 22g, Karbonhidrat: 2g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 10 gün', 'Süt ürünleri', 'Ezine Beyaz Peynir - Manyaslı Gıda', 'Ezine yöresine özgü beyaz peynir', 'ezine peyniri, beyaz peynir', 'ezine-beyaz-peynir-300g', GETDATE()),
('Kaymak 200g', 'Geleneksel yöntemlerle üretilen kaymak', 45.90, 49.90, 20, 5, 1, 1, 1, 1, 1, '200g', 'Tam yağlı', 'Krema', 'Protein: 2g, Yağ: 85g, Karbonhidrat: 1g', '2-4°C arasında buzdolabında saklayın', 'Üretim tarihinden itibaren 5 gün', 'Süt ürünleri', 'Kaymak - Manyaslı Gıda', 'Geleneksel yöntemlerle üretilen kaymak', 'kaymak, geleneksel', 'kaymak-200g', GETDATE());

PRINT '7 ürün eklendi.'

-- Blog yazıları (Log dosyalarından çıkarıldı)
INSERT INTO Blogs (Title, Summary, Content, ImageUrl, Author, IsActive, CreatedAt, PublishedAt) VALUES
('Süt Ürünlerinin Faydaları', 'Süt ürünlerinin sağlığımıza olan faydalarını keşfedin', 'Süt ürünleri, sağlıklı bir yaşam için vazgeçilmez besin kaynaklarıdır. Kalsiyum, protein ve D vitamini açısından zengin olan bu ürünler, kemik sağlığından bağışıklık sistemine kadar birçok alanda fayda sağlar. Geleneksel yöntemlerle üretilen süt ürünleri, modern üretim tekniklerinin yanı sıra atalarımızdan gelen bilgi ve deneyimi de içerir. Balıkesir''in bereketli topraklarında yetişen kaliteli otlarla beslenen ineklerin sütünden üretilen ürünlerimiz, doğallığı ve lezzeti ile sofralarınızı zenginleştirir.', '/uploads/blog/sut-urunleri-faydalari.jpg', 'Ege Manyaslı', 1, GETDATE(), GETDATE()),
('Organik Üretimin Önemi', 'Organik süt ürünleri üretiminin çevre ve sağlık açısından önemi', 'Organik üretim, sadece sağlığımız için değil, aynı zamanda çevremiz için de büyük önem taşır. Katkı maddesi kullanmadan, doğal yöntemlerle üretilen süt ürünleri, hem besin değeri açısından zengin hem de çevre dostudur. Manyaslı Gıda olarak, 1985 yılından bu yana geleneksel üretim yöntemlerini koruyarak, modern teknoloji ile birleştirip, müşterilerimize en kaliteli ürünleri sunmaya devam ediyoruz.', '/uploads/blog/organik-uretim.jpg', 'Admin User', 1, GETDATE(), GETDATE()),
('Balıkesir''in Süt Ürünleri Kültürü', 'Balıkesir yöresinin zengin süt ürünleri kültürü ve geleneksel yöntemleri', 'Balıkesir, Türkiye''nin en önemli süt ürünleri üretim merkezlerinden biridir. Yörenin zengin otlakları, temiz havası ve geleneksel üretim yöntemleri, burada üretilen süt ürünlerine eşsiz bir lezzet katar. Bandırma''nın deniz etkisi ile oluşan mikro klima, süt ürünlerimizin karakteristik özelliklerini destekler. Bu yazımızda, Balıkesir''in süt ürünleri kültürünü ve geleneksel yöntemlerini detaylı olarak ele alıyoruz.', '/uploads/blog/balikesir-sut-kulturu.jpg', 'Ege Manyaslı', 1, GETDATE(), GETDATE());

PRINT '3 blog yazısı eklendi.'

-- Cookie kategorileri
INSERT INTO CookieCategories (Name, Description, IsRequired, IsActive, SortOrder, CreatedAt) VALUES
('Gerekli Çerezler', 'Sitenin çalışması için gerekli çerezler', 1, 1, 1, GETDATE()),
('Analitik Çerezler', 'Site kullanımını analiz eden çerezler', 0, 1, 2, GETDATE()),
('Pazarlama Çerezler', 'Kişiselleştirilmiş reklamlar için çerezler', 0, 1, 3, GETDATE());

PRINT '3 cookie kategorisi eklendi.'

-- FAQ
INSERT INTO FAQs (Question, Answer, Category, IsActive, DisplayOrder, CreatedAt) VALUES
('Ürünleriniz organik mi?', 'Evet, tüm ürünlerimiz organik ve doğaldır. Hiçbir katkı maddesi kullanmadan, geleneksel yöntemlerle üretim yapıyoruz.', 'Ürünler', 1, 1, GETDATE()),
('Teslimat süresi ne kadar?', 'Sipariş verdikten sonra 1-2 iş günü içinde teslim edilir. Balıkesir ve çevre illerde aynı gün teslimat yapılabilir.', 'Teslimat', 1, 2, GETDATE()),
('İade koşulları nelerdir?', 'Ürün hasarlı gelirse 24 saat içinde iade edilebilir. Süt ürünleri olduğu için sağlık ve hijyen kurallarına uygun olarak işlem yapılır.', 'İade', 1, 3, GETDATE()),
('Hangi bölgelere teslimat yapıyorsunuz?', 'Balıkesir, İstanbul, İzmir, Ankara ve çevre illere teslimat yapıyoruz. Diğer iller için özel anlaşmalar yapılabilir.', 'Teslimat', 1, 4, GETDATE()),
('Ürünleriniz sertifikalı mı?', 'Evet, tüm ürünlerimiz ISO 22000, HACCP ve Helal sertifikalarına sahiptir.', 'Kalite', 1, 5, GETDATE());

PRINT '5 FAQ eklendi.'

-- Galeri
INSERT INTO Galleries (Title, Description, ImageUrl, ThumbnailUrl, IsActive, DisplayOrder, Category, CreatedAt) VALUES
('Üretim Tesisi', 'Modern teknoloji ile geleneksel yöntemlerin buluştuğu üretim tesisimiz', '/uploads/gallery/uretim-tesisi.jpg', '/uploads/gallery/thumbnails/uretim-tesisi-thumb.jpg', 1, 1, 'Tesis', GETDATE()),
('Süt Toplama', 'Balıkesir''in temiz havasında yetişen ineklerden günlük süt toplama', '/uploads/gallery/sut-toplama.jpg', '/uploads/gallery/thumbnails/sut-toplama-thumb.jpg', 1, 2, 'Üretim', GETDATE()),
('Kalite Kontrol', 'Laboratuvar ortamında detaylı kalite kontrol süreçleri', '/uploads/gallery/kalite-kontrol.jpg', '/uploads/gallery/thumbnails/kalite-kontrol-thumb.jpg', 1, 3, 'Kalite', GETDATE()),
('Paketleme', 'Hijyenik koşullarda paketleme süreçleri', '/uploads/gallery/paketleme.jpg', '/uploads/gallery/thumbnails/paketleme-thumb.jpg', 1, 4, 'Üretim', GETDATE());

PRINT '4 galeri görseli eklendi.'

-- Videolar
INSERT INTO Videos (Title, Description, VideoUrl, ThumbnailUrl, Duration, ViewCount, IsActive, IsFeatured, DisplayOrder, CreatedAt) VALUES
('Manyaslı Gıda Tanıtım', 'Şirketimizin tanıtım videosu', '/uploads/video/tanitim-video.mp4', '/uploads/video/thumbnails/tanitim-thumb.jpg', 180, 1250, 1, 1, 1, GETDATE()),
('Üretim Süreci', 'Geleneksel yöntemlerle süt ürünleri üretim süreci', '/uploads/video/uretim-sureci.mp4', '/uploads/video/thumbnails/uretim-thumb.jpg', 240, 890, 1, 0, 2, GETDATE()),
('Balıkesir''de Süt Üretimi', 'Balıkesir yöresinde süt üretimi ve kültürü', '/uploads/video/balikesir-sut-uretim.mp4', '/uploads/video/thumbnails/balikesir-thumb.jpg', 300, 567, 1, 0, 3, GETDATE());

PRINT '3 video eklendi.'

-- ========================================
-- TAMAMLANMA MESAJI
-- ========================================
PRINT '========================================';
PRINT 'MANYASLI GIDA VERİ EKLEME TAMAMLANDI!';
PRINT '========================================';
PRINT 'EKLENEN VERİLER:';
PRINT '- 5 Kullanıcı (Admin, Test, Ege, Ahmet, Fatma)';
PRINT '- 6 Kategori (Peynirler, Süt Ürünleri, Yoğurt, Tereyağı, Özel Ürünler, Kahvaltılık)';
PRINT '- 7 Ürün (Beyaz Peynir, Kaşar, Süt, Yoğurt, Tereyağı, Ezine Peyniri, Kaymak)';
PRINT '- 3 Blog Yazısı';
PRINT '- 3 Cookie Kategorisi';
PRINT '- 5 FAQ';
PRINT '- 4 Galeri Görseli';
PRINT '- 3 Video';
PRINT '';
PRINT 'GİRİŞ BİLGİLERİ:';
PRINT '- Admin: admin@manyasligida.com / admin123';
PRINT '- Test: test@manyasligida.com / test123';
PRINT '- Ege: ege@manyasligida.com / ege123';
PRINT '';
PRINT 'VERİLER BAŞARIYLA EKLENDİ! 🚀';
PRINT '========================================';

-- Son kontrol
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Categories', COUNT(*) FROM Categories
UNION ALL  
SELECT 'Products', COUNT(*) FROM Products
UNION ALL
SELECT 'Blogs', COUNT(*) FROM Blogs
UNION ALL
SELECT 'CookieCategories', COUNT(*) FROM CookieCategories
UNION ALL
SELECT 'FAQs', COUNT(*) FROM FAQs
UNION ALL
SELECT 'Galleries', COUNT(*) FROM Galleries
UNION ALL
SELECT 'Videos', COUNT(*) FROM Videos
ORDER BY TableName;
