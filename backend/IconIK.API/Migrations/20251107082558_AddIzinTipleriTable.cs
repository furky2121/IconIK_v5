using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIzinTipleriTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IzinTipiId",
                table: "izin_talepleri",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "izin_tipleri",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    izin_tipi_adi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    standart_gun_sayisi = table.Column<int>(type: "integer", nullable: true),
                    minimum_gun_sayisi = table.Column<int>(type: "integer", nullable: true),
                    maksimum_gun_sayisi = table.Column<int>(type: "integer", nullable: true),
                    cinsiyet_kisiti = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    rapor_gerekli = table.Column<bool>(type: "boolean", nullable: false),
                    ucretli_mi = table.Column<bool>(type: "boolean", nullable: false),
                    renk = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    sira = table.Column<int>(type: "integer", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izin_tipleri", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_izin_talepleri_IzinTipiId",
                table: "izin_talepleri",
                column: "IzinTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_izin_tipleri_izin_tipi_adi",
                table: "izin_tipleri",
                column: "izin_tipi_adi",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_izin_talepleri_izin_tipleri_IzinTipiId",
                table: "izin_talepleri",
                column: "IzinTipiId",
                principalTable: "izin_tipleri",
                principalColumn: "id");

            // Seed default leave types
            migrationBuilder.InsertData(
                table: "izin_tipleri",
                columns: new[] { "id", "izin_tipi_adi", "standart_gun_sayisi", "minimum_gun_sayisi", "maksimum_gun_sayisi", "cinsiyet_kisiti", "rapor_gerekli", "ucretli_mi", "renk", "aciklama", "sira", "aktif", "created_at" },
                values: new object[,]
                {
                    { 1, "Yýllýk Ýzin", 14, 14, 26, null, false, true, "#4CAF50", "Çalýþma süresine göre belirlenen yýllýk izin hakký", 1, true, DateTime.UtcNow },
                    { 2, "Mazeret Ýzni", 3, 1, 7, null, false, true, "#2196F3", "Evlenme, ölüm vb. durumlarda kullanýlan mazeret izni", 2, true, DateTime.UtcNow },
                    { 3, "Hastalýk Ýzni", null, 1, null, null, true, true, "#FF9800", "Saðlýk raporu ile alýnan hastalýk izni", 3, true, DateTime.UtcNow },
                    { 4, "Doðum Ýzni", 112, 112, 112, "Kadýn", false, true, "#E91E63", "Doðum öncesi ve sonrasý analýk izni", 4, true, DateTime.UtcNow },
                    { 5, "Ücretsiz Ýzin", null, 1, null, null, false, false, "#9E9E9E", "Ücretsiz izin talebi", 5, true, DateTime.UtcNow },
                    { 6, "Dýþ Görev", null, 1, null, null, false, true, "#673AB7", "Ýþ gereði yapýlan dýþ görev", 6, true, DateTime.UtcNow },
                    { 7, "Diðer", null, 1, null, null, false, true, "#607D8B", "Diðer izin türleri", 7, true, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_izin_talepleri_izin_tipleri_IzinTipiId",
                table: "izin_talepleri");

            migrationBuilder.DropIndex(
                name: "IX_izin_talepleri_IzinTipiId",
                table: "izin_talepleri");

            migrationBuilder.DropColumn(
                name: "IzinTipiId",
                table: "izin_talepleri");

            migrationBuilder.DropTable(
                name: "izin_tipleri");
        }
    }
}
