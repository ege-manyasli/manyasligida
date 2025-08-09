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
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        // SİPARİŞ SİSTEMİ
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // OTURUM VE GÜVENLİK
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }

        // COOKIE CONSENT
        public DbSet<CookieConsent> CookieConsents { get; set; }
        public DbSet<CookieConsentDetail> CookieConsentDetails { get; set; }
        public DbSet<CookieCategory> CookieCategories { get; set; }

        // MUHASEBE
        public DbSet<Expense> Expenses { get; set; }

        // ABOUT CONTENT
        public DbSet<AboutContent> AboutContents { get; set; }

        // HOME CONTENT
        public DbSet<HomeContent> HomeContents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Password).IsRequired();
            });

            // Product Configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Price).HasPrecision(18, 2).IsRequired();
                entity.HasOne(e => e.Category).WithMany(e => e.Products).HasForeignKey(e => e.CategoryId);
            });

            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.User).WithMany(e => e.Orders).HasForeignKey(e => e.UserId);
            });

            // UserSession Configuration
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SessionId).IsUnique();
                entity.Property(e => e.SessionId).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
            });
        }
    }
}
