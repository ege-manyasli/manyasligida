using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace manyasligida.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecisionCorrect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Blogs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FAQs",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Galleries",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Galleries",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Galleries",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RatingCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RatingCount",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "Blogs",
                columns: new[] { "Id", "Author", "Content", "CreatedAt", "ImageUrl", "IsActive", "PublishedAt", "Summary", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, "Peynir üretimi, binlerce yıllık bir geleneğe sahiptir. Manyas'ta bu geleneksel yöntemler modern teknoloji ile birleştirilerek en kaliteli peynirler üretilmektedir...", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/img/blog-1.jpg", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Manyas'ta peynir üretiminde kullanılan geleneksel yöntemler ve modern teknolojinin uyumu hakkında detaylı bilgi.", "Peynir Üretiminde Geleneksel Yöntemler", null },
                    { 2, null, "Peynir, protein, kalsiyum ve diğer önemli besin maddelerini içeren değerli bir gıda maddesidir. Düzenli tüketimi kemik sağlığı ve kas gelişimi için önemlidir...", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/img/blog-2.jpg", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Peynirin besin değeri ve sağlıklı beslenmedeki rolü hakkında uzman görüşleri.", "Sağlıklı Beslenmede Peynirin Önemi", null },
                    { 3, null, "Her peynir çeşidinin kendine özgü lezzeti ve kullanım alanı vardır. Beyaz peynir kahvaltı sofralarının vazgeçilmezi iken, kaşar peyniri ısıtıldığında eriyen yapısıyla pizza ve tost için idealdir...", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/img/blog-3.jpg", true, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Farklı peynir çeşitlerinin özellikleri ve mutfakta nasıl kullanılacağı hakkında pratik bilgiler.", "Peynir Çeşitleri ve Kullanım Alanları", null }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Geleneksel beyaz peynir çeşitleri", 1, null, true, "Beyaz Peynir", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Taze ve eski kaşar peynirleri", 2, null, true, "Kaşar Peyniri", null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Özel sepet peynirleri", 3, null, true, "Sepet Peyniri", null },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Özel üretim peynir çeşitleri", 4, null, true, "Özel Peynirler", null }
                });

            migrationBuilder.InsertData(
                table: "FAQs",
                columns: new[] { "Id", "Answer", "Category", "CreatedAt", "DisplayOrder", "IsActive", "Question", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Tüm peynirlerimiz günlük üretimdir ve en fazla 24 saat içinde satışa sunulur.", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, true, "Peynirleriniz ne kadar taze?", null },
                    { 2, "Siparişleriniz 1-3 iş günü içinde teslim edilir.", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, true, "Kargo süresi ne kadar?", null },
                    { 3, "Evet, minimum sipariş tutarı 50 TL'dir.", null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, true, "Minimum sipariş tutarı var mı?", null }
                });

            migrationBuilder.InsertData(
                table: "Galleries",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "IsActive", "ThumbnailUrl", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Modern üretim tesisimiz", 0, "/img/about.jpg", true, null, "Üretim Tesisi", null },
                    { 2, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kalite kontrol süreçlerimiz", 0, "/img/product-2.jpg", true, null, "Kalite Kontrol", null },
                    { 3, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Geniş ürün yelpazemiz", 0, "/img/product-3.jpg", true, null, "Ürün Çeşitliliği", null }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AllergenInfo", "CategoryId", "CreatedAt", "Description", "ExpiryInfo", "FatContent", "ImageUrl", "ImageUrls", "Ingredients", "IsActive", "IsFeatured", "IsNew", "IsPopular", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "NutritionalInfo", "OldPrice", "Price", "PublishedAt", "Slug", "SortOrder", "StockQuantity", "StorageInfo", "ThumbnailUrl", "UpdatedAt", "Weight" },
                values: new object[,]
                {
                    { 1, null, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Çanakkale Ezine'den özel üretim tam yağlı beyaz peynir", null, "%45-50", "/img/ezine-tipi-sert-beyaz-peynir-650-gr.-52d9.jpg", null, null, true, false, true, true, null, null, null, "Ezine Beyaz Peynir", null, 55.00m, 45.00m, null, null, 0, 100, null, null, null, "600 Gr" },
                    { 2, null, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Günlük taze üretim yumuşak kaşar peyniri", null, "%40-45", "/img/taze-kasar-peyniri-1000-gr.-63a593.jpg", null, null, true, false, false, true, null, null, null, "Taze Kaşar Peyniri", null, null, 35.00m, null, null, 0, 75, null, null, null, "1000 Gr" },
                    { 3, null, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Geleneksel Mihaliç peyniri", null, "%35-40", "/img/mihalic-peyniri-350-gr.-122f.jpg", null, null, true, false, true, false, null, null, null, "Mihaliç Peyniri", null, 35.00m, 28.00m, null, null, 0, 50, null, null, null, "350 Gr" },
                    { 4, null, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Çörek otu ile aromalandırılmış özel sepet peyniri", null, "%42-47", "/img/corek-otlu-sepet-peyniri-350-gr.-8c96.jpg", null, null, true, false, false, false, null, null, null, "Çörek Otlu Sepet Peyniri", null, null, 32.00m, null, null, 0, 60, null, null, null, "350 Gr" },
                    { 5, null, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kırmızı biber ile tatlandırılmış sepet peyniri", null, "%40-45", "/img/biberli-sepet-peyniri-350-gr.-2e1f.jpg", null, null, true, false, false, true, null, null, null, "Biberli Sepet Peyniri", null, 38.00m, 30.00m, null, null, 0, 40, null, null, null, "350 Gr" },
                    { 6, null, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Olgunlaştırılmış eski kaşar peyniri", null, "%45-50", "/img/eski-kasar-peyniri-300-g.-6a1-d2.jpg", null, null, true, false, false, false, null, null, null, "Eski Kars Kaşarı", null, null, 42.00m, null, null, 0, 30, null, null, null, "300 Gr" },
                    { 7, null, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yumuşak ve lezzetli dil peyniri", null, "%35-40", "/img/dil-peyniri-400-gr.-5f1a.jpg", null, null, true, false, true, false, null, null, null, "Dil Peyniri", null, 32.00m, 25.00m, null, null, 0, 80, null, null, null, "400 Gr" },
                    { 8, null, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Taze otlar ile aromalandırılmış beyaz peynir", null, "%42-47", "/img/product-1.jpg", null, null, true, false, false, true, null, null, null, "Otlu Beyaz Peynir", null, null, 38.00m, null, null, 0, 55, null, null, null, "500 Gr" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
