using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddZimmetStokDosya : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "zimmet_stok_dosyalar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zimmet_stok_id = table.Column<int>(type: "integer", nullable: false),
                    dosya_adi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    orijinal_adi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    dosya_yolu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    dosya_boyutu = table.Column<long>(type: "bigint", nullable: false),
                    mime_tipi = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    olusturma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zimmet_stok_dosyalar", x => x.id);
                    table.ForeignKey(
                        name: "FK_zimmet_stok_dosyalar_zimmet_stok_zimmet_stok_id",
                        column: x => x.zimmet_stok_id,
                        principalTable: "zimmet_stok",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_stok_dosyalar_zimmet_stok_id",
                table: "zimmet_stok_dosyalar",
                column: "zimmet_stok_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "zimmet_stok_dosyalar");
        }
    }
}
