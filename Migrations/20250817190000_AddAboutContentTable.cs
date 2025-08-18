using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;

#nullable disable

namespace manyasligida.Migrations
{
    /// <inheritdoc />
    public partial class AddAboutContentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AboutContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Subtitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StoryTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StorySubtitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StoryContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StoryImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MissionTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MissionContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VisionTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VisionContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValuesTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValuesSubtitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValuesContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueItems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductionTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProductionSubtitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProductionSteps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificatesTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CertificatesSubtitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CertificateItems = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RegionSubtitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RegionContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegionImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegionFeatures = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CtaTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CtaContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CtaButtonText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CtaSecondButtonText = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StoryFeatures = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AboutContents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AboutContents");
        }
    }
}
