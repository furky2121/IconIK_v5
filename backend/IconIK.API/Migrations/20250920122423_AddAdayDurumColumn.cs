using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAdayDurumColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Basvuru_Durum",
                table: "basvurular");

            migrationBuilder.AddColumn<string>(
                name: "cv_dosya_yolu",
                table: "adaylar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "durum",
                table: "adaylar",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "durum_guncelleme_notu",
                table: "adaylar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "durum_guncelleme_tarihi",
                table: "adaylar",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "otomatik_cv_olusturuldu",
                table: "adaylar",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                    table.CheckConstraint("CK_AdayCV_CVTipi", "cv_tipi IN ('Otomatik', 'Yuklenmiþ')");
                    table.ForeignKey(
                        name: "FK_aday_cv_adaylar_aday_id",
                        column: x => x.aday_id,
                        principalTable: "adaylar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.AddCheckConstraint(
                name: "CK_Basvuru_Durum",
                table: "basvurular",
                sql: "durum IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Aday_Durum",
                table: "adaylar",
                sql: "durum IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)");

            migrationBuilder.CreateIndex(
                name: "IX_aday_cv_aday_id",
                table: "aday_cv",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aday_cv");

            migrationBuilder.DropTable(
                name: "aday_durum_gecmisi");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Basvuru_Durum",
                table: "basvurular");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Aday_Durum",
                table: "adaylar");

            migrationBuilder.DropColumn(
                name: "cv_dosya_yolu",
                table: "adaylar");

            migrationBuilder.DropColumn(
                name: "durum",
                table: "adaylar");

            migrationBuilder.DropColumn(
                name: "durum_guncelleme_notu",
                table: "adaylar");

            migrationBuilder.DropColumn(
                name: "durum_guncelleme_tarihi",
                table: "adaylar");

            migrationBuilder.DropColumn(
                name: "otomatik_cv_olusturuldu",
                table: "adaylar");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Basvuru_Durum",
                table: "basvurular",
                sql: "durum IN (1, 2, 3, 4, 5, 6, 7, 8)");
        }
    }
}
