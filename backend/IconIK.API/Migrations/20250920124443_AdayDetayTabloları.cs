using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AdayDetayTablolarý : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aday_diller",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    dil = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    okuma_seviyesi = table.Column<int>(type: "integer", nullable: false),
                    yazma_seviyesi = table.Column<int>(type: "integer", nullable: false),
                    konusma_seviyesi = table.Column<int>(type: "integer", nullable: false),
                    sertifika = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sertifika_puani = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_diller", x => x.id);
                    table.CheckConstraint("CK_AdayDil_Seviyeler", "okuma_seviyesi BETWEEN 1 AND 5 AND yazma_seviyesi BETWEEN 1 AND 5 AND konusma_seviyesi BETWEEN 1 AND 5");
                    table.ForeignKey(
                        name: "FK_aday_diller_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aday_egitimler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    okul_ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    bolum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    derece = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    baslangic_yili = table.Column<int>(type: "integer", nullable: false),
                    mezuniyet_yili = table.Column<int>(type: "integer", nullable: true),
                    devam_ediyor = table.Column<bool>(type: "boolean", nullable: false),
                    not_ortalamasi = table.Column<decimal>(type: "numeric", nullable: true),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_egitimler", x => x.id);
                    table.ForeignKey(
                        name: "FK_aday_egitimler_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aday_hobiler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    hobi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kategori = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    seviye = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_hobiler", x => x.id);
                    table.ForeignKey(
                        name: "FK_aday_hobiler_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aday_projeler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    proje_ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    rol = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    baslangic_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bitis_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    devam_ediyor = table.Column<bool>(type: "boolean", nullable: false),
                    teknolojiler = table.Column<string>(type: "text", nullable: true),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    proje_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_projeler", x => x.id);
                    table.ForeignKey(
                        name: "FK_aday_projeler_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aday_referanslar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    ad_soyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sirket = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    pozisyon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    iliski_turu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_referanslar", x => x.id);
                    table.ForeignKey(
                        name: "FK_aday_referanslar_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aday_sertifikalar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    sertifika_ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    veren_kurum = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gecerlilik_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sertifika_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_sertifikalar", x => x.id);
                    table.ForeignKey(
                        name: "FK_aday_sertifikalar_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aday_diller_aday_id",
                table: "aday_diller",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_egitimler_aday_id",
                table: "aday_egitimler",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_hobiler_aday_id",
                table: "aday_hobiler",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_projeler_aday_id",
                table: "aday_projeler",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_referanslar_aday_id",
                table: "aday_referanslar",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_sertifikalar_aday_id",
                table: "aday_sertifikalar",
                column: "aday_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aday_diller");

            migrationBuilder.DropTable(
                name: "aday_egitimler");

            migrationBuilder.DropTable(
                name: "aday_hobiler");

            migrationBuilder.DropTable(
                name: "aday_projeler");

            migrationBuilder.DropTable(
                name: "aday_referanslar");

            migrationBuilder.DropTable(
                name: "aday_sertifikalar");
        }
    }
}
