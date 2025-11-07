using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBordroModuleComprehensive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bordro_parametreleri",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    yil = table.Column<int>(type: "integer", nullable: false),
                    donem = table.Column<int>(type: "integer", nullable: false),
                    asgari_ucret_brut = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    asgari_ucret_net = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    agi_orani = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    agi_tutari = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    sgk_isci_orani = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    sgk_isveren_orani = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    sgk_tavan_brut = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    sgk_taban_brut = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    issizlik_isci_orani = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    issizlik_isveren_orani = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    damga_vergisi_orani = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    vergi_dilim1_ust_sinir = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vergi_dilim1_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    vergi_dilim2_ust_sinir = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vergi_dilim2_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    vergi_dilim3_ust_sinir = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vergi_dilim3_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    vergi_dilim4_ust_sinir = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    vergi_dilim4_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    vergi_dilim5_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    agi_bekar_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    agi_evli_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    agi_cocuk1_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    agi_cocuk2_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    agi_cocuk3_oran = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    normal_mesai_carpan = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    hafta_ici_mesai_carpan = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    hafta_sonu_mesai_carpan = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    gece_mesai_carpan = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    resmi_tatil_carpan = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    yillik_izin_1_5_yil = table.Column<int>(type: "int", nullable: false),
                    yillik_izin_5_15_yil = table.Column<int>(type: "int", nullable: false),
                    yillik_izin_15_yil_ustu = table.Column<int>(type: "int", nullable: false),
                    kidem_tavan = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bordro_parametreleri", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "kesinti_tanimlari",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kesinti_turu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    otomatik_hesaplama = table.Column<bool>(type: "boolean", nullable: false),
                    hesaplama_formulu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    oran = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    sabit_tutar = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    taksitlenebilir = table.Column<bool>(type: "boolean", nullable: false),
                    sira_no = table.Column<int>(type: "integer", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kesinti_tanimlari", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "odeme_tanimlari",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    odeme_turu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sgk_matrahina_dahil = table.Column<bool>(type: "boolean", nullable: false),
                    vergi_matrahina_dahil = table.Column<bool>(type: "boolean", nullable: false),
                    agi_uygulanir = table.Column<bool>(type: "boolean", nullable: false),
                    damga_vergisi_dahil = table.Column<bool>(type: "boolean", nullable: false),
                    varsayilan_tutar = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    hesaplama_formulu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sira_no = table.Column<int>(type: "integer", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_odeme_tanimlari", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "puantajlar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    donem_yil = table.Column<int>(type: "integer", nullable: false),
                    donem_ay = table.Column<int>(type: "integer", nullable: false),
                    calisan_gun_sayisi = table.Column<int>(type: "integer", nullable: false),
                    hafta_sonu_calisma = table.Column<int>(type: "integer", nullable: false),
                    resmi_tatil_calisma = table.Column<int>(type: "integer", nullable: false),
                    yillik_izin = table.Column<int>(type: "integer", nullable: false),
                    ucretsiz_izin = table.Column<int>(type: "integer", nullable: false),
                    hastalik_izni = table.Column<int>(type: "integer", nullable: false),
                    mazeret_izni = table.Column<int>(type: "integer", nullable: false),
                    hafta_ici_mesai_saat = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    hafta_sonu_mesai_saat = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    gece_mesai_saat = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    resmi_tatil_mesai_saat = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    gec_gelme_dakika = table.Column<int>(type: "integer", nullable: false),
                    erken_cikma_dakika = table.Column<int>(type: "integer", nullable: false),
                    devamsizlik_gun = table.Column<int>(type: "integer", nullable: false),
                    toplam_calisilan_gun = table.Column<int>(type: "integer", nullable: false),
                    toplam_mesai_saat = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    notlar = table.Column<string>(type: "text", nullable: true),
                    onay_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    onaylayan_id = table.Column<int>(type: "integer", nullable: true),
                    onay_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    red_nedeni = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_puantajlar", x => x.id);
                    table.CheckConstraint("CK_Puantaj_OnayDurumu", "onay_durumu IN ('Taslak', 'Onayda', 'Onaylandi', 'Reddedildi')");
                    table.ForeignKey(
                        name: "FK_puantajlar_personeller_onaylayan_id",
                        column: x => x.onaylayan_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_puantajlar_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bordro_ana",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    puantaj_id = table.Column<int>(type: "integer", nullable: true),
                    donem_yil = table.Column<int>(type: "integer", nullable: false),
                    donem_ay = table.Column<int>(type: "integer", nullable: false),
                    bordro_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pozisyon_adi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    departman_adi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    medeni_durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cocuk_sayisi = table.Column<int>(type: "integer", nullable: false),
                    engelli_durumu = table.Column<bool>(type: "boolean", nullable: false),
                    brut_maas = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    toplam_odeme = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    toplam_kesinti = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    net_ucret = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    sgk_matrahi = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    sgk_isci_payi = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    sgk_isveren_payi = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    issizlik_isci_payi = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    issizlik_isveren_payi = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    gelir_vergisi_matrahi = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    gelir_vergisi = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    damga_vergisi = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    agi_tutari = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    agi_orani = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    kumulatif_gelir = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    kumulatif_vergi = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    isveren_maliyeti = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    odeme_sekli = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    odeme_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    odeme_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    bordro_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    onay_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    ozel_notlar = table.Column<string>(type: "text", nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    hesaplama_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bordro_ana", x => x.id);
                    table.CheckConstraint("CK_BordroAna_BordroDurumu", "bordro_durumu IN ('Taslak', 'Onayda', 'Onaylandi', 'Odendi', 'Iptal')");
                    table.CheckConstraint("CK_BordroAna_OdemeDurumu", "odeme_durumu IN ('Beklemede', 'Odendi', 'Iptal')");
                    table.CheckConstraint("CK_BordroAna_OnayDurumu", "onay_durumu IN ('Beklemede', 'Onaylandi', 'Reddedildi')");
                    table.ForeignKey(
                        name: "FK_bordro_ana_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_bordro_ana_puantajlar_puantaj_id",
                        column: x => x.puantaj_id,
                        principalTable: "puantajlar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "bordro_kesintiler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bordro_id = table.Column<int>(type: "integer", nullable: false),
                    kesinti_tanimi_id = table.Column<int>(type: "integer", nullable: false),
                    kesinti_kodu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kesinti_adi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kesinti_turu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tutar = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    matrah = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    oran = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    otomatik_hesaplama = table.Column<bool>(type: "boolean", nullable: false),
                    taksit_no = table.Column<int>(type: "integer", nullable: true),
                    toplam_taksit = table.Column<int>(type: "integer", nullable: true),
                    kalan_borc = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    referans_id = table.Column<int>(type: "integer", nullable: true),
                    referans_tablo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    referans_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    hesaplama_detayi = table.Column<string>(type: "text", nullable: true),
                    sira_no = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bordro_kesintiler", x => x.id);
                    table.ForeignKey(
                        name: "FK_bordro_kesintiler_bordro_ana_bordro_id",
                        column: x => x.bordro_id,
                        principalTable: "bordro_ana",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bordro_kesintiler_kesinti_tanimlari_kesinti_tanimi_id",
                        column: x => x.kesinti_tanimi_id,
                        principalTable: "kesinti_tanimlari",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bordro_odemeler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bordro_id = table.Column<int>(type: "integer", nullable: false),
                    odeme_tanimi_id = table.Column<int>(type: "integer", nullable: false),
                    odeme_kodu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    odeme_adi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    odeme_turu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    tutar = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    miktar = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    birim = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    birim_fiyat = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    sgk_matrahina_dahil = table.Column<bool>(type: "boolean", nullable: false),
                    vergi_matrahina_dahil = table.Column<bool>(type: "boolean", nullable: false),
                    agi_uygulanir = table.Column<bool>(type: "boolean", nullable: false),
                    damga_vergisi_dahil = table.Column<bool>(type: "boolean", nullable: false),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    hesaplama_detayi = table.Column<string>(type: "text", nullable: true),
                    sira_no = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bordro_odemeler", x => x.id);
                    table.ForeignKey(
                        name: "FK_bordro_odemeler_bordro_ana_bordro_id",
                        column: x => x.bordro_id,
                        principalTable: "bordro_ana",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bordro_odemeler_odeme_tanimlari_odeme_tanimi_id",
                        column: x => x.odeme_tanimi_id,
                        principalTable: "odeme_tanimlari",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bordro_onaylar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bordro_id = table.Column<int>(type: "integer", nullable: false),
                    onay_seviyesi = table.Column<int>(type: "integer", nullable: false),
                    onay_seviye_adi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    onaylayan_id = table.Column<int>(type: "integer", nullable: true),
                    onaylayan_ad_soyad = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    onay_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    onay_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    red_nedeni = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    onay_notu = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    email_gonderildi = table.Column<bool>(type: "boolean", nullable: false),
                    email_gonderim_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bordro_onaylar", x => x.id);
                    table.CheckConstraint("CK_BordroOnay_OnayDurumu", "onay_durumu IN ('Beklemede', 'Onaylandi', 'Reddedildi', 'Iptal')");
                    table.ForeignKey(
                        name: "FK_bordro_onaylar_bordro_ana_bordro_id",
                        column: x => x.bordro_id,
                        principalTable: "bordro_ana",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bordro_onaylar_personeller_onaylayan_id",
                        column: x => x.onaylayan_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bordro_ana_personel_id_donem_yil_donem_ay",
                table: "bordro_ana",
                columns: new[] { "personel_id", "donem_yil", "donem_ay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_bordro_ana_puantaj_id",
                table: "bordro_ana",
                column: "puantaj_id");

            migrationBuilder.CreateIndex(
                name: "IX_bordro_kesintiler_bordro_id",
                table: "bordro_kesintiler",
                column: "bordro_id");

            migrationBuilder.CreateIndex(
                name: "IX_bordro_kesintiler_kesinti_tanimi_id",
                table: "bordro_kesintiler",
                column: "kesinti_tanimi_id");

            migrationBuilder.CreateIndex(
                name: "IX_bordro_odemeler_bordro_id",
                table: "bordro_odemeler",
                column: "bordro_id");

            migrationBuilder.CreateIndex(
                name: "IX_bordro_odemeler_odeme_tanimi_id",
                table: "bordro_odemeler",
                column: "odeme_tanimi_id");

            migrationBuilder.CreateIndex(
                name: "IX_bordro_onaylar_bordro_id",
                table: "bordro_onaylar",
                column: "bordro_id");

            migrationBuilder.CreateIndex(
                name: "IX_bordro_onaylar_onaylayan_id",
                table: "bordro_onaylar",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_bordro_parametreleri_yil_donem",
                table: "bordro_parametreleri",
                columns: new[] { "yil", "donem" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kesinti_tanimlari_kod",
                table: "kesinti_tanimlari",
                column: "kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_odeme_tanimlari_kod",
                table: "odeme_tanimlari",
                column: "kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_puantajlar_onaylayan_id",
                table: "puantajlar",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_puantajlar_personel_id_donem_yil_donem_ay",
                table: "puantajlar",
                columns: new[] { "personel_id", "donem_yil", "donem_ay" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bordro_kesintiler");

            migrationBuilder.DropTable(
                name: "bordro_odemeler");

            migrationBuilder.DropTable(
                name: "bordro_onaylar");

            migrationBuilder.DropTable(
                name: "bordro_parametreleri");

            migrationBuilder.DropTable(
                name: "kesinti_tanimlari");

            migrationBuilder.DropTable(
                name: "odeme_tanimlari");

            migrationBuilder.DropTable(
                name: "bordro_ana");

            migrationBuilder.DropTable(
                name: "puantajlar");
        }
    }
}
