using Microsoft.EntityFrameworkCore;
using manyasligida.Data;
using manyasligida.Models;
using Microsoft.Extensions.Logging;

namespace manyasligida.Services
{
    public class SeedDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SeedDataService> _logger;

        public SeedDataService(ApplicationDbContext context, ILogger<SeedDataService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding...");

                // Check if data already exists
                if (await _context.Users.AnyAsync())
                {
                    _logger.LogInformation("Database already contains data, skipping seeding");
                    return;
                }

                // Seed Users
                await SeedUsersAsync();

                // Seed Categories
                await SeedCategoriesAsync();

                // Seed Products
                await SeedProductsAsync();

                // Seed Blogs
                await SeedBlogsAsync();

                // Seed Cookie Categories
                await SeedCookieCategoriesAsync();

                // Seed FAQs
                await SeedFAQsAsync();

                // Seed Galleries
                await SeedGalleriesAsync();

                // Seed Videos
                await SeedVideosAsync();

                await _context.SaveChangesAsync();
                _logger.LogInformation("Database seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database seeding");
                throw;
            }
        }

        private async Task SeedUsersAsync()
        {
            var users = new List<User>
            {
                new User
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@manyasligida.com",
                    Phone = "+90 555 000 0000",
                    Password = "admin123",
                    Address = "Balıkesir, Bandırma",
                    City = "Balıkesir",
                    PostalCode = "10200",
                    IsActive = true,
                    IsAdmin = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@manyasligida.com",
                    Phone = "+90 555 111 1111",
                    Password = "test123",
                    Address = "İstanbul, Kadıköy",
                    City = "İstanbul",
                    PostalCode = "34700",
                    IsActive = true,
                    IsAdmin = false,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                },
                new User
                {
                    FirstName = "Ege",
                    LastName = "Manyaslı",
                    Email = "ege@manyasligida.com",
                    Phone = "+90 555 222 2222",
                    Password = "ege123",
                    Address = "Balıkesir, Bandırma",
                    City = "Balıkesir",
                    PostalCode = "10200",
                    IsActive = true,
                    IsAdmin = true,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                }
            };

            await _context.Users.AddRangeAsync(users);
            _logger.LogInformation("Added {Count} users", users.Count);
        }

        private async Task SeedCategoriesAsync()
        {
            var categories = new List<Category>
            {
                new Category { Name = "Peynirler", Description = "Taze ve kaliteli peynir çeşitleri", IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new Category { Name = "Süt Ürünleri", Description = "Günlük taze süt ürünleri", IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.Now },
                new Category { Name = "Yoğurt", Description = "Doğal ve katkısız yoğurtlar", IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.Now },
                new Category { Name = "Tereyağı", Description = "Ev yapımı tereyağları", IsActive = true, DisplayOrder = 4, CreatedAt = DateTime.Now },
                new Category { Name = "Özel Ürünler", Description = "Sezonluk ve özel ürünler", IsActive = true, DisplayOrder = 5, CreatedAt = DateTime.Now },
                new Category { Name = "Kahvaltılık", Description = "Kahvaltı sofralarının vazgeçilmezleri", IsActive = true, DisplayOrder = 6, CreatedAt = DateTime.Now }
            };

            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync(); // Save to get IDs
            _logger.LogInformation("Added {Count} categories", categories.Count);
        }

        private async Task SeedProductsAsync()
        {
            var peynirlerCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Peynirler");
            var sutUrunleriCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Süt Ürünleri");
            var yogurtCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Yoğurt");
            var tereyagiCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Tereyağı");
            var ozelUrunlerCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Özel Ürünler");

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Taze Beyaz Peynir 500g",
                    Description = "Günlük taze beyaz peynir, geleneksel yöntemlerle üretilmiştir.",
                    Price = 25.90m,
                    OldPrice = 29.90m,
                    StockQuantity = 100,
                    CategoryId = peynirlerCategory?.Id,
                    IsActive = true,
                    IsPopular = true,
                    IsNew = false,
                    IsFeatured = true,
                    SortOrder = 1,
                    Weight = "500g",
                    FatContent = "Tam yağlı",
                    Ingredients = "Süt, tuz, maya",
                    NutritionalInfo = "Protein: 20g, Yağ: 25g, Karbonhidrat: 2g",
                    StorageInfo = "2-4°C arasında buzdolabında saklayın",
                    ExpiryInfo = "Üretim tarihinden itibaren 7 gün",
                    AllergenInfo = "Süt ürünleri",
                    MetaTitle = "Taze Beyaz Peynir - Manyaslı Gıda",
                    MetaDescription = "Balıkesir Bandırma'dan taze beyaz peynir",
                    MetaKeywords = "peynir, beyaz peynir, taze, organik",
                    Slug = "taze-beyaz-peynir-500g",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Name = "Kaşar Peyniri 400g",
                    Description = "Olgun kaşar peyniri, özel olgunlaştırma süreci ile",
                    Price = 35.50m,
                    OldPrice = 39.90m,
                    StockQuantity = 50,
                    CategoryId = peynirlerCategory?.Id,
                    IsActive = true,
                    IsPopular = true,
                    IsNew = false,
                    IsFeatured = true,
                    SortOrder = 2,
                    Weight = "400g",
                    FatContent = "Tam yağlı",
                    Ingredients = "Süt, tuz, maya",
                    NutritionalInfo = "Protein: 25g, Yağ: 30g, Karbonhidrat: 1g",
                    StorageInfo = "2-4°C arasında buzdolabında saklayın",
                    ExpiryInfo = "Üretim tarihinden itibaren 14 gün",
                    AllergenInfo = "Süt ürünleri",
                    MetaTitle = "Kaşar Peyniri - Manyaslı Gıda",
                    MetaDescription = "Geleneksel yöntemlerle üretilen kaşar peyniri",
                    MetaKeywords = "kaşar peyniri, peynir, olgun",
                    Slug = "kasar-peyniri-400g",
                    CreatedAt = DateTime.Now
                },
                new Product
                {
                    Name = "Tam Yağlı Süt 1L",
                    Description = "Günlük taze tam yağlı süt, doğal ve katkısız",
                    Price = 8.90m,
                    OldPrice = 10.90m,
                    StockQuantity = 200,
                    CategoryId = sutUrunleriCategory?.Id,
                    IsActive = true,
                    IsPopular = false,
                    IsNew = true,
                    IsFeatured = false,
                    SortOrder = 1,
                    Weight = "1L",
                    FatContent = "Tam yağlı",
                    Ingredients = "Süt",
                    NutritionalInfo = "Protein: 3.2g, Yağ: 3.6g, Karbonhidrat: 4.8g",
                    StorageInfo = "2-4°C arasında buzdolabında saklayın",
                    ExpiryInfo = "Üretim tarihinden itibaren 3 gün",
                    AllergenInfo = "Süt ürünleri",
                    MetaTitle = "Tam Yağlı Süt - Manyaslı Gıda",
                    MetaDescription = "Günlük taze tam yağlı süt",
                    MetaKeywords = "süt, tam yağlı, taze, organik",
                    Slug = "tam-yagli-sut-1l",
                    CreatedAt = DateTime.Now
                }
            };

            await _context.Products.AddRangeAsync(products);
            _logger.LogInformation("Added {Count} products", products.Count);
        }

        private async Task SeedBlogsAsync()
        {
            var blogs = new List<Blog>
            {
                new Blog
                {
                    Title = "Süt Ürünlerinin Faydaları",
                    Summary = "Süt ürünlerinin sağlığımıza olan faydalarını keşfedin",
                    Content = "Süt ürünleri, sağlıklı bir yaşam için vazgeçilmez besin kaynaklarıdır. Kalsiyum, protein ve D vitamini açısından zengin olan bu ürünler, kemik sağlığından bağışıklık sistemine kadar birçok alanda fayda sağlar.",
                    ImageUrl = "/uploads/blog/sut-urunleri-faydalari.jpg",
                    Author = "Ege Manyaslı",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    PublishedAt = DateTime.Now
                },
                new Blog
                {
                    Title = "Organik Üretimin Önemi",
                    Summary = "Organik süt ürünleri üretiminin çevre ve sağlık açısından önemi",
                    Content = "Organik üretim, sadece sağlığımız için değil, aynı zamanda çevremiz için de büyük önem taşır. Katkı maddesi kullanmadan, doğal yöntemlerle üretilen süt ürünleri, hem besin değeri açısından zengin hem de çevre dostudur.",
                    ImageUrl = "/uploads/blog/organik-uretim.jpg",
                    Author = "Admin User",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    PublishedAt = DateTime.Now
                }
            };

            await _context.Blogs.AddRangeAsync(blogs);
            _logger.LogInformation("Added {Count} blogs", blogs.Count);
        }

        private async Task SeedCookieCategoriesAsync()
        {
            var cookieCategories = new List<CookieCategory>
            {
                new CookieCategory { Name = "Gerekli Çerezler", Description = "Sitenin çalışması için gerekli çerezler", IsRequired = true, IsActive = true, SortOrder = 1, CreatedAt = DateTime.Now },
                new CookieCategory { Name = "Analitik Çerezler", Description = "Site kullanımını analiz eden çerezler", IsRequired = false, IsActive = true, SortOrder = 2, CreatedAt = DateTime.Now },
                new CookieCategory { Name = "Pazarlama Çerezler", Description = "Kişiselleştirilmiş reklamlar için çerezler", IsRequired = false, IsActive = true, SortOrder = 3, CreatedAt = DateTime.Now }
            };

            await _context.CookieCategories.AddRangeAsync(cookieCategories);
            _logger.LogInformation("Added {Count} cookie categories", cookieCategories.Count);
        }

        private async Task SeedFAQsAsync()
        {
            var faqs = new List<FAQ>
            {
                new FAQ { Question = "Ürünleriniz organik mi?", Answer = "Evet, tüm ürünlerimiz organik ve doğaldır. Hiçbir katkı maddesi kullanmadan, geleneksel yöntemlerle üretim yapıyoruz.", Category = "Ürünler", IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new FAQ { Question = "Teslimat süresi ne kadar?", Answer = "Sipariş verdikten sonra 1-2 iş günü içinde teslim edilir. Balıkesir ve çevre illerde aynı gün teslimat yapılabilir.", Category = "Teslimat", IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.Now },
                new FAQ { Question = "İade koşulları nelerdir?", Answer = "Ürün hasarlı gelirse 24 saat içinde iade edilebilir. Süt ürünleri olduğu için sağlık ve hijyen kurallarına uygun olarak işlem yapılır.", Category = "İade", IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.Now }
            };

            await _context.FAQs.AddRangeAsync(faqs);
            _logger.LogInformation("Added {Count} FAQs", faqs.Count);
        }

        private async Task SeedGalleriesAsync()
        {
            var galleries = new List<Gallery>
            {
                new Gallery { Title = "Üretim Tesisi", Description = "Modern teknoloji ile geleneksel yöntemlerin buluştuğu üretim tesisimiz", ImageUrl = "/uploads/gallery/uretim-tesisi.jpg", ThumbnailUrl = "/uploads/gallery/thumbnails/uretim-tesisi-thumb.jpg", IsActive = true, DisplayOrder = 1, Category = "Tesis", CreatedAt = DateTime.Now },
                new Gallery { Title = "Süt Toplama", Description = "Balıkesir'in temiz havasında yetişen ineklerden günlük süt toplama", ImageUrl = "/uploads/gallery/sut-toplama.jpg", ThumbnailUrl = "/uploads/gallery/thumbnails/sut-toplama-thumb.jpg", IsActive = true, DisplayOrder = 2, Category = "Üretim", CreatedAt = DateTime.Now }
            };

            await _context.Galleries.AddRangeAsync(galleries);
            _logger.LogInformation("Added {Count} gallery items", galleries.Count);
        }

        private async Task SeedVideosAsync()
        {
            var videos = new List<Video>
            {
                new Video { Title = "Manyaslı Gıda Tanıtım", Description = "Şirketimizin tanıtım videosu", VideoUrl = "/uploads/video/tanitim-video.mp4", ThumbnailUrl = "/uploads/video/thumbnails/tanitim-thumb.jpg", Duration = 180, ViewCount = 1250, IsActive = true, IsFeatured = true, DisplayOrder = 1, CreatedAt = DateTime.Now },
                new Video { Title = "Üretim Süreci", Description = "Geleneksel yöntemlerle süt ürünleri üretim süreci", VideoUrl = "/uploads/video/uretim-sureci.mp4", ThumbnailUrl = "/uploads/video/thumbnails/uretim-thumb.jpg", Duration = 240, ViewCount = 890, IsActive = true, IsFeatured = false, DisplayOrder = 2, CreatedAt = DateTime.Now }
            };

            await _context.Videos.AddRangeAsync(videos);
            _logger.LogInformation("Added {Count} videos", videos.Count);
        }
    }
}
