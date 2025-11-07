using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAnketTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    table.CheckConstraint("CK_Anket_Durum", "\"AnketDurumu\" IN ('Taslak', 'Aktif', 'Tamamlandý')");
                    table.ForeignKey(
                        name: "FK_Anketler_personeller_OlusturanPersonelId",
                        column: x => x.OlusturanPersonelId,
                        principalTable: "personeller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.CheckConstraint("CK_AnketAtama_Durum", "\"Durum\" IN ('Atandý', 'Tamamlandý', 'SuresiGecti')");
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
                    table.CheckConstraint("CK_AnketKatilim_Durum", "\"Durum\" IN ('Baþlamadý', 'Devam Ediyor', 'Tamamlandý')");
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnketAtamalar");

            migrationBuilder.DropTable(
                name: "AnketCevaplar");

            migrationBuilder.DropTable(
                name: "AnketKatilimlar");

            migrationBuilder.DropTable(
                name: "AnketSecenekler");

            migrationBuilder.DropTable(
                name: "AnketSorular");

            migrationBuilder.DropTable(
                name: "Anketler");
        }
    }
}
