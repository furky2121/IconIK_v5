using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class VideoIzlemeEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IzlemeBaslangicSaniye",
                table: "VideoIzlemeler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IzlemeBitisSaniye",
                table: "VideoIzlemeler",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VideoPlatform",
                table: "VideoIzlemeler",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VideoToplamSure",
                table: "VideoIzlemeler",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IzlemeBaslangicSaniye",
                table: "VideoIzlemeler");

            migrationBuilder.DropColumn(
                name: "IzlemeBitisSaniye",
                table: "VideoIzlemeler");

            migrationBuilder.DropColumn(
                name: "VideoPlatform",
                table: "VideoIzlemeler");

            migrationBuilder.DropColumn(
                name: "VideoToplamSure",
                table: "VideoIzlemeler");
        }
    }
}
