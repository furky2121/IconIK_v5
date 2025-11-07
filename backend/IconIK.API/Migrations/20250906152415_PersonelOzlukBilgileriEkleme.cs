using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class PersonelOzlukBilgileriEkleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "acil_durum_iletisim",
                table: "personeller",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "anne_adi",
                table: "personeller",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "askerlik_durumu",
                table: "personeller",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "baba_adi",
                table: "personeller",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "banka_hesap_no",
                table: "personeller",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cinsiyet",
                table: "personeller",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dogum_yeri",
                table: "personeller",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "egitim_durumu",
                table: "personeller",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ehliyet_sinifi",
                table: "personeller",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "iban_no",
                table: "personeller",
                type: "character varying(34)",
                maxLength: 34,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "kan_grubu",
                table: "personeller",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "medeni_hal",
                table: "personeller",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nufus_il_kod",
                table: "personeller",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nufus_ilce_kod",
                table: "personeller",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "acil_durum_iletisim",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "anne_adi",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "askerlik_durumu",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "baba_adi",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "banka_hesap_no",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "cinsiyet",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "dogum_yeri",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "egitim_durumu",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "ehliyet_sinifi",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "iban_no",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "kan_grubu",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "medeni_hal",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "nufus_il_kod",
                table: "personeller");

            migrationBuilder.DropColumn(
                name: "nufus_ilce_kod",
                table: "personeller");
        }
    }
}
