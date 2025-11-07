using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departmanlar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kod = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departmanlar", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "egitimler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    baslik = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    baslangic_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bitis_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sure_saat = table.Column<int>(type: "integer", nullable: true),
                    egitmen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    konum = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    kapasite = table.Column<int>(type: "integer", nullable: true),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_egitimler", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kademeler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    seviye = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kademeler", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pozisyonlar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    departman_id = table.Column<int>(type: "integer", nullable: false),
                    kademe_id = table.Column<int>(type: "integer", nullable: false),
                    min_maas = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    max_maas = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pozisyonlar", x => x.id);
                    table.ForeignKey(
                        name: "FK_pozisyonlar_departmanlar_departman_id",
                        column: x => x.departman_id,
                        principalTable: "departmanlar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pozisyonlar_kademeler_kademe_id",
                        column: x => x.kademe_id,
                        principalTable: "kademeler",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "personeller",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tc_kimlik = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    ad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    soyad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    telefon = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    dogum_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ise_baslama_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cikis_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    pozisyon_id = table.Column<int>(type: "integer", nullable: false),
                    yonetici_id = table.Column<int>(type: "integer", nullable: true),
                    maas = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    fotograf_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    adres = table.Column<string>(type: "text", nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personeller", x => x.id);
                    table.ForeignKey(
                        name: "FK_personeller_personeller_yonetici_id",
                        column: x => x.yonetici_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_personeller_pozisyonlar_pozisyon_id",
                        column: x => x.pozisyon_id,
                        principalTable: "pozisyonlar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bordrolar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    donem_yil = table.Column<int>(type: "integer", nullable: false),
                    donem_ay = table.Column<int>(type: "integer", nullable: false),
                    brut_maas = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    net_maas = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    sgk_kesinti = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vergi_kesinti = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    diger_kesintiler = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    prim_odemeler = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    mesai_odemeler = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bordrolar", x => x.id);
                    table.ForeignKey(
                        name: "FK_bordrolar_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "izin_talepleri",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    baslangic_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bitis_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gun_sayisi = table.Column<int>(type: "integer", nullable: false),
                    izin_tipi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    onaylayan_id = table.Column<int>(type: "integer", nullable: true),
                    onay_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    onay_notu = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izin_talepleri", x => x.id);
                    table.CheckConstraint("CK_IzinTalebi_Durum", "durum IN ('Beklemede', 'Onaylandý', 'Reddedildi')");
                    table.ForeignKey(
                        name: "FK_izin_talepleri_personeller_onaylayan_id",
                        column: x => x.onaylayan_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_izin_talepleri_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kullanicilar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    kullanici_adi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    sifre_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ilk_giris = table.Column<bool>(type: "boolean", nullable: false),
                    son_giris_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kullanicilar", x => x.id);
                    table.ForeignKey(
                        name: "FK_kullanicilar_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "personel_egitimleri",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    egitim_id = table.Column<int>(type: "integer", nullable: false),
                    katilim_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    puan = table.Column<int>(type: "integer", nullable: true),
                    sertifika_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personel_egitimleri", x => x.id);
                    table.ForeignKey(
                        name: "FK_personel_egitimleri_egitimler_egitim_id",
                        column: x => x.egitim_id,
                        principalTable: "egitimler",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_personel_egitimleri_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bordrolar_personel_id_donem_yil_donem_ay",
                table: "bordrolar",
                columns: new[] { "personel_id", "donem_yil", "donem_ay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departmanlar_ad",
                table: "departmanlar",
                column: "ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departmanlar_kod",
                table: "departmanlar",
                column: "kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_izin_talepleri_onaylayan_id",
                table: "izin_talepleri",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_izin_talepleri_personel_id",
                table: "izin_talepleri",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_kademeler_ad",
                table: "kademeler",
                column: "ad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kademeler_seviye",
                table: "kademeler",
                column: "seviye",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kullanicilar_kullanici_adi",
                table: "kullanicilar",
                column: "kullanici_adi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kullanicilar_personel_id",
                table: "kullanicilar",
                column: "personel_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personel_egitimleri_egitim_id",
                table: "personel_egitimleri",
                column: "egitim_id");

            migrationBuilder.CreateIndex(
                name: "IX_personel_egitimleri_personel_id",
                table: "personel_egitimleri",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_personeller_email",
                table: "personeller",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personeller_pozisyon_id",
                table: "personeller",
                column: "pozisyon_id");

            migrationBuilder.CreateIndex(
                name: "IX_personeller_tc_kimlik",
                table: "personeller",
                column: "tc_kimlik",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personeller_yonetici_id",
                table: "personeller",
                column: "yonetici_id");

            migrationBuilder.CreateIndex(
                name: "IX_pozisyonlar_ad_departman_id_kademe_id",
                table: "pozisyonlar",
                columns: new[] { "ad", "departman_id", "kademe_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pozisyonlar_departman_id",
                table: "pozisyonlar",
                column: "departman_id");

            migrationBuilder.CreateIndex(
                name: "IX_pozisyonlar_kademe_id",
                table: "pozisyonlar",
                column: "kademe_id");

            // Seed Data
            SeedData(migrationBuilder);
        }

        private void SeedData(MigrationBuilder migrationBuilder)
        {
            // Kademeler
            migrationBuilder.InsertData(
                table: "kademeler",
                columns: new[] { "ad", "seviye", "created_at", "updated_at" },
                values: new object[,]
                {
                    { "Genel Müdür", 1, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "Direktör", 2, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "Müdür", 3, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "Þef", 4, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "Uzman", 5, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "Uzman Yardýmcýsý", 6, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "Memur", 7, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "Stajyer", 8, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "Danýþman", 9, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) }
                });

            // Departmanlar
            migrationBuilder.InsertData(
                table: "departmanlar",
                columns: new[] { "ad", "kod", "aciklama", "aktif", "created_at", "updated_at" },
                values: new object[,]
                {
                    { "Ýnsan Kaynaklarý", "IK", "Personel iþleri ve insan kaynaklarý yönetimi", true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "Bilgi Ýþlem", "BIT", "Bilgi teknolojileri ve sistem yönetimi", true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) }
                });

            // Pozisyonlar
            migrationBuilder.InsertData(
                table: "pozisyonlar",
                columns: new[] { "ad", "departman_id", "kademe_id", "min_maas", "max_maas", "aktif", "created_at", "updated_at" },
                values: new object[,]
                {
                    { "Genel Müdür", 1, 1, 50000.00m, 75000.00m, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "ÝK Direktörü", 1, 2, 30000.00m, 45000.00m, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "BIT Direktörü", 2, 2, 35000.00m, 50000.00m, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "ÝK Uzmaný", 1, 5, 12000.00m, 18000.00m, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) }
                });

            // Personeller
            migrationBuilder.InsertData(
                table: "personeller",
                columns: new[] { "tc_kimlik", "ad", "soyad", "email", "telefon", "dogum_tarihi", "ise_baslama_tarihi", "pozisyon_id", "yonetici_id", "maas", "aktif", "created_at", "updated_at" },
                values: new object[,]
                {
                    { "12345678901", "Ahmet", "Yýlmaz", "ahmet.yilmaz@bilgelojistik.com", "0532-123-4567", new DateTime(1975, 5, 15, 0, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1, null, 60000.00m, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "12345678902", "Mehmet", "Kaya", "mehmet.kaya@bilgelojistik.com", "0532-234-5678", new DateTime(1980, 3, 20, 0, 0, 0, DateTimeKind.Utc), new DateTime(2012, 3, 1, 0, 0, 0, DateTimeKind.Utc), 2, 1, 35000.00m, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "12345678903", "Ali", "Demir", "ali.demir@bilgelojistik.com", "0532-345-6789", new DateTime(1978, 8, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 6, 1, 0, 0, 0, DateTimeKind.Utc), 3, 1, 40000.00m, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { "12345678912", "Özcan", "Bulut", "ozcan.bulut@bilgelojistik.com", "0532-234-5679", new DateTime(1990, 5, 30, 0, 0, 0, DateTimeKind.Utc), new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), 4, 2, 15000.00m, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) }
                });

            // Kullanýcýlar (SHA256 hash'leri ile)
            migrationBuilder.InsertData(
                table: "kullanicilar",
                columns: new[] { "personel_id", "kullanici_adi", "sifre_hash", "ilk_giris", "aktif", "created_at", "updated_at" },
                values: new object[,]
                {
                    { 1, "ahmet.yilmaz", "213fc33d8f2dbde3207734e3097ea72a69fb8b009f2878468cdd9e74b70a1e59", true, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { 2, "mehmet.kaya", "ed209dafb3c690d3b9b2ed800b703da20648ffaa6b47883f4cf4a2474c853cc3", true, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { 3, "ali.demir", "d64fe06e31bcae668cd7dd2db726d60b8e80cc7e7b1445a9aa14f110a7d6f386", true, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) },
                    { 4, "ozcan.bulut", "e781b2c55b7dd50d14e90f2b616af651c8ec1d8064db6491b7e0daeea4e4efe8", true, true, new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 9, 13, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bordrolar");

            migrationBuilder.DropTable(
                name: "izin_talepleri");

            migrationBuilder.DropTable(
                name: "kullanicilar");

            migrationBuilder.DropTable(
                name: "personel_egitimleri");

            migrationBuilder.DropTable(
                name: "egitimler");

            migrationBuilder.DropTable(
                name: "personeller");

            migrationBuilder.DropTable(
                name: "pozisyonlar");

            migrationBuilder.DropTable(
                name: "departmanlar");

            migrationBuilder.DropTable(
                name: "kademeler");
        }
    }
}
