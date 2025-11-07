using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddScreenPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_EkranYetkileri_EkranKodu",
                table: "EkranYetkileri",
                column: "EkranKodu",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KademeEkranYetkileri");

            migrationBuilder.DropTable(
                name: "EkranYetkileri");
        }
    }
}
