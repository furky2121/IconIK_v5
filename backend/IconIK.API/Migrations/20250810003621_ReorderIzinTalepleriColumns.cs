using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IconIK.API.Migrations
{
    /// <inheritdoc />
    public partial class ReorderIzinTalepleriColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL'de sütun sýrasýný deðiþtirmek için tablo yeniden oluþturulmalý
            migrationBuilder.Sql(@"
                -- Geçici tablo oluþtur
                CREATE TABLE izin_talepleri_temp AS 
                SELECT id, personel_id, izin_baslama_tarihi, isbasi_tarihi, gun_sayisi, 
                       izin_tipi, aciklama, durum, onaylayan_id, onay_tarihi, onay_notu, 
                       created_at, updated_at 
                FROM izin_talepleri;

                -- Eski tabloyu sil
                DROP TABLE izin_talepleri CASCADE;

                -- Yeni tabloyu doðru sütun sýrasý ile oluþtur
                CREATE TABLE izin_talepleri (
                    id SERIAL PRIMARY KEY,
                    personel_id INTEGER NOT NULL,
                    izin_baslama_tarihi TIMESTAMP WITH TIME ZONE NOT NULL,
                    isbasi_tarihi TIMESTAMP WITH TIME ZONE NOT NULL,
                    gun_sayisi INTEGER NOT NULL,
                    izin_tipi VARCHAR(50) NOT NULL DEFAULT 'Yýllýk Ýzin',
                    aciklama TEXT,
                    durum VARCHAR(20) NOT NULL DEFAULT 'Beklemede',
                    onaylayan_id INTEGER,
                    onay_tarihi TIMESTAMP WITH TIME ZONE,
                    onay_notu TEXT,
                    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
                );

                -- Verileri geri kopyala
                INSERT INTO izin_talepleri 
                SELECT * FROM izin_talepleri_temp;

                -- Geçici tabloyu sil
                DROP TABLE izin_talepleri_temp;

                -- Foreign key constraint'leri yeniden oluþtur
                ALTER TABLE izin_talepleri 
                ADD CONSTRAINT ""FK_izin_talepleri_personeller_PersonelId"" 
                FOREIGN KEY (personel_id) REFERENCES personeller(id) ON DELETE CASCADE;

                ALTER TABLE izin_talepleri 
                ADD CONSTRAINT ""FK_izin_talepleri_personeller_OnaylayanId"" 
                FOREIGN KEY (onaylayan_id) REFERENCES personeller(id) ON DELETE SET NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
