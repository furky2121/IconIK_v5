using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class FixSequenceIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Sequence'ý tablodaki en büyük id'ye göre resetle
            migrationBuilder.Sql("SELECT setval('izin_talepleri_id_seq', (SELECT COALESCE(MAX(id), 1) FROM izin_talepleri))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
