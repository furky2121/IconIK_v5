using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonelZimmetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "personel_zimmet");
        }
    }
}
