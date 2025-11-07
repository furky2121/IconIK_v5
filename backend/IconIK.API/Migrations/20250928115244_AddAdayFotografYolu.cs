using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdayFotografYolu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fotograf_yolu",
                table: "adaylar",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fotograf_yolu",
                table: "adaylar");
        }
    }
}
