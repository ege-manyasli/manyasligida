using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace manyasligida.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlsToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Product tablosuna ImageUrls kolonunu ekle
            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            // Eğer GalleryImageUrls kolonu varsa onu kaldır
            migrationBuilder.DropColumn(
                name: "GalleryImageUrls",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ImageUrls kolonunu kaldır
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "Products");

            // GalleryImageUrls kolonunu geri ekle
            migrationBuilder.AddColumn<string>(
                name: "GalleryImageUrls",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
