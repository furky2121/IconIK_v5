using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddZimmetMalzemeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "zimmet_malzemeler",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    zimmet_id = table.Column<int>(type: "integer", nullable: false),
                    zimmet_stok_id = table.Column<int>(type: "integer", nullable: false),
                    miktar = table.Column<int>(type: "integer", nullable: false),
                    olusturma_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zimmet_malzemeler", x => x.id);
                    table.ForeignKey(
                        name: "FK_zimmet_malzemeler_zimmet_stok_zimmet_stok_id",
                        column: x => x.zimmet_stok_id,
                        principalTable: "zimmet_stok",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_zimmet_malzemeler_zimmetler_zimmet_id",
                        column: x => x.zimmet_id,
                        principalTable: "zimmetler",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_malzemeler_zimmet_id",
                table: "zimmet_malzemeler",
                column: "zimmet_id");

            migrationBuilder.CreateIndex(
                name: "IX_zimmet_malzemeler_zimmet_stok_id",
                table: "zimmet_malzemeler",
                column: "zimmet_stok_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "zimmet_malzemeler");
        }
    }
}
