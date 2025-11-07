using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class FixIzinTalepleriColumnMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Geçici bir sütun oluþtur
            migrationBuilder.AddColumn<DateTime>(
                name: "temp_column",
                table: "izin_talepleri",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // izin_baslama_tarihi (eski bitis_tarihi) verilerini geçici sütuna kopyala
            migrationBuilder.Sql("UPDATE izin_talepleri SET temp_column = izin_baslama_tarihi");

            // izin_baslama_tarihi'ni isbasi_tarihi (eski baslangic_tarihi) ile deðiþtir
            migrationBuilder.Sql("UPDATE izin_talepleri SET izin_baslama_tarihi = isbasi_tarihi");

            // isbasi_tarihi'ni geçici sütundaki verilerle deðiþtir
            migrationBuilder.Sql("UPDATE izin_talepleri SET isbasi_tarihi = temp_column");

            // Geçici sütunu sil
            migrationBuilder.DropColumn(
                name: "temp_column",
                table: "izin_talepleri");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Geri alma iþlemi - sütunlarý tekrar deðiþtir
            migrationBuilder.AddColumn<DateTime>(
                name: "temp_column",
                table: "izin_talepleri",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("UPDATE izin_talepleri SET temp_column = izin_baslama_tarihi");
            migrationBuilder.Sql("UPDATE izin_talepleri SET izin_baslama_tarihi = isbasi_tarihi");
            migrationBuilder.Sql("UPDATE izin_talepleri SET isbasi_tarihi = temp_column");

            migrationBuilder.DropColumn(
                name: "temp_column",
                table: "izin_talepleri");
        }
    }
}
