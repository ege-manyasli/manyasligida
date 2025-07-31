using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace manyasligida.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Products_ProductId1",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductId1",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductId1",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ProductId1",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "CartItems");

            migrationBuilder.AddColumn<string>(
                name: "AllergenInfo",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpiryInfo",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ingredients",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NutritionalInfo",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "Products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StorageInfo",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "Blogs",
                columns: new[] { "Id", "Author", "Content", "CreatedAt", "ImageUrl", "IsActive", "PublishedAt", "Summary", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, "Peynir üretimi, binlerce yıllık bir geleneğe sahiptir. Manyas'ta bu geleneksel yöntemler modern teknoloji ile birleştirilerek en kaliteli peynirler üretilmektedir...", new DateTime(2025, 7, 26, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(4843), "/img/blog-1.jpg", true, new DateTime(2025, 7, 26, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(4460), "Manyas'ta peynir üretiminde kullanılan geleneksel yöntemler ve modern teknolojinin uyumu hakkında detaylı bilgi.", "Peynir Üretiminde Geleneksel Yöntemler", null },
                    { 2, null, "Peynir, protein, kalsiyum ve diğer önemli besin maddelerini içeren değerli bir gıda maddesidir. Düzenli tüketimi kemik sağlığı ve kas gelişimi için önemlidir...", new DateTime(2025, 7, 21, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(5065), "/img/blog-2.jpg", true, new DateTime(2025, 7, 21, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(5063), "Peynirin besin değeri ve sağlıklı beslenmedeki rolü hakkında uzman görüşleri.", "Sağlıklı Beslenmede Peynirin Önemi", null },
                    { 3, null, "Her peynir çeşidinin kendine özgü lezzeti ve kullanım alanı vardır. Beyaz peynir kahvaltı sofralarının vazgeçilmezi iken, kaşar peyniri ısıtıldığında eriyen yapısıyla pizza ve tost için idealdir...", new DateTime(2025, 7, 16, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(5070), "/img/blog-3.jpg", true, new DateTime(2025, 7, 16, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(5069), "Farklı peynir çeşitlerinin özellikleri ve mutfakta nasıl kullanılacağı hakkında pratik bilgiler.", "Peynir Çeşitleri ve Kullanım Alanları", null }
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ImageUrl" },
                values: new object[] { new DateTime(2025, 7, 31, 16, 13, 46, 546, DateTimeKind.Local).AddTicks(7414), null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "ImageUrl" },
                values: new object[] { new DateTime(2025, 7, 31, 16, 13, 46, 546, DateTimeKind.Local).AddTicks(7868), "Taze ve eski kaşar peynirleri", null });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "ImageUrl", "Name" },
                values: new object[] { new DateTime(2025, 7, 31, 16, 13, 46, 546, DateTimeKind.Local).AddTicks(7871), "Özel sepet peynirleri", null, "Sepet Peyniri" });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "IsActive", "Name", "UpdatedAt" },
                values: new object[] { 4, new DateTime(2025, 7, 31, 16, 13, 46, 546, DateTimeKind.Local).AddTicks(7874), "Özel üretim peynir çeşitleri", 4, null, true, "Özel Peynirler", null });

            migrationBuilder.InsertData(
                table: "FAQs",
                columns: new[] { "Id", "Answer", "Category", "CreatedAt", "DisplayOrder", "IsActive", "Question", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Tüm peynirlerimiz günlük üretimdir ve en fazla 24 saat içinde satışa sunulur.", null, new DateTime(2025, 7, 31, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(7047), 1, true, "Peynirleriniz ne kadar taze?", null },
                    { 2, "Siparişleriniz 1-3 iş günü içinde teslim edilir.", null, new DateTime(2025, 7, 31, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(7272), 2, true, "Kargo süresi ne kadar?", null },
                    { 3, "Evet, minimum sipariş tutarı 50 TL'dir.", null, new DateTime(2025, 7, 31, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(7275), 3, true, "Minimum sipariş tutarı var mı?", null }
                });

            migrationBuilder.InsertData(
                table: "Galleries",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "DisplayOrder", "ImageUrl", "IsActive", "ThumbnailUrl", "Title", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2025, 7, 31, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(9002), "Modern üretim tesisimiz", 0, "/img/about.jpg", true, null, "Üretim Tesisi", null },
                    { 2, null, new DateTime(2025, 7, 31, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(9302), "Kalite kontrol süreçlerimiz", 0, "/img/product-2.jpg", true, null, "Kalite Kontrol", null },
                    { 3, null, new DateTime(2025, 7, 31, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(9304), "Geniş ürün yelpazemiz", 0, "/img/product-3.jpg", true, null, "Ürün Çeşitliliği", null }
                });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AllergenInfo", "CreatedAt", "Description", "ExpiryInfo", "FatContent", "ImageUrls", "Ingredients", "IsFeatured", "IsNew", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "NutritionalInfo", "Price", "PublishedAt", "Slug", "SortOrder", "StorageInfo", "ThumbnailUrl", "Weight" },
                values: new object[] { null, new DateTime(2025, 7, 26, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(1718), "Çanakkale Ezine'den özel üretim tam yağlı beyaz peynir", null, "%45-50", null, null, false, true, null, null, null, "Ezine Beyaz Peynir", null, 45.00m, null, null, 0, null, null, "600 Gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AllergenInfo", "CreatedAt", "Description", "ExpiryInfo", "FatContent", "ImageUrls", "Ingredients", "IsFeatured", "IsNew", "MetaDescription", "MetaKeywords", "MetaTitle", "NutritionalInfo", "OldPrice", "Price", "PublishedAt", "Slug", "SortOrder", "StorageInfo", "ThumbnailUrl", "Weight" },
                values: new object[] { null, new DateTime(2025, 7, 21, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(2060), "Günlük taze üretim yumuşak kaşar peyniri", null, "%40-45", null, null, false, false, null, null, null, null, null, 35.00m, null, null, 0, null, null, "1000 Gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AllergenInfo", "CategoryId", "CreatedAt", "Description", "ExpiryInfo", "FatContent", "ImageUrl", "ImageUrls", "Ingredients", "IsFeatured", "IsNew", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "NutritionalInfo", "OldPrice", "Price", "PublishedAt", "Slug", "SortOrder", "StorageInfo", "ThumbnailUrl", "Weight" },
                values: new object[] { null, 4, new DateTime(2025, 7, 28, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(2069), "Geleneksel Mihaliç peyniri", null, "%35-40", "/img/mihalic-peyniri-350-gr.-122f.jpg", null, null, false, true, null, null, null, "Mihaliç Peyniri", null, 35.00m, 28.00m, null, null, 0, null, null, "350 Gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "AllergenInfo", "CreatedAt", "Description", "ExpiryInfo", "FatContent", "ImageUrl", "ImageUrls", "Ingredients", "IsFeatured", "IsNew", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "NutritionalInfo", "OldPrice", "Price", "PublishedAt", "Slug", "SortOrder", "StockQuantity", "StorageInfo", "ThumbnailUrl", "Weight" },
                values: new object[] { null, new DateTime(2025, 7, 16, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(2102), "Çörek otu ile aromalandırılmış özel sepet peyniri", null, "%42-47", "/img/corek-otlu-sepet-peyniri-350-gr.-8c96.jpg", null, null, false, false, null, null, null, "Çörek Otlu Sepet Peyniri", null, null, 32.00m, null, null, 0, 60, null, null, "350 Gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "AllergenInfo", "CreatedAt", "Description", "ExpiryInfo", "FatContent", "ImageUrls", "Ingredients", "IsFeatured", "MetaDescription", "MetaKeywords", "MetaTitle", "NutritionalInfo", "OldPrice", "Price", "PublishedAt", "Slug", "SortOrder", "StockQuantity", "StorageInfo", "ThumbnailUrl", "Weight" },
                values: new object[] { null, new DateTime(2025, 7, 11, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(2107), "Kırmızı biber ile tatlandırılmış sepet peyniri", null, "%40-45", null, null, false, null, null, null, null, 38.00m, 30.00m, null, null, 0, 40, null, null, "350 Gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "AllergenInfo", "CategoryId", "CreatedAt", "Description", "ExpiryInfo", "FatContent", "ImageUrl", "ImageUrls", "Ingredients", "IsFeatured", "IsNew", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "NutritionalInfo", "OldPrice", "Price", "PublishedAt", "Slug", "SortOrder", "StockQuantity", "StorageInfo", "ThumbnailUrl", "Weight" },
                values: new object[] { null, 2, new DateTime(2025, 7, 6, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(2114), "Olgunlaştırılmış eski kaşar peyniri", null, "%45-50", "/img/eski-kasar-peyniri-300-g.-6a1-d2.jpg", null, null, false, false, null, null, null, "Eski Kars Kaşarı", null, null, 42.00m, null, null, 0, 30, null, null, "300 Gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "AllergenInfo", "CreatedAt", "Description", "ExpiryInfo", "FatContent", "ImageUrl", "ImageUrls", "Ingredients", "IsFeatured", "IsNew", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "NutritionalInfo", "OldPrice", "Price", "PublishedAt", "Slug", "SortOrder", "StockQuantity", "StorageInfo", "ThumbnailUrl", "Weight" },
                values: new object[] { null, new DateTime(2025, 7, 29, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(2122), "Yumuşak ve lezzetli dil peyniri", null, "%35-40", "/img/dil-peyniri-400-gr.-5f1a.jpg", null, null, false, true, null, null, null, "Dil Peyniri", null, 32.00m, 25.00m, null, null, 0, 80, null, null, "400 Gr" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AllergenInfo", "CategoryId", "CreatedAt", "Description", "ExpiryInfo", "FatContent", "ImageUrl", "ImageUrls", "Ingredients", "IsActive", "IsFeatured", "IsNew", "IsPopular", "MetaDescription", "MetaKeywords", "MetaTitle", "Name", "NutritionalInfo", "OldPrice", "Price", "PublishedAt", "Slug", "SortOrder", "StockQuantity", "StorageInfo", "ThumbnailUrl", "UpdatedAt", "Weight" },
                values: new object[] { 8, null, 1, new DateTime(2025, 7, 19, 16, 13, 46, 548, DateTimeKind.Local).AddTicks(2127), "Taze otlar ile aromalandırılmış beyaz peynir", null, "%42-47", "/img/product-1.jpg", null, null, true, false, false, true, null, null, null, "Otlu Beyaz Peynir", null, null, 38.00m, null, null, 0, 55, null, null, null, "500 Gr" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

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
                keyValue: 8);

            migrationBuilder.DropColumn(
                name: "AllergenInfo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ExpiryInfo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Ingredients",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "NutritionalInfo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StorageInfo",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Blogs");

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "OrderItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "ImageUrl" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "/img/category-beyaz.jpg" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "ImageUrl" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Kaşar peyniri çeşitleri", "/img/category-kasar.jpg" });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "ImageUrl", "Name" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Özel üretim peynir çeşitleri", "/img/category-ozel.jpg", "Özel Peynirler" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Description", "FatContent", "IsNew", "Name", "Price", "Weight" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ezine bölgesinin özel iklim koşullarında üretilen sert beyaz peynir", "%45", false, "Ezine Tipi Sert Beyaz Peynir", 45.50m, "650 gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "FatContent", "IsNew", "OldPrice", "Price", "Weight" },
                values: new object[] { new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Taze ve yumuşak kaşar peyniri", "%50", true, 95.00m, 85.00m, "1000 gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CategoryId", "CreatedAt", "Description", "FatContent", "ImageUrl", "IsNew", "Name", "OldPrice", "Price", "Weight" },
                values: new object[] { 2, new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Olgunlaştırılmış eski kaşar peyniri", "%55", "/img/eski-kasar-peyniri-300-g.-6a1-d2.jpg", false, "Eski Kaşar Peyniri", 140.00m, 120.00m, "300 gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Description", "FatContent", "ImageUrl", "IsNew", "Name", "OldPrice", "Price", "StockQuantity", "Weight" },
                values: new object[] { new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), "Geleneksel dil peyniri", "%40", "/img/dil-peyniri-400-gr.-5f1a.jpg", true, "Dil Peyniri", 75.00m, 65.00m, 80, "400 gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "Description", "FatContent", "OldPrice", "Price", "StockQuantity", "Weight" },
                values: new object[] { new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Biber ile tatlandırılmış özel sepet peyniri", "%42", 65.00m, 55.00m, 60, "350 gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CategoryId", "CreatedAt", "Description", "FatContent", "ImageUrl", "IsNew", "Name", "OldPrice", "Price", "StockQuantity", "Weight" },
                values: new object[] { 3, new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Çörek otu ile zenginleştirilmiş sepet peyniri", "%43", "/img/corek-otlu-sepet-peyniri-350-gr.-8c96.jpg", true, "Çörek Otlu Sepet Peyniri", 68.00m, 58.00m, 70, "350 gr" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "Description", "FatContent", "ImageUrl", "IsNew", "Name", "OldPrice", "Price", "StockQuantity", "Weight" },
                values: new object[] { new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "Geleneksel Mihaliç peyniri", "%48", "/img/mihalic-peyniri-350-gr.-122f.jpg", false, "Mihaliç Peyniri", 82.00m, 72.00m, 45, "350 gr" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId1",
                table: "OrderItems",
                column: "ProductId1");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductId1",
                table: "CartItems",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Products_ProductId1",
                table: "CartItems",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductId1",
                table: "OrderItems",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id");
        }
    }
}
