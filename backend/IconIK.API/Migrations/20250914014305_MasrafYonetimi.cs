using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class MasrafYonetimi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_masraf_talepleri_onaylayan_id",
                table: "masraf_talepleri",
                column: "onaylayan_id");

            migrationBuilder.CreateIndex(
                name: "IX_masraf_talepleri_personel_id",
                table: "masraf_talepleri",
                column: "personel_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "masraf_talepleri");
        }
    }
}
