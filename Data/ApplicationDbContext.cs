using Microsoft.EntityFrameworkCore;
using manyasligida.Models;

namespace manyasligida.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Video> Videos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CartItem - Cart relationship
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .IsRequired(false);

            // Decimal precision configurations
            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.SubTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TaxAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.OldPrice)
                .HasPrecision(18, 2);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Default Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@manyasligida.com",
                    Phone = "+90 266 123 45 67",
                    Password = "sNaWAMfPr1AQUDPjD9iKHB3Jc+Ky6BbhLPYBvU4hwjI=", // Hash of "admin123"
                    IsActive = true,
                    IsAdmin = true,
                    EmailConfirmed = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Beyaz Peynir", Description = "Geleneksel beyaz peynir çeşitleri", DisplayOrder = 1, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 2, Name = "Kaşar Peyniri", Description = "Taze ve eski kaşar peynirleri", DisplayOrder = 2, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 3, Name = "Sepet Peyniri", Description = "Özel sepet peynirleri", DisplayOrder = 3, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) },
                new Category { Id = 4, Name = "Özel Peynirler", Description = "Özel üretim peynir çeşitleri", DisplayOrder = 4, IsActive = true, CreatedAt = new DateTime(2024, 1, 1) }
            );

            // Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Ezine Beyaz Peynir",
                    Description = "Çanakkale Ezine'den özel üretim tam yağlı beyaz peynir",
                    Price = 45.00m,
                    OldPrice = 55.00m,
                    Weight = "600 Gr",
                    FatContent = "%45-50",
                    StockQuantity = 100,
                    ImageUrl = "/img/ezine-tipi-sert-beyaz-peynir-650-gr.-52d9.jpg",
                    CategoryId = 1,
                    IsActive = true,
                    IsPopular = true,
                    IsNew = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Product
                {
                    Id = 2,
                    Name = "Taze Kaşar Peyniri",
                    Description = "Günlük taze üretim yumuşak kaşar peyniri",
                    Price = 35.00m,
                    OldPrice = null,
                    Weight = "1000 Gr",
                    FatContent = "%40-45",
                    StockQuantity = 75,
                    ImageUrl = "/img/taze-kasar-peyniri-1000-gr.-63a593.jpg",
                    CategoryId = 2,
                    IsActive = true,
                    IsPopular = true,
                    IsNew = false,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Product
                {
                    Id = 3,
                    Name = "Mihaliç Peyniri",
                    Description = "Geleneksel Mihaliç peyniri",
                    Price = 28.00m,
                    OldPrice = 35.00m,
                    Weight = "350 Gr",
                    FatContent = "%35-40",
                    StockQuantity = 50,
                    ImageUrl = "/img/mihalic-peyniri-350-gr.-122f.jpg",
                    CategoryId = 4,
                    IsActive = true,
                    IsPopular = false,
                    IsNew = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Product
                {
                    Id = 4,
                    Name = "Çörek Otlu Sepet Peyniri",
                    Description = "Çörek otu ile aromalandırılmış özel sepet peyniri",
                    Price = 32.00m,
                    OldPrice = null,
                    Weight = "350 Gr",
                    FatContent = "%42-47",
                    StockQuantity = 60,
                    ImageUrl = "/img/corek-otlu-sepet-peyniri-350-gr.-8c96.jpg",
                    CategoryId = 3,
                    IsActive = true,
                    IsPopular = false,
                    IsNew = false,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Product
                {
                    Id = 5,
                    Name = "Biberli Sepet Peyniri",
                    Description = "Kırmızı biber ile tatlandırılmış sepet peyniri",
                    Price = 30.00m,
                    OldPrice = 38.00m,
                    Weight = "350 Gr",
                    FatContent = "%40-45",
                    StockQuantity = 40,
                    ImageUrl = "/img/biberli-sepet-peyniri-350-gr.-2e1f.jpg",
                    CategoryId = 3,
                    IsActive = true,
                    IsPopular = true,
                    IsNew = false,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Product
                {
                    Id = 6,
                    Name = "Eski Kars Kaşarı",
                    Description = "Olgunlaştırılmış eski kaşar peyniri",
                    Price = 42.00m,
                    OldPrice = null,
                    Weight = "300 Gr",
                    FatContent = "%45-50",
                    StockQuantity = 30,
                    ImageUrl = "/img/eski-kasar-peyniri-300-g.-6a1-d2.jpg",
                    CategoryId = 2,
                    IsActive = true,
                    IsPopular = false,
                    IsNew = false,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Product
                {
                    Id = 7,
                    Name = "Dil Peyniri",
                    Description = "Yumuşak ve lezzetli dil peyniri",
                    Price = 25.00m,
                    OldPrice = 32.00m,
                    Weight = "400 Gr",
                    FatContent = "%35-40",
                    StockQuantity = 80,
                    ImageUrl = "/img/dil-peyniri-400-gr.-5f1a.jpg",
                    CategoryId = 1,
                    IsActive = true,
                    IsPopular = false,
                    IsNew = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Product
                {
                    Id = 8,
                    Name = "Otlu Beyaz Peynir",
                    Description = "Taze otlar ile aromalandırılmış beyaz peynir",
                    Price = 38.00m,
                    OldPrice = null,
                    Weight = "500 Gr",
                    FatContent = "%42-47",
                    StockQuantity = 55,
                    ImageUrl = "/img/product-1.jpg",
                    CategoryId = 1,
                    IsActive = true,
                    IsPopular = true,
                    IsNew = false,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Blogs
            modelBuilder.Entity<Blog>().HasData(
                new Blog
                {
                    Id = 1,
                    Title = "Peynir Üretiminde Geleneksel Yöntemler",
                    Summary = "Manyas'ta peynir üretiminde kullanılan geleneksel yöntemler ve modern teknolojinin uyumu hakkında detaylı bilgi.",
                    Content = "Peynir üretimi, binlerce yıllık bir geleneğe sahiptir. Manyas'ta bu geleneksel yöntemler modern teknoloji ile birleştirilerek en kaliteli peynirler üretilmektedir...",
                    ImageUrl = "/img/blog-1.jpg",
                    IsActive = true,
                    PublishedAt = new DateTime(2024, 1, 1),
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Blog
                {
                    Id = 2,
                    Title = "Sağlıklı Beslenmede Peynirin Önemi",
                    Summary = "Peynirin besin değeri ve sağlıklı beslenmedeki rolü hakkında uzman görüşleri.",
                    Content = "Peynir, protein, kalsiyum ve diğer önemli besin maddelerini içeren değerli bir gıda maddesidir. Düzenli tüketimi kemik sağlığı ve kas gelişimi için önemlidir...",
                    ImageUrl = "/img/blog-2.jpg",
                    IsActive = true,
                    PublishedAt = new DateTime(2024, 1, 1),
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Blog
                {
                    Id = 3,
                    Title = "Peynir Çeşitleri ve Kullanım Alanları",
                    Summary = "Farklı peynir çeşitlerinin özellikleri ve mutfakta nasıl kullanılacağı hakkında pratik bilgiler.",
                    Content = "Her peynir çeşidinin kendine özgü lezzeti ve kullanım alanı vardır. Beyaz peynir kahvaltı sofralarının vazgeçilmezi iken, kaşar peyniri ısıtıldığında eriyen yapısıyla pizza ve tost için idealdir...",
                    ImageUrl = "/img/blog-3.jpg",
                    IsActive = true,
                    PublishedAt = new DateTime(2024, 1, 1),
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // FAQs
            modelBuilder.Entity<FAQ>().HasData(
                new FAQ
                {
                    Id = 1,
                    Question = "Peynirleriniz ne kadar taze?",
                    Answer = "Tüm peynirlerimiz günlük üretimdir ve en fazla 24 saat içinde satışa sunulur.",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new FAQ
                {
                    Id = 2,
                    Question = "Kargo süresi ne kadar?",
                    Answer = "Siparişleriniz 1-3 iş günü içinde teslim edilir.",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new FAQ
                {
                    Id = 3,
                    Question = "Minimum sipariş tutarı var mı?",
                    Answer = "Evet, minimum sipariş tutarı 50 TL'dir.",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Gallery
            modelBuilder.Entity<Gallery>().HasData(
                new Gallery
                {
                    Id = 1,
                    Title = "Üretim Tesisi",
                    Description = "Modern üretim tesisimiz",
                    ImageUrl = "/img/about.jpg",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Gallery
                {
                    Id = 2,
                    Title = "Kalite Kontrol",
                    Description = "Kalite kontrol süreçlerimiz",
                    ImageUrl = "/img/product-2.jpg",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Gallery
                {
                    Id = 3,
                    Title = "Ürün Çeşitliliği",
                    Description = "Geniş ürün yelpazemiz",
                    ImageUrl = "/img/product-3.jpg",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Videos
            modelBuilder.Entity<Video>().HasData(
                new Video
                {
                    Id = 1,
                    Title = "Peynir Üretim Süreci",
                    Description = "Manyas'ta geleneksel peynir üretim sürecimizi detaylı olarak anlatan video",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                    ThumbnailUrl = "/img/video-thumb-1.jpg",
                    Duration = 180,
                    ViewCount = 1250,
                    IsActive = true,
                    IsFeatured = true,
                    DisplayOrder = 1,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Video
                {
                    Id = 2,
                    Title = "Kalite Kontrol Süreçleri",
                    Description = "Ürünlerimizin kalite kontrolünde uyguladığımız standartlar",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                    ThumbnailUrl = "/img/video-thumb-2.jpg",
                    Duration = 240,
                    ViewCount = 890,
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 2,
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Video
                {
                    Id = 3,
                    Title = "Müşteri Görüşleri",
                    Description = "Müşterilerimizin deneyimleri ve ürünlerimiz hakkındaki düşünceleri",
                    VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                    ThumbnailUrl = "/img/video-thumb-3.jpg",
                    Duration = 150,
                    ViewCount = 654,
                    IsActive = true,
                    IsFeatured = false,
                    DisplayOrder = 3,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}