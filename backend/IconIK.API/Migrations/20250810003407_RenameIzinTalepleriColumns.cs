using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameIzinTalepleriColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "bitis_tarihi",
                table: "izin_talepleri",
                newName: "izin_baslama_tarihi");

            migrationBuilder.RenameColumn(
                name: "baslangic_tarihi",
                table: "izin_talepleri",
                newName: "isbasi_tarihi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "izin_baslama_tarihi",
                table: "izin_talepleri",
                newName: "bitis_tarihi");

            migrationBuilder.RenameColumn(
                name: "isbasi_tarihi",
                table: "izin_talepleri",
                newName: "baslangic_tarihi");
        }
    }
}
