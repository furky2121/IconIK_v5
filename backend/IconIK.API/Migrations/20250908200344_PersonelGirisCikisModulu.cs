using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class PersonelGirisCikisModulu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_personel_giris_cikis_personel_id",
                table: "personel_giris_cikis",
                column: "personel_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "personel_giris_cikis");
        }
    }
}
