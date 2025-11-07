using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddKVKKIzinMetni : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "kvkk_onay_tarihi",
                table: "kullanicilar",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "kvkk_onaylandi",
                table: "kullanicilar",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "kvkk_izin_metinleri",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    baslik = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    metin = table.Column<string>(type: "text", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    versiyon = table.Column<int>(type: "integer", nullable: false),
                    yayinlanma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    olusturan_personel_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kvkk_izin_metinleri", x => x.id);
                    table.ForeignKey(
                        name: "FK_kvkk_izin_metinleri_personeller_olusturan_personel_id",
                        column: x => x.olusturan_personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kvkk_izin_metinleri_olusturan_personel_id",
                table: "kvkk_izin_metinleri",
                column: "olusturan_personel_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kvkk_izin_metinleri");

            migrationBuilder.DropColumn(
                name: "kvkk_onay_tarihi",
                table: "kullanicilar");

            migrationBuilder.DropColumn(
                name: "kvkk_onaylandi",
                table: "kullanicilar");
        }
    }
}
