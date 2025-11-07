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
                    dogum_tarihi = table.Column<DateTime>(type: "date", nullable: true),
                    cinsiyet = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    medeni_durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    askerlik_durumu = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    adres = table.Column<string>(type: "text", nullable: true),
                    sehir = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    universite = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bolum = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mezuniyet_yili = table.Column<int>(type: "integer", nullable: true),
                    toplam_deneyim = table.Column<int>(type: "integer", nullable: false),
                    ozgecmis_dosyasi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    linkedin_url = table.Column<string>(type: "text", nullable: true),
                    notlar = table.Column<string>(type: "text", nullable: true),
                    durum = table.Column<int>(type: "integer", nullable: false),
                    durum_guncelleme_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    durum_guncelleme_notu = table.Column<string>(type: "text", nullable: true),
                    otomatik_cv_olusturuldu = table.Column<bool>(type: "boolean", nullable: false),
                    cv_dosya_yolu = table.Column<string>(type: "text", nullable: true),
                    fotograf_yolu = table.Column<string>(type: "text", nullable: true),
                    kara_liste = table.Column<bool>(type: "boolean", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_adaylar", x => x.id);
                    table.CheckConstraint("CK_Aday_Durum", "durum IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)");
                });

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
                name: "EkranYetkileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EkranAdi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EkranKodu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkranYetkileri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eposta_ayarlari",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    smtp_host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    smtp_port = table.Column<int>(type: "integer", nullable: false),
                    smtp_username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    smtp_password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    enable_ssl = table.Column<bool>(type: "boolean", nullable: false),
                    from_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    from_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eposta_ayarlari", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eposta_yonlendirme",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    yonlendirme_turu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    alici_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    gonderim_saati = table.Column<TimeSpan>(type: "interval", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    son_gonderim_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eposta_yonlendirme", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "firma_ayarlari",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    firma_adi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_firma_ayarlari", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "kademeler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    seviye = table.Column<int>(type: "integer", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kademeler", x => x.id);
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
                name: "Sehirler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SehirAd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PlakaKodu = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sehirler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VideoKategoriler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Renk = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Sira = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoKategoriler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "aday_cv",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    cv_tipi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    dosya_adi = table.Column<string>(type: "text", nullable: true),
                    dosya_yolu = table.Column<string>(type: "text", nullable: true),
                    dosya_boyutu = table.Column<long>(type: "bigint", nullable: true),
                    mime_tipi = table.Column<string>(type: "text", nullable: true),
                    otomatik_cv_html = table.Column<string>(type: "text", nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_cv", x => x.id);
                    table.CheckConstraint("CK_AdayCV_CVTipi", "cv_tipi IN ('Otomatik', 'Yuklenmiş')");
                    table.ForeignKey(
                        name: "FK_aday_cv_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    baslangic_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                name: "KademeEkranYetkileri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KademeId = table.Column<int>(type: "integer", nullable: false),
                    EkranYetkisiId = table.Column<int>(type: "integer", nullable: false),
                    OkumaYetkisi = table.Column<bool>(type: "boolean", nullable: false),
                    YazmaYetkisi = table.Column<bool>(type: "boolean", nullable: false),
                    SilmeYetkisi = table.Column<bool>(type: "boolean", nullable: false),
                    GuncellemeYetkisi = table.Column<bool>(type: "boolean", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KademeEkranYetkileri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KademeEkranYetkileri_EkranYetkileri_EkranYetkisiId",
                        column: x => x.EkranYetkisiId,
                        principalTable: "EkranYetkileri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KademeEkranYetkileri_kademeler_KademeId",
                        column: x => x.KademeId,
                        principalTable: "kademeler",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "VideoEgitimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Baslik = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VideoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Sure = table.Column<int>(type: "integer", nullable: false),
                    KategoriId = table.Column<int>(type: "integer", nullable: false),
                    Seviye = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Egitmen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IzlenmeMinimum = table.Column<int>(type: "integer", nullable: false),
                    ZorunluMu = table.Column<bool>(type: "boolean", nullable: false),
                    SonTamamlanmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoEgitimler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoEgitimler_VideoKategoriler_KategoriId",
                        column: x => x.KategoriId,
                        principalTable: "VideoKategoriler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    medeni_hal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cinsiyet = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    askerlik_durumu = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    egitim_durumu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    kan_grubu = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ehliyet_sinifi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    anne_adi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    baba_adi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    dogum_yeri = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    nufus_il_kod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    nufus_ilce_kod = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    acil_durum_iletisim = table.Column<string>(type: "text", nullable: true),
                    banka_hesap_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    iban_no = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
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
                name: "VideoSorular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoEgitimId = table.Column<int>(type: "integer", nullable: false),
                    Soru = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CevapA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CevapB = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CevapC = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CevapD = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DogruCevap = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    Puan = table.Column<int>(type: "integer", nullable: false),
                    Sira = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoSorular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoSorular_VideoEgitimler_VideoEgitimId",
                        column: x => x.VideoEgitimId,
                        principalTable: "VideoEgitimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anketler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Baslik = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Aciklama = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BaslangicTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BitisTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AnketDurumu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AnonymousMu = table.Column<bool>(type: "boolean", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false),
                    OlusturanPersonelId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anketler", x => x.Id);
                    table.CheckConstraint("CK_Anket_Durum", "\"AnketDurumu\" IN ('Taslak', 'Aktif', 'Tamamlandı')");
                    table.ForeignKey(
                        name: "FK_Anketler_personeller_OlusturanPersonelId",
                        column: x => x.OlusturanPersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "avans_talepleri",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    talep_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    talep_tutari = table.Column<decimal>(type: "numeric", nullable: false),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    onay_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    onaylayan_id = table.Column<int>(type: "integer", nullable: true),
                    onay_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    onay_notu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avans_talepleri", x => x.id);
                    table.ForeignKey(
                        name: "FK_avans_talepleri_personeller_onaylayan_id",
                        column: x => x.onaylayan_id,
                        principalTable: "personeller",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_avans_talepleri_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bildirimler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AliciId = table.Column<int>(type: "integer", nullable: false),
                    Baslik = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mesaj = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Kategori = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tip = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Okundu = table.Column<bool>(type: "boolean", nullable: false),
                    OkunmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GonderenAd = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ActionUrl = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bildirimler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bildirimler_personeller_AliciId",
                        column: x => x.AliciId,
                        principalTable: "personeller",
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
                name: "istifa_talepleri",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    istifa_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    son_calisma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    istifa_nedeni = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    onay_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    onaylayan_id = table.Column<int>(type: "integer", nullable: true),
                    onay_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    onay_notu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_istifa_talepleri", x => x.id);
                    table.ForeignKey(
                        name: "FK_istifa_talepleri_personeller_onaylayan_id",
                        column: x => x.onaylayan_id,
                        principalTable: "personeller",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_istifa_talepleri_personeller_personel_id",
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
                    izin_baslama_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    isbasi_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gun_sayisi = table.Column<int>(type: "integer", nullable: false),
                    izin_tipi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    aciklama = table.Column<string>(type: "text", nullable: true),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    onaylayan_id = table.Column<int>(type: "integer", nullable: true),
                    onay_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    onay_notu = table.Column<string>(type: "text", nullable: true),
                    gorev_yeri = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rapor_dosya_yolu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IzinTipiId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_izin_talepleri", x => x.id);
                    table.CheckConstraint("CK_IzinTalebi_Durum", "durum IN ('Beklemede', 'Onaylandı', 'Reddedildi')");
                    table.ForeignKey(
                        name: "FK_izin_talepleri_izin_tipleri_IzinTipiId",
                        column: x => x.IzinTipiId,
                        principalTable: "izin_tipleri",
                        principalColumn: "id");
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
                    kvkk_onaylandi = table.Column<bool>(type: "boolean", nullable: false),
                    kvkk_onay_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                name: "masraf_talepleri",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    masraf_tipi = table.Column<int>(type: "integer", nullable: false),
                    talep_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    tutar = table.Column<decimal>(type: "numeric", nullable: false),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    belge_url = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    onay_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    onaylayan_id = table.Column<int>(type: "integer", nullable: true),
                    onay_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    onay_notu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_masraf_talepleri", x => x.id);
                    table.ForeignKey(
                        name: "FK_masraf_talepleri_personeller_onaylayan_id",
                        column: x => x.onaylayan_id,
                        principalTable: "personeller",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_masraf_talepleri_personeller_personel_id",
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

            migrationBuilder.CreateTable(
                name: "personel_giris_cikis",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    giris_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cikis_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    giris_tipi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    calisma_suresi_dakika = table.Column<int>(type: "integer", nullable: true),
                    gec_kalma_dakika = table.Column<int>(type: "integer", nullable: false),
                    erken_cikma_dakika = table.Column<int>(type: "integer", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    olusturma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    guncelleme_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personel_giris_cikis", x => x.id);
                    table.CheckConstraint("CK_PersonelGirisCikis_GirisTipi", "giris_tipi IN ('Normal', 'Fazla Mesai', 'Hafta Sonu')");
                    table.ForeignKey(
                        name: "FK_personel_giris_cikis_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "VideoAtamalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoEgitimId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: true),
                    DepartmanId = table.Column<int>(type: "integer", nullable: true),
                    PozisyonId = table.Column<int>(type: "integer", nullable: true),
                    AtamaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Durum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AtayanPersonelId = table.Column<int>(type: "integer", nullable: true),
                    Not = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HatirlatmaGonderildiMi = table.Column<bool>(type: "boolean", nullable: false),
                    SonHatirlatmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoAtamalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoAtamalar_VideoEgitimler_VideoEgitimId",
                        column: x => x.VideoEgitimId,
                        principalTable: "VideoEgitimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoAtamalar_departmanlar_DepartmanId",
                        column: x => x.DepartmanId,
                        principalTable: "departmanlar",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_VideoAtamalar_personeller_AtayanPersonelId",
                        column: x => x.AtayanPersonelId,
                        principalTable: "personeller",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_VideoAtamalar_personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "personeller",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_VideoAtamalar_pozisyonlar_PozisyonId",
                        column: x => x.PozisyonId,
                        principalTable: "pozisyonlar",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "VideoIzlemeler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoEgitimId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    IzlemeBaslangic = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IzlemeBitis = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ToplamIzlenenSure = table.Column<int>(type: "integer", nullable: false),
                    IzlemeYuzdesi = table.Column<int>(type: "integer", nullable: false),
                    TamamlandiMi = table.Column<bool>(type: "boolean", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Puan = table.Column<int>(type: "integer", nullable: true),
                    CihazTipi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IpAdresi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VideoPlatform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IzlemeBaslangicSaniye = table.Column<int>(type: "integer", nullable: false),
                    IzlemeBitisSaniye = table.Column<int>(type: "integer", nullable: false),
                    VideoToplamSure = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoIzlemeler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoIzlemeler_VideoEgitimler_VideoEgitimId",
                        column: x => x.VideoEgitimId,
                        principalTable: "VideoEgitimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoIzlemeler_personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoSertifikalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoEgitimId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    SertifikaNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VerilmeTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GecerlilikTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuizPuani = table.Column<int>(type: "integer", nullable: true),
                    IzlemeYuzdesi = table.Column<int>(type: "integer", nullable: false),
                    SertifikaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Durum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoSertifikalar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoSertifikalar_VideoEgitimler_VideoEgitimId",
                        column: x => x.VideoEgitimId,
                        principalTable: "VideoEgitimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoSertifikalar_personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VideoYorumlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoEgitimId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    Yorum = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Puan = table.Column<int>(type: "integer", nullable: true),
                    YorumTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoYorumlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoYorumlar_VideoEgitimler_VideoEgitimId",
                        column: x => x.VideoEgitimId,
                        principalTable: "VideoEgitimler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoYorumlar_personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zimmet_stok",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    malzeme_adi = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    kategori = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    marka = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    seri_no = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    miktar = table.Column<int>(type: "integer", nullable: false),
                    kalan_miktar = table.Column<int>(type: "integer", nullable: false),
                    birim = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    onay_durumu = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    onaylayan_id = table.Column<int>(type: "integer", nullable: true),
                    onay_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    onay_notu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    olusturan_id = table.Column<int>(type: "integer", nullable: true),
                    olusturma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    guncelleme_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zimmet_stok", x => x.id);
                    table.CheckConstraint("CK_ZimmetStok_OnayDurumu", "onay_durumu IN ('Bekliyor', 'Onaylandi', 'Reddedildi')");
                    table.ForeignKey(
                        name: "FK_zimmet_stok_personeller_olusturan_id",
                        column: x => x.olusturan_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_zimmet_stok_personeller_onaylayan_id",
                        column: x => x.onaylayan_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "zimmetler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    dokuman_no = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    zimmet_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    gsm_hat = table.Column<bool>(type: "boolean", nullable: false),
                    gsm_hat_detay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    monitor = table.Column<bool>(type: "boolean", nullable: false),
                    monitor_detay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ofis_telefonu = table.Column<bool>(type: "boolean", nullable: false),
                    ofis_telefonu_detay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cep_telefonu = table.Column<bool>(type: "boolean", nullable: false),
                    cep_telefonu_detay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    dizustu_bilgisayar = table.Column<bool>(type: "boolean", nullable: false),
                    dizustu_bilgisayar_detay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    yemek_karti = table.Column<bool>(type: "boolean", nullable: false),
                    yemek_karti_detay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    klavye = table.Column<bool>(type: "boolean", nullable: false),
                    mouse = table.Column<bool>(type: "boolean", nullable: false),
                    bilgisayar_cantasi = table.Column<bool>(type: "boolean", nullable: false),
                    bilgisayar_cantasi_detay = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    erisim_yetkileri = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    teslim_alma_notlari = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    teslim_eden = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    hazirlayan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    onaylayan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    teslim_durumu = table.Column<bool>(type: "boolean", nullable: false),
                    teslim_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    olusturma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    guncelleme_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zimmetler", x => x.id);
                    table.ForeignKey(
                        name: "FK_zimmetler_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VideoSoruCevaplar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VideoSoruId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    VerilenCevap = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    DogruMu = table.Column<bool>(type: "boolean", nullable: false),
                    AlinanPuan = table.Column<int>(type: "integer", nullable: false),
                    CevapTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoSoruCevaplar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VideoSoruCevaplar_VideoSorular_VideoSoruId",
                        column: x => x.VideoSoruId,
                        principalTable: "VideoSorular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoSoruCevaplar_personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnketAtamalar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnketId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: true),
                    DepartmanId = table.Column<int>(type: "integer", nullable: true),
                    PozisyonId = table.Column<int>(type: "integer", nullable: true),
                    AtayanPersonelId = table.Column<int>(type: "integer", nullable: false),
                    AtamaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Durum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Not = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BildirimGonderildiMi = table.Column<bool>(type: "boolean", nullable: false),
                    SonBildirimTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnketAtamalar", x => x.Id);
                    table.CheckConstraint("CK_AnketAtama_Durum", "\"Durum\" IN ('Atandı', 'Tamamlandı', 'SuresiGecti')");
                    table.ForeignKey(
                        name: "FK_AnketAtamalar_Anketler_AnketId",
                        column: x => x.AnketId,
                        principalTable: "Anketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnketAtamalar_departmanlar_DepartmanId",
                        column: x => x.DepartmanId,
                        principalTable: "departmanlar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnketAtamalar_personeller_AtayanPersonelId",
                        column: x => x.AtayanPersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnketAtamalar_personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AnketAtamalar_pozisyonlar_PozisyonId",
                        column: x => x.PozisyonId,
                        principalTable: "pozisyonlar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnketKatilimlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnketId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TamamlanmaTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TamamlandiMi = table.Column<bool>(type: "boolean", nullable: false),
                    TamamlananSoruSayisi = table.Column<int>(type: "integer", nullable: false),
                    Durum = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnketKatilimlar", x => x.Id);
                    table.CheckConstraint("CK_AnketKatilim_Durum", "\"Durum\" IN ('Başlamadı', 'Devam Ediyor', 'Tamamlandı')");
                    table.ForeignKey(
                        name: "FK_AnketKatilimlar_Anketler_AnketId",
                        column: x => x.AnketId,
                        principalTable: "Anketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnketKatilimlar_personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnketSorular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnketId = table.Column<int>(type: "integer", nullable: false),
                    SoruMetni = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SoruTipi = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Sira = table.Column<int>(type: "integer", nullable: false),
                    ZorunluMu = table.Column<bool>(type: "boolean", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnketSorular", x => x.Id);
                    table.CheckConstraint("CK_AnketSoru_Tipi", "\"SoruTipi\" IN ('TekSecim', 'CokluSecim', 'AcikUclu')");
                    table.ForeignKey(
                        name: "FK_AnketSorular_Anketler_AnketId",
                        column: x => x.AnketId,
                        principalTable: "Anketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.CheckConstraint("CK_Basvuru_Durum", "durum IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)");
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
                name: "personel_zimmet",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    zimmet_stok_id = table.Column<int>(type: "integer", nullable: false),
                    zimmet_miktar = table.Column<int>(type: "integer", nullable: false),
                    zimmet_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    iade_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    durum = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    zimmet_notu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    iade_notu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    zimmet_veren_id = table.Column<int>(type: "integer", nullable: true),
                    iade_alan_id = table.Column<int>(type: "integer", nullable: true),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    olusturma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    guncelleme_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personel_zimmet", x => x.id);
                    table.CheckConstraint("CK_PersonelZimmet_Durum", "durum IN ('Zimmetli', 'Iade Edildi')");
                    table.ForeignKey(
                        name: "FK_personel_zimmet_personeller_iade_alan_id",
                        column: x => x.iade_alan_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_personel_zimmet_personeller_personel_id",
                        column: x => x.personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_personel_zimmet_personeller_zimmet_veren_id",
                        column: x => x.zimmet_veren_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_personel_zimmet_zimmet_stok_zimmet_stok_id",
                        column: x => x.zimmet_stok_id,
                        principalTable: "zimmet_stok",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "zimmet_stok_dosyalar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zimmet_stok_id = table.Column<int>(type: "integer", nullable: false),
                    dosya_adi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    orijinal_adi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    dosya_yolu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    dosya_boyutu = table.Column<long>(type: "bigint", nullable: false),
                    mime_tipi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    olusturma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zimmet_stok_dosyalar", x => x.id);
                    table.ForeignKey(
                        name: "FK_zimmet_stok_dosyalar_zimmet_stok_zimmet_stok_id",
                        column: x => x.zimmet_stok_id,
                        principalTable: "zimmet_stok",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "zimmet_malzemeler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zimmet_id = table.Column<int>(type: "integer", nullable: false),
                    zimmet_stok_id = table.Column<int>(type: "integer", nullable: false),
                    miktar = table.Column<int>(type: "integer", nullable: false),
                    olusturma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zimmet_malzemeler", x => x.id);
                    table.ForeignKey(
                        name: "FK_zimmet_malzemeler_zimmet_stok_zimmet_stok_id",
                        column: x => x.zimmet_stok_id,
                        principalTable: "zimmet_stok",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_zimmet_malzemeler_zimmetler_zimmet_id",
                        column: x => x.zimmet_id,
                        principalTable: "zimmetler",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnketSecenekler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SoruId = table.Column<int>(type: "integer", nullable: false),
                    SecenekMetni = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Sira = table.Column<int>(type: "integer", nullable: false),
                    Aktif = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnketSecenekler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnketSecenekler_AnketSorular_SoruId",
                        column: x => x.SoruId,
                        principalTable: "AnketSorular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "AnketCevaplar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnketId = table.Column<int>(type: "integer", nullable: false),
                    SoruId = table.Column<int>(type: "integer", nullable: false),
                    PersonelId = table.Column<int>(type: "integer", nullable: false),
                    SecenekId = table.Column<int>(type: "integer", nullable: true),
                    AcikCevap = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CevapTarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnketCevaplar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnketCevaplar_AnketSecenekler_SecenekId",
                        column: x => x.SecenekId,
                        principalTable: "AnketSecenekler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AnketCevaplar_AnketSorular_SoruId",
                        column: x => x.SoruId,
                        principalTable: "AnketSorular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnketCevaplar_Anketler_AnketId",
                        column: x => x.AnketId,
                        principalTable: "Anketler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnketCevaplar_personeller_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "aday_durum_gecmisi",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    aday_id = table.Column<int>(type: "integer", nullable: false),
                    eski_durum = table.Column<int>(type: "integer", nullable: true),
                    yeni_durum = table.Column<int>(type: "integer", nullable: false),
                    degisiklik_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    degisiklik_notu = table.Column<string>(type: "text", nullable: true),
                    degistiren_personel_id = table.Column<int>(type: "integer", nullable: true),
                    otomatik_degisiklik = table.Column<bool>(type: "boolean", nullable: false),
                    ilgili_basvuru_id = table.Column<int>(type: "integer", nullable: true),
                    ilgili_mulakat_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aday_durum_gecmisi", x => x.id);
                    table.ForeignKey(
                        name: "FK_aday_durum_gecmisi_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_aday_durum_gecmisi_basvurular_ilgili_basvuru_id",
                        column: x => x.ilgili_basvuru_id,
                        principalTable: "basvurular",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_aday_durum_gecmisi_mulakatlar_ilgili_mulakat_id",
                        column: x => x.ilgili_mulakat_id,
                        principalTable: "mulakatlar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_aday_durum_gecmisi_personeller_degistiren_personel_id",
                        column: x => x.degistiren_personel_id,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aday_cv_aday_id",
                table: "aday_cv",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_deneyimler_aday_id",
                table: "aday_deneyimler",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_diller_aday_id",
                table: "aday_diller",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_durum_gecmisi_aday_id",
                table: "aday_durum_gecmisi",
                column: "aday_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_durum_gecmisi_degistiren_personel_id",
                table: "aday_durum_gecmisi",
                column: "degistiren_personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_durum_gecmisi_ilgili_basvuru_id",
                table: "aday_durum_gecmisi",
                column: "ilgili_basvuru_id");

            migrationBuilder.CreateIndex(
                name: "IX_aday_durum_gecmisi_ilgili_mulakat_id",
                table: "aday_durum_gecmisi",
                column: "ilgili_mulakat_id");

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
                name: "IX_AnketAtamalar_AnketId",
                table: "AnketAtamalar",
                column: "AnketId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketAtamalar_AtayanPersonelId",
                table: "AnketAtamalar",
                column: "AtayanPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketAtamalar_DepartmanId",
                table: "AnketAtamalar",
                column: "DepartmanId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketAtamalar_PersonelId",
                table: "AnketAtamalar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketAtamalar_PozisyonId",
                table: "AnketAtamalar",
                column: "PozisyonId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketCevaplar_AnketId",
                table: "AnketCevaplar",
                column: "AnketId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketCevaplar_PersonelId",
                table: "AnketCevaplar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketCevaplar_SecenekId",
                table: "AnketCevaplar",
                column: "SecenekId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketCevaplar_SoruId",
                table: "AnketCevaplar",
                column: "SoruId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketKatilimlar_AnketId_PersonelId",
                table: "AnketKatilimlar",
                columns: new[] { "AnketId", "PersonelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnketKatilimlar_PersonelId",
                table: "AnketKatilimlar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Anketler_OlusturanPersonelId",
                table: "Anketler",
                column: "OlusturanPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketSecenekler_SoruId",
                table: "AnketSecenekler",
                column: "SoruId");

            migrationBuilder.CreateIndex(
                name: "IX_AnketSorular_AnketId",
                table: "AnketSorular",
                column: "AnketId");

            migrationBuilder.CreateIndex(
                name: "IX_avans_talepleri_onaylayan_id",
                table: "avans_talepleri",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_avans_talepleri_personel_id",
                table: "avans_talepleri",
                column: "personel_id");

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
                name: "IX_Bildirimler_AliciId",
                table: "Bildirimler",
                column: "AliciId");

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
                name: "IX_EkranYetkileri_EkranKodu",
                table: "EkranYetkileri",
                column: "EkranKodu",
                unique: true);

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
                name: "IX_istifa_talepleri_onaylayan_id",
                table: "istifa_talepleri",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_istifa_talepleri_personel_id",
                table: "istifa_talepleri",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_izin_talepleri_IzinTipiId",
                table: "izin_talepleri",
                column: "IzinTipiId");

            migrationBuilder.CreateIndex(
                name: "IX_izin_talepleri_onaylayan_id",
                table: "izin_talepleri",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_izin_talepleri_personel_id",
                table: "izin_talepleri",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_izin_tipleri_izin_tipi_adi",
                table: "izin_tipleri",
                column: "izin_tipi_adi",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KademeEkranYetkileri_EkranYetkisiId",
                table: "KademeEkranYetkileri",
                column: "EkranYetkisiId");

            migrationBuilder.CreateIndex(
                name: "IX_KademeEkranYetkileri_KademeId_EkranYetkisiId",
                table: "KademeEkranYetkileri",
                columns: new[] { "KademeId", "EkranYetkisiId" },
                unique: true);

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
                name: "IX_kesinti_tanimlari_kod",
                table: "kesinti_tanimlari",
                column: "kod",
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
                name: "IX_kvkk_izin_metinleri_olusturan_personel_id",
                table: "kvkk_izin_metinleri",
                column: "olusturan_personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_luca_bordrolar_personel_id",
                table: "luca_bordrolar",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_masraf_talepleri_onaylayan_id",
                table: "masraf_talepleri",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_masraf_talepleri_personel_id",
                table: "masraf_talepleri",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_mulakatlar_basvuru_id",
                table: "mulakatlar",
                column: "basvuru_id");

            migrationBuilder.CreateIndex(
                name: "IX_mulakatlar_mulakat_yapan_id",
                table: "mulakatlar",
                column: "mulakat_yapan_id");

            migrationBuilder.CreateIndex(
                name: "IX_odeme_tanimlari_kod",
                table: "odeme_tanimlari",
                column: "kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_otp_kodlar_kullanici_id",
                table: "otp_kodlar",
                column: "kullanici_id");

            migrationBuilder.CreateIndex(
                name: "IX_otp_kodlar_luca_bordro_id",
                table: "otp_kodlar",
                column: "luca_bordro_id");

            migrationBuilder.CreateIndex(
                name: "IX_personel_egitimleri_egitim_id",
                table: "personel_egitimleri",
                column: "egitim_id");

            migrationBuilder.CreateIndex(
                name: "IX_personel_egitimleri_personel_id",
                table: "personel_egitimleri",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_personel_giris_cikis_personel_id",
                table: "personel_giris_cikis",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_personel_zimmet_iade_alan_id",
                table: "personel_zimmet",
                column: "iade_alan_id");

            migrationBuilder.CreateIndex(
                name: "IX_personel_zimmet_personel_id",
                table: "personel_zimmet",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_personel_zimmet_zimmet_stok_id",
                table: "personel_zimmet",
                column: "zimmet_stok_id");

            migrationBuilder.CreateIndex(
                name: "IX_personel_zimmet_zimmet_veren_id",
                table: "personel_zimmet",
                column: "zimmet_veren_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_puantajlar_onaylayan_id",
                table: "puantajlar",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_puantajlar_personel_id_donem_yil_donem_ay",
                table: "puantajlar",
                columns: new[] { "personel_id", "donem_yil", "donem_ay" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sehirler_PlakaKodu",
                table: "Sehirler",
                column: "PlakaKodu",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sehirler_SehirAd",
                table: "Sehirler",
                column: "SehirAd",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teklif_mektuplari_basvuru_id",
                table: "teklif_mektuplari",
                column: "basvuru_id");

            migrationBuilder.CreateIndex(
                name: "IX_teklif_mektuplari_hazirlayan_id",
                table: "teklif_mektuplari",
                column: "hazirlayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAtamalar_AtayanPersonelId",
                table: "VideoAtamalar",
                column: "AtayanPersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAtamalar_DepartmanId",
                table: "VideoAtamalar",
                column: "DepartmanId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAtamalar_PersonelId",
                table: "VideoAtamalar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAtamalar_PozisyonId",
                table: "VideoAtamalar",
                column: "PozisyonId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoAtamalar_VideoEgitimId",
                table: "VideoAtamalar",
                column: "VideoEgitimId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoEgitimler_KategoriId",
                table: "VideoEgitimler",
                column: "KategoriId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoIzlemeler_PersonelId",
                table: "VideoIzlemeler",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoIzlemeler_VideoEgitimId",
                table: "VideoIzlemeler",
                column: "VideoEgitimId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoSertifikalar_PersonelId",
                table: "VideoSertifikalar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoSertifikalar_VideoEgitimId",
                table: "VideoSertifikalar",
                column: "VideoEgitimId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoSoruCevaplar_PersonelId",
                table: "VideoSoruCevaplar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoSoruCevaplar_VideoSoruId",
                table: "VideoSoruCevaplar",
                column: "VideoSoruId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoSorular_VideoEgitimId",
                table: "VideoSorular",
                column: "VideoEgitimId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoYorumlar_PersonelId",
                table: "VideoYorumlar",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_VideoYorumlar_VideoEgitimId",
                table: "VideoYorumlar",
                column: "VideoEgitimId");

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_malzemeler_zimmet_id",
                table: "zimmet_malzemeler",
                column: "zimmet_id");

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_malzemeler_zimmet_stok_id",
                table: "zimmet_malzemeler",
                column: "zimmet_stok_id");

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_stok_olusturan_id",
                table: "zimmet_stok",
                column: "olusturan_id");

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_stok_onaylayan_id",
                table: "zimmet_stok",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_stok_dosyalar_zimmet_stok_id",
                table: "zimmet_stok_dosyalar",
                column: "zimmet_stok_id");

            migrationBuilder.CreateIndex(
                name: "IX_zimmetler_personel_id",
                table: "zimmetler",
                column: "personel_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aday_cv");

            migrationBuilder.DropTable(
                name: "aday_deneyimler");

            migrationBuilder.DropTable(
                name: "aday_diller");

            migrationBuilder.DropTable(
                name: "aday_durum_gecmisi");

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

            migrationBuilder.DropTable(
                name: "aday_yetenekler");

            migrationBuilder.DropTable(
                name: "AnketAtamalar");

            migrationBuilder.DropTable(
                name: "AnketCevaplar");

            migrationBuilder.DropTable(
                name: "AnketKatilimlar");

            migrationBuilder.DropTable(
                name: "avans_talepleri");

            migrationBuilder.DropTable(
                name: "Bildirimler");

            migrationBuilder.DropTable(
                name: "bordro_kesintiler");

            migrationBuilder.DropTable(
                name: "bordro_odemeler");

            migrationBuilder.DropTable(
                name: "bordro_onaylar");

            migrationBuilder.DropTable(
                name: "bordro_parametreleri");

            migrationBuilder.DropTable(
                name: "eposta_ayarlari");

            migrationBuilder.DropTable(
                name: "eposta_yonlendirme");

            migrationBuilder.DropTable(
                name: "firma_ayarlari");

            migrationBuilder.DropTable(
                name: "istifa_talepleri");

            migrationBuilder.DropTable(
                name: "izin_talepleri");

            migrationBuilder.DropTable(
                name: "KademeEkranYetkileri");

            migrationBuilder.DropTable(
                name: "kvkk_izin_metinleri");

            migrationBuilder.DropTable(
                name: "luca_bordro_ayarlari");

            migrationBuilder.DropTable(
                name: "masraf_talepleri");

            migrationBuilder.DropTable(
                name: "otp_kodlar");

            migrationBuilder.DropTable(
                name: "personel_egitimleri");

            migrationBuilder.DropTable(
                name: "personel_giris_cikis");

            migrationBuilder.DropTable(
                name: "personel_zimmet");

            migrationBuilder.DropTable(
                name: "Sehirler");

            migrationBuilder.DropTable(
                name: "teklif_mektuplari");

            migrationBuilder.DropTable(
                name: "VideoAtamalar");

            migrationBuilder.DropTable(
                name: "VideoIzlemeler");

            migrationBuilder.DropTable(
                name: "VideoSertifikalar");

            migrationBuilder.DropTable(
                name: "VideoSoruCevaplar");

            migrationBuilder.DropTable(
                name: "VideoYorumlar");

            migrationBuilder.DropTable(
                name: "zimmet_malzemeler");

            migrationBuilder.DropTable(
                name: "zimmet_stok_dosyalar");

            migrationBuilder.DropTable(
                name: "mulakatlar");

            migrationBuilder.DropTable(
                name: "AnketSecenekler");

            migrationBuilder.DropTable(
                name: "kesinti_tanimlari");

            migrationBuilder.DropTable(
                name: "odeme_tanimlari");

            migrationBuilder.DropTable(
                name: "bordro_ana");

            migrationBuilder.DropTable(
                name: "izin_tipleri");

            migrationBuilder.DropTable(
                name: "EkranYetkileri");

            migrationBuilder.DropTable(
                name: "kullanicilar");

            migrationBuilder.DropTable(
                name: "luca_bordrolar");

            migrationBuilder.DropTable(
                name: "egitimler");

            migrationBuilder.DropTable(
                name: "VideoSorular");

            migrationBuilder.DropTable(
                name: "zimmetler");

            migrationBuilder.DropTable(
                name: "zimmet_stok");

            migrationBuilder.DropTable(
                name: "basvurular");

            migrationBuilder.DropTable(
                name: "AnketSorular");

            migrationBuilder.DropTable(
                name: "puantajlar");

            migrationBuilder.DropTable(
                name: "VideoEgitimler");

            migrationBuilder.DropTable(
                name: "adaylar");

            migrationBuilder.DropTable(
                name: "is_ilanlari");

            migrationBuilder.DropTable(
                name: "Anketler");

            migrationBuilder.DropTable(
                name: "VideoKategoriler");

            migrationBuilder.DropTable(
                name: "ilan_kategoriler");

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
