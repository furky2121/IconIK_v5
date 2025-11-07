using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddKalanMiktarToZimmetStok : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "kalan_miktar",
                table: "zimmet_stok",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "kalan_miktar",
                table: "zimmet_stok");
        }
    }
}
