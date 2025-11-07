using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AvansVeIstifaTablolarý : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_avans_talepleri_onaylayan_id",
                table: "avans_talepleri",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_avans_talepleri_personel_id",
                table: "avans_talepleri",
                column: "personel_id");

            migrationBuilder.CreateIndex(
                name: "IX_istifa_talepleri_onaylayan_id",
                table: "istifa_talepleri",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_istifa_talepleri_personel_id",
                table: "istifa_talepleri",
                column: "personel_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avans_talepleri");

            migrationBuilder.DropTable(
                name: "istifa_talepleri");
        }
    }
}
