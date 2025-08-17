using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace manyasligida.Migrations
{
    /// <inheritdoc />
    public partial class FixCookieConsentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CookieConsent tablosuna eksik kolonları ekle
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CookieConsents",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CookieConsents",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "CookieConsents",
                type: "datetime2",
                nullable: true);

            // CookieConsentDetails tablosuna eksik kolonları ekle
            migrationBuilder.AddColumn<int>(
                name: "CookieCategoryId",
                table: "CookieConsentDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CookieConsentId",
                table: "CookieConsentDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CookieConsentDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            // CookieCategories tablosuna eksik kolonları ekle
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "CookieCategories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CookieCategories",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            // Foreign key constraints ekle
            migrationBuilder.CreateIndex(
                name: "IX_CookieConsentDetails_CookieCategoryId",
                table: "CookieConsentDetails",
                column: "CookieCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CookieConsentDetails_CookieConsentId",
                table: "CookieConsentDetails",
                column: "CookieConsentId");

            migrationBuilder.AddForeignKey(
                name: "FK_CookieConsentDetails_CookieCategories_CookieCategoryId",
                table: "CookieConsentDetails",
                column: "CookieCategoryId",
                principalTable: "CookieCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CookieConsentDetails_CookieConsents_CookieConsentId",
                table: "CookieConsentDetails",
                column: "CookieConsentId",
                principalTable: "CookieConsents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CookieConsentDetails_CookieCategories_CookieCategoryId",
                table: "CookieConsentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_CookieConsentDetails_CookieConsents_CookieConsentId",
                table: "CookieConsentDetails");

            migrationBuilder.DropIndex(
                name: "IX_CookieConsentDetails_CookieCategoryId",
                table: "CookieConsentDetails");

            migrationBuilder.DropIndex(
                name: "IX_CookieConsentDetails_CookieConsentId",
                table: "CookieConsentDetails");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CookieConsents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CookieConsents");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "CookieConsents");

            migrationBuilder.DropColumn(
                name: "CookieCategoryId",
                table: "CookieConsentDetails");

            migrationBuilder.DropColumn(
                name: "CookieConsentId",
                table: "CookieConsentDetails");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CookieConsentDetails");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "CookieCategories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CookieCategories");
        }
    }
}
