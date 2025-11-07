using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddZimmetStokTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_stok_olusturan_id",
                table: "zimmet_stok",
                column: "olusturan_id");

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_stok_onaylayan_id",
                table: "zimmet_stok",
                column: "onaylayan_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "zimmet_stok");
        }
    }
}
