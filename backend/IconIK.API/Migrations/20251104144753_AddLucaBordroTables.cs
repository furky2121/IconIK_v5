using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLucaBordroTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "luca_bordro_ayarlari",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    baglanti_tipi = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    api_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    api_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    api_username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    api_password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    dosya_yolu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    otomatik_senkron = table.Column<bool>(type: "boolean", nullable: false),
                    senkron_saati = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    son_senkron_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_luca_bordro_ayarlari", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "luca_bordrolar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: true),
                    tc_kimlik = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    sicil_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ad_soyad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    donem_yil = table.Column<int>(type: "integer", nullable: false),
                    donem_ay = table.Column<int>(type: "integer", nullable: false),
                    bordro_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    brut_maas = table.Column<decimal>(type: "numeric", nullable: true),
                    net_ucret = table.Column<decimal>(type: "numeric", nullable: true),
                    toplam_odeme = table.Column<decimal>(type: "numeric", nullable: true),
                    toplam_kesinti = table.Column<decimal>(type: "numeric", nullable: true),
                    sgk_isci = table.Column<decimal>(type: "numeric", nullable: true),
                    gelir_vergisi = table.Column<decimal>(type: "numeric", nullable: true),
                    damga_vergisi = table.Column<decimal>(type: "numeric", nullable: true),
                    detay_json = table.Column<string>(type: "text", nullable: true),
                    senkron_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_luca_bordrolar", x => x.id);
                    table.ForeignKey(
                        name: "FK_luca_bordrolar_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "otp_kodlar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kullanici_id = table.Column<int>(type: "integer", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    otp_kodu = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    luca_bordro_id = table.Column<int>(type: "integer", nullable: true),
                    olusturma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gecerlilik_suresi = table.Column<int>(type: "integer", nullable: false),
                    kullanildi = table.Column<bool>(type: "boolean", nullable: false),
                    kullanim_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deneme_sayisi = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_otp_kodlar", x => x.id);
                    table.ForeignKey(
                        name: "FK_otp_kodlar_kullanicilar_kullanici_id",
                        column: x => x.kullanici_id,
                        principalTable: "kullanicilar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_otp_kodlar_luca_bordrolar_luca_bordro_id",
                        column: x => x.luca_bordro_id,
                        principalTable: "luca_bordrolar",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_luca_bordrolar_personel_id",
                table: "luca_bordrolar",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_otp_kodlar_kullanici_id",
                table: "otp_kodlar",
                column: "kullanici_id");

            migrationBuilder.CreateIndex(
                name: "IX_otp_kodlar_luca_bordro_id",
                table: "otp_kodlar",
                column: "luca_bordro_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "luca_bordro_ayarlari");

            migrationBuilder.DropTable(
                name: "otp_kodlar");

            migrationBuilder.DropTable(
                name: "luca_bordrolar");
        }
    }
}
