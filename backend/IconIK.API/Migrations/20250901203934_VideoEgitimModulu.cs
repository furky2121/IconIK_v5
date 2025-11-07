using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class VideoEgitimModulu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bordrolar");

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
                    IpAdresi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
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
                    SertifikaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "VideoSorular");

            migrationBuilder.DropTable(
                name: "VideoEgitimler");

            migrationBuilder.DropTable(
                name: "VideoKategoriler");

            migrationBuilder.CreateTable(
                name: "bordrolar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personel_id = table.Column<int>(type: "integer", nullable: false),
                    brut_maas = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    diger_kesintiler = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    donem_ay = table.Column<int>(type: "integer", nullable: false),
                    donem_yil = table.Column<int>(type: "integer", nullable: false),
                    mesai_odemeler = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    net_maas = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    prim_odemeler = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    sgk_kesinti = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    vergi_kesinti = table.Column<decimal>(type: "numeric(10,2)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_bordrolar_personel_id_donem_yil_donem_ay",
                table: "bordrolar",
                columns: new[] { "personel_id", "donem_yil", "donem_ay" },
                unique: true);
        }
    }
}
