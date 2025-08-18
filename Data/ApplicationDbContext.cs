using Microsoft.EntityFrameworkCore;
using manyasligida.Models;

namespace manyasligida.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // ANA TABLOLAR
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        // SİPARİŞ SİSTEMİ
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // GÜVENLİK VE DOĞRULAMA
        public DbSet<EmailVerification> EmailVerifications { get; set; }

        // COOKIE CONSENT
        public DbSet<CookieConsent> CookieConsents { get; set; }
        public DbSet<CookieConsentDetail> CookieConsentDetails { get; set; }
        public DbSet<CookieCategory> CookieCategories { get; set; }

        // MUHASEBE
        public DbSet<Expense> Expenses { get; set; }

        // STOK YÖNETİMİ
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryStock> InventoryStocks { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        // ABOUT CONTENT
        public DbSet<AboutContent> AboutContents { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        // HOME CONTENT
        public DbSet<HomeContent> HomeContents { get; set; }
        
        // SITE SETTINGS
        public DbSet<SiteSettings> SiteSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Password).IsRequired();
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.PostalCode).HasMaxLength(10);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsAdmin).HasDefaultValue(false);
                entity.Property(e => e.EmailConfirmed).HasDefaultValue(false);
            });

            // EmailVerification configuration
            modelBuilder.Entity<EmailVerification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.VerificationCode).IsRequired().HasMaxLength(10);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.IsUsed).HasDefaultValue(false);
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ImageUrl).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(e => e.Category).WithMany(c => c.Products).HasForeignKey(e => e.CategoryId);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.OrderDate).IsRequired();
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ShippingCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TaxAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.OrderStatus).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.User).WithMany(u => u.Orders).HasForeignKey(e => e.UserId);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Order).WithMany(o => o.OrderItems).HasForeignKey(e => e.OrderId);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            });

            // Cart configuration
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasOne(e => e.User).WithMany(u => u.Carts).HasForeignKey(e => e.UserId);
            });

            // CartItem configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).IsRequired();
                entity.HasOne(e => e.Cart).WithMany(c => c.CartItems).HasForeignKey(e => e.CartId);
                entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
            });

            // Blog configuration
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Gallery configuration
            modelBuilder.Entity<Gallery>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Video configuration
            modelBuilder.Entity<Video>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.VideoUrl).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // ContactMessage configuration
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsRead).HasDefaultValue(false);
            });

            // CookieConsent configuration
            modelBuilder.Entity<CookieConsent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionId).HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.ConsentDate).IsRequired();
                entity.Property(e => e.ExpiryDate).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsAccepted).IsRequired();
                entity.Property(e => e.Preferences).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });

            // CookieConsentDetail configuration
            modelBuilder.Entity<CookieConsentDetail>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CookieConsentId).IsRequired();
                entity.Property(e => e.CookieCategoryId).IsRequired();
                entity.Property(e => e.IsAccepted).IsRequired();
                entity.HasOne(e => e.CookieConsent).WithMany(cc => cc.CookieConsentDetails).HasForeignKey(e => e.CookieConsentId);
                entity.HasOne(e => e.CookieCategory).WithMany().HasForeignKey(e => e.CookieCategoryId);
            });

            // CookieCategory configuration
            modelBuilder.Entity<CookieCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.IsRequired).HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // Expense configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ExpenseDate).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // AboutContent configuration
            modelBuilder.Entity<AboutContent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subtitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.StoryTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.StorySubtitle).HasMaxLength(200);
                entity.Property(e => e.StoryContent).IsRequired();
                entity.Property(e => e.StoryImageUrl).HasMaxLength(500);
                entity.Property(e => e.MissionTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MissionContent).IsRequired();
                entity.Property(e => e.VisionTitle).IsRequired().HasMaxLength(200);
                entity.Property(e => e.VisionContent).IsRequired();
                entity.Property(e => e.ValuesTitle).HasMaxLength(200);
                entity.Property(e => e.ValuesSubtitle).HasMaxLength(200);
                entity.Property(e => e.ValuesContent);
                entity.Property(e => e.ValueItems);
                entity.Property(e => e.ProductionTitle).HasMaxLength(200);
                entity.Property(e => e.ProductionSubtitle).HasMaxLength(200);
                entity.Property(e => e.ProductionSteps);
                entity.Property(e => e.CertificatesTitle).HasMaxLength(200);
                entity.Property(e => e.CertificatesSubtitle).HasMaxLength(200);
                entity.Property(e => e.CertificateItems);
                entity.Property(e => e.RegionTitle).HasMaxLength(200);
                entity.Property(e => e.RegionSubtitle).HasMaxLength(200);
                entity.Property(e => e.RegionContent);
                entity.Property(e => e.RegionImageUrl).HasMaxLength(500);
                entity.Property(e => e.RegionFeatures);
                entity.Property(e => e.CtaTitle).HasMaxLength(200);
                entity.Property(e => e.CtaContent);
                entity.Property(e => e.CtaButtonText).HasMaxLength(100);
                entity.Property(e => e.CtaSecondButtonText).HasMaxLength(100);
                entity.Property(e => e.StoryFeatures);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // HomeContent configuration
            modelBuilder.Entity<HomeContent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // SiteSettings configuration
            modelBuilder.Entity<SiteSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Phone).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.Address).IsRequired();
                entity.Property(e => e.WorkingHours).IsRequired();
                entity.Property(e => e.SiteTitle).IsRequired();
                entity.Property(e => e.SiteDescription).IsRequired();
                entity.Property(e => e.SiteKeywords).IsRequired();
                entity.Property(e => e.LogoUrl).HasMaxLength(200);
                entity.Property(e => e.FaviconUrl).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            // UserSession configuration
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserAgent).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.DeviceInfo).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.LastActivity).IsRequired();
                entity.Property(e => e.ExpiresAt);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ExpiresAt);
            });
        }
    }
}
