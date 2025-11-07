using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailConfigurationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eposta_ayarlari",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    smtp_host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    smtp_port = table.Column<int>(type: "integer", nullable: false),
                    smtp_username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    smtp_password = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    enable_ssl = table.Column<bool>(type: "boolean", nullable: false),
                    from_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    from_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eposta_ayarlari", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "eposta_yonlendirme",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    yonlendirme_turu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    alici_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    gonderim_saati = table.Column<TimeSpan>(type: "interval", nullable: false),
                    aktif = table.Column<bool>(type: "boolean", nullable: false),
                    son_gonderim_tarihi = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    aciklama = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eposta_yonlendirme", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eposta_ayarlari");

            migrationBuilder.DropTable(
                name: "eposta_yonlendirme");
        }
    }
}
