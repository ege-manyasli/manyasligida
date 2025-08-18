-- Cookie kategorileri için örnek veriler
INSERT INTO CookieCategories (Name, Description, IsRequired, IsActive, SortOrder, CreatedAt) VALUES
('Gerekli', 'Bu çerezler web sitesinin temel işlevlerini yerine getirmek için gereklidir ve güvenlik, navigasyon ve erişilebilirlik gibi özellikleri sağlar.', 1, 1, 1, GETDATE()),
('Analitik', 'Bu çerezler web sitesinin nasıl kullanıldığını anlamamıza yardımcı olur ve performansı iyileştirmek için kullanılır.', 0, 1, 2, GETDATE()),
('Pazarlama', 'Bu çerezler reklamları kişiselleştirmek ve sosyal medya özelliklerini sağlamak için kullanılır.', 0, 1, 3, GETDATE()),
('Tercihler', 'Bu çerezler kullanıcı tercihlerini hatırlamak ve kişiselleştirilmiş deneyim sağlamak için kullanılır.', 0, 1, 4, GETDATE());

-- Örnek cookie consent verileri (test için)
INSERT INTO CookieConsents (SessionId, UserId, IpAddress, UserAgent, ConsentDate, ExpiryDate, IsActive, IsAccepted, Preferences, CreatedAt) VALUES
('test-session-1', NULL, '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', GETDATE(), DATEADD(YEAR, 1, GETDATE()), 1, 1, '{"analytics": true, "marketing": false}', GETDATE()),
('test-session-2', NULL, '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', GETDATE(), DATEADD(YEAR, 1, GETDATE()), 1, 1, '{"analytics": true, "marketing": true}', GETDATE()),
('test-session-3', NULL, '127.0.0.1', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', GETDATE(), DATEADD(YEAR, 1, GETDATE()), 1, 0, '{"analytics": false, "marketing": false}', GETDATE());

-- Cookie consent detayları
INSERT INTO CookieConsentDetails (CookieConsentId, CookieCategoryId, IsAccepted, CreatedAt) VALUES
(1, 1, 1, GETDATE()), -- Gerekli - Kabul
(1, 2, 1, GETDATE()), -- Analitik - Kabul
(1, 3, 0, GETDATE()), -- Pazarlama - Red
(1, 4, 0, GETDATE()), -- Tercihler - Red

(2, 1, 1, GETDATE()), -- Gerekli - Kabul
(2, 2, 1, GETDATE()), -- Analitik - Kabul
(2, 3, 1, GETDATE()), -- Pazarlama - Kabul
(2, 4, 1, GETDATE()), -- Tercihler - Kabul

(3, 1, 1, GETDATE()), -- Gerekli - Kabul (zorunlu)
(3, 2, 0, GETDATE()), -- Analitik - Red
(3, 3, 0, GETDATE()), -- Pazarlama - Red
(3, 4, 0, GETDATE()); -- Tercihler - Red
