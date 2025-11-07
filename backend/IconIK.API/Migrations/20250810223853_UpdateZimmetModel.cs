using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateZimmetModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_zimmetler_personel_id",
                table: "zimmetler",
                column: "personel_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "zimmetler");
        }
    }
}
