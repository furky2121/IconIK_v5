using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class IseAlimModulu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adaylar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    soyad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tc_kimlik = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dogum_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cinsiyet = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    medeni_durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    adres = table.Column<string>(type: "text", nullable: true),
                    sehir = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    universite = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bolum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mezuniyet_yili = table.Column<int>(type: "integer", nullable: true),
                    toplam_deneyim = table.Column<int>(type: "integer", nullable: false),
                    ozgecmis_dosyasi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    linkedin_url = table.Column<string>(type: "text", nullable: true),
                    notlar = table.Column<string>(type: "text", nullable: true),
                    kara_liste = table.Column<bool>(type: "boolean", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adaylar", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ilan_kategoriler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ilan_kategoriler", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "aday_deneyimler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    sirket_ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    pozisyon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    baslangic_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bitis_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    halen_calisiyor = table.Column<bool>(type: "boolean", nullable: false),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_deneyimler", x => x.id);
                    table.ForeignKey(
                        name: "FK_aday_deneyimler_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aday_yetenekler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    yetenek = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    seviye = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_yetenekler", x => x.id);
                    table.ForeignKey(
                        name: "FK_aday_yetenekler_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "is_ilanlari",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    baslik = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kategori_id = table.Column<int>(type: "integer", nullable: false),
                    pozisyon_id = table.Column<int>(type: "integer", nullable: false),
                    departman_id = table.Column<int>(type: "integer", nullable: false),
                    is_tanimi = table.Column<string>(type: "text", nullable: false),
                    gereksinimler = table.Column<string>(type: "text", nullable: false),
                    min_maas = table.Column<decimal>(type: "numeric", nullable: true),
                    max_maas = table.Column<decimal>(type: "numeric", nullable: true),
                    calisme_sekli = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    deneyim_yili = table.Column<int>(type: "integer", nullable: false),
                    egitim_seviyesi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    yayin_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bitis_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    durum = table.Column<int>(type: "integer", nullable: false),
                    olusturan_id = table.Column<int>(type: "integer", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_is_ilanlari", x => x.id);
                    table.CheckConstraint("CK_IsIlani_Durum", "durum IN (1, 2, 3, 4)");
                    table.ForeignKey(
                        name: "FK_is_ilanlari_departmanlar_departman_id",
                        column: x => x.departman_id,
                        principalTable: "departmanlar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_is_ilanlari_ilan_kategoriler_kategori_id",
                        column: x => x.kategori_id,
                        principalTable: "ilan_kategoriler",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_is_ilanlari_personeller_olusturan_id",
                        column: x => x.olusturan_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_is_ilanlari_pozisyonlar_pozisyon_id",
                        column: x => x.pozisyon_id,
                        principalTable: "pozisyonlar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "basvurular",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ilan_id = table.Column<int>(type: "integer", nullable: false),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    basvuru_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    durum = table.Column<int>(type: "integer", nullable: false),
                    kapak_mektubu = table.Column<string>(type: "text", nullable: true),
                    beklenen_maas = table.Column<decimal>(type: "numeric", nullable: true),
                    ise_baslama_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    degerlendiren_id = table.Column<int>(type: "integer", nullable: true),
                    degerlendirme_notu = table.Column<string>(type: "text", nullable: true),
                    puan = table.Column<int>(type: "integer", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_basvurular", x => x.id);
                    table.CheckConstraint("CK_Basvuru_Durum", "durum IN (1, 2, 3, 4, 5, 6, 7, 8)");
                    table.ForeignKey(
                        name: "FK_basvurular_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_basvurular_is_ilanlari_ilan_id",
                        column: x => x.ilan_id,
                        principalTable: "is_ilanlari",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_basvurular_personeller_degerlendiren_id",
                        column: x => x.degerlendiren_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "mulakatlar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    basvuru_id = table.Column<int>(type: "integer", nullable: false),
                    tur = table.Column<int>(type: "integer", nullable: false),
                    tarih = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sure = table.Column<int>(type: "integer", nullable: false),
                    lokasyon = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    mulakat_yapan_id = table.Column<int>(type: "integer", nullable: false),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notlar = table.Column<string>(type: "text", nullable: true),
                    puan = table.Column<int>(type: "integer", nullable: true),
                    sonuc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mulakatlar", x => x.id);
                    table.CheckConstraint("CK_Mulakat_Tur", "tur IN (1, 2, 3, 4, 5)");
                    table.ForeignKey(
                        name: "FK_mulakatlar_basvurular_basvuru_id",
                        column: x => x.basvuru_id,
                        principalTable: "basvurular",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mulakatlar_personeller_mulakat_yapan_id",
                        column: x => x.mulakat_yapan_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teklif_mektuplari",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    basvuru_id = table.Column<int>(type: "integer", nullable: false),
                    pozisyon = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    maas = table.Column<decimal>(type: "numeric", nullable: false),
                    ek_odemeler = table.Column<string>(type: "text", nullable: true),
                    izin_hakki = table.Column<int>(type: "integer", nullable: false),
                    ise_baslama_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gecerlilik_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    gonderim_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    yanit_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    red_nedeni = table.Column<string>(type: "text", nullable: true),
                    hazirlayan_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teklif_mektuplari", x => x.id);
                    table.ForeignKey(
                        name: "FK_teklif_mektuplari_basvurular_basvuru_id",
                        column: x => x.basvuru_id,
                        principalTable: "basvurular",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_teklif_mektuplari_personeller_hazirlayan_id",
                        column: x => x.hazirlayan_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aday_deneyimler_aday_id",
                table: "aday_deneyimler",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_yetenekler_aday_id",
                table: "aday_yetenekler",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_adaylar_email",
                table: "adaylar",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_adaylar_tc_kimlik",
                table: "adaylar",
                column: "tc_kimlik",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_basvurular_aday_id",
                table: "basvurular",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_basvurular_degerlendiren_id",
                table: "basvurular",
                column: "degerlendiren_id");

            migrationBuilder.CreateIndex(
                name: "IX_basvurular_ilan_id",
                table: "basvurular",
                column: "ilan_id");

            migrationBuilder.CreateIndex(
                name: "IX_ilan_kategoriler_ad",
                table: "ilan_kategoriler",
                column: "ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_is_ilanlari_departman_id",
                table: "is_ilanlari",
                column: "departman_id");

            migrationBuilder.CreateIndex(
                name: "IX_is_ilanlari_kategori_id",
                table: "is_ilanlari",
                column: "kategori_id");

            migrationBuilder.CreateIndex(
                name: "IX_is_ilanlari_olusturan_id",
                table: "is_ilanlari",
                column: "olusturan_id");

            migrationBuilder.CreateIndex(
                name: "IX_is_ilanlari_pozisyon_id",
                table: "is_ilanlari",
                column: "pozisyon_id");

            migrationBuilder.CreateIndex(
                name: "IX_mulakatlar_basvuru_id",
                table: "mulakatlar",
                column: "basvuru_id");

            migrationBuilder.CreateIndex(
                name: "IX_mulakatlar_mulakat_yapan_id",
                table: "mulakatlar",
                column: "mulakat_yapan_id");

            migrationBuilder.CreateIndex(
                name: "IX_teklif_mektuplari_basvuru_id",
                table: "teklif_mektuplari",
                column: "basvuru_id");

            migrationBuilder.CreateIndex(
                name: "IX_teklif_mektuplari_hazirlayan_id",
                table: "teklif_mektuplari",
                column: "hazirlayan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aday_deneyimler");

            migrationBuilder.DropTable(
                name: "aday_yetenekler");

            migrationBuilder.DropTable(
                name: "mulakatlar");

            migrationBuilder.DropTable(
                name: "teklif_mektuplari");

            migrationBuilder.DropTable(
                name: "basvurular");

            migrationBuilder.DropTable(
                name: "adaylar");

            migrationBuilder.DropTable(
                name: "is_ilanlari");

            migrationBuilder.DropTable(
                name: "ilan_kategoriler");
        }
    }
}
