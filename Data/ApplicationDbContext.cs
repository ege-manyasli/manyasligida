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
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<FAQ> FAQs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product - Category relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Order - User relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            // Order - OrderItems relationship
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            // Cart - User relationship
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId);

            // Cart - CartItems relationship
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Beyaz Peynir", Description = "Geleneksel beyaz peynir çeşitleri" },
                new Category { Id = 2, Name = "Kaşar Peyniri", Description = "Kaşar peyniri çeşitleri" },
                new Category { Id = 3, Name = "Özel Peynirler", Description = "Özel üretim peynir çeşitleri" }
            );

            // Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Ezine Tipi Sert Beyaz Peynir",
                    Description = "Ezine bölgesinin özel iklim koşullarında üretilen sert beyaz peynir",
                    Price = 45.50m,
                    OldPrice = 55.00m,
                    StockQuantity = 100,
                    CategoryId = 1,
                    ImageUrl = "/img/ezine-tipi-sert-beyaz-peynir-650-gr.-52d9.jpg",
                    IsPopular = true,
                    IsNew = false,
                    Weight = "650 gr",
                    FatContent = "%45",
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Product
                {
                    Id = 2,
                    Name = "Taze Kaşar Peyniri",
                    Description = "Taze ve yumuşak kaşar peyniri",
                    Price = 85.00m,
                    OldPrice = 95.00m,
                    StockQuantity = 75,
                    CategoryId = 2,
                    ImageUrl = "/img/taze-kasar-peyniri-1000-gr.-63a593.jpg",
                    IsPopular = true,
                    IsNew = true,
                    Weight = "1000 gr",
                    FatContent = "%50",
                    CreatedAt = new DateTime(2024, 1, 2)
                },
                new Product
                {
                    Id = 3,
                    Name = "Eski Kaşar Peyniri",
                    Description = "Olgunlaştırılmış eski kaşar peyniri",
                    Price = 120.00m,
                    OldPrice = 140.00m,
                    StockQuantity = 50,
                    CategoryId = 2,
                    ImageUrl = "/img/eski-kasar-peyniri-300-g.-6a1-d2.jpg",
                    IsPopular = false,
                    IsNew = false,
                    Weight = "300 gr",
                    FatContent = "%55",
                    CreatedAt = new DateTime(2024, 1, 3)
                },
                new Product
                {
                    Id = 4,
                    Name = "Dil Peyniri",
                    Description = "Geleneksel dil peyniri",
                    Price = 65.00m,
                    OldPrice = 75.00m,
                    StockQuantity = 80,
                    CategoryId = 3,
                    ImageUrl = "/img/dil-peyniri-400-gr.-5f1a.jpg",
                    IsPopular = false,
                    IsNew = true,
                    Weight = "400 gr",
                    FatContent = "%40",
                    CreatedAt = new DateTime(2024, 1, 4)
                },
                new Product
                {
                    Id = 5,
                    Name = "Biberli Sepet Peyniri",
                    Description = "Biber ile tatlandırılmış özel sepet peyniri",
                    Price = 55.00m,
                    OldPrice = 65.00m,
                    StockQuantity = 60,
                    CategoryId = 3,
                    ImageUrl = "/img/biberli-sepet-peyniri-350-gr.-2e1f.jpg",
                    IsPopular = true,
                    IsNew = false,
                    Weight = "350 gr",
                    FatContent = "%42",
                    CreatedAt = new DateTime(2024, 1, 5)
                },
                new Product
                {
                    Id = 6,
                    Name = "Çörek Otlu Sepet Peyniri",
                    Description = "Çörek otu ile zenginleştirilmiş sepet peyniri",
                    Price = 58.00m,
                    OldPrice = 68.00m,
                    StockQuantity = 70,
                    CategoryId = 3,
                    ImageUrl = "/img/corek-otlu-sepet-peyniri-350-gr.-8c96.jpg",
                    IsPopular = false,
                    IsNew = true,
                    Weight = "350 gr",
                    FatContent = "%43",
                    CreatedAt = new DateTime(2024, 1, 6)
                },
                new Product
                {
                    Id = 7,
                    Name = "Mihaliç Peyniri",
                    Description = "Geleneksel Mihaliç peyniri",
                    Price = 72.00m,
                    OldPrice = 82.00m,
                    StockQuantity = 45,
                    CategoryId = 1,
                    ImageUrl = "/img/mihalic-peyniri-350-gr.-122f.jpg",
                    IsPopular = false,
                    IsNew = false,
                    Weight = "350 gr",
                    FatContent = "%48",
                    CreatedAt = new DateTime(2024, 1, 7)
                }
            );
        }
    }
} 