using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("zimmetler")]
    public class Zimmet
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("personel_id")]
        public int PersonelId { get; set; }

        [Column("dokuman_no")]
        [StringLength(50)]
        public string DokumanNo { get; set; } = "BLG.ZM.001";

        [Column("zimmet_tarihi")]
        public DateTime ZimmetTarihi { get; set; } = DateTime.UtcNow;

        // Zimmetlenen Malzemeler
        [Column("gsm_hat")]
        public bool GsmHat { get; set; }

        [Column("gsm_hat_detay")]
        [StringLength(200)]
        public string? GsmHatDetay { get; set; }

        [Column("monitor")]
        public bool Monitor { get; set; }

        [Column("monitor_detay")]
        [StringLength(200)]
        public string? MonitorDetay { get; set; }

        [Column("ofis_telefonu")]
        public bool OfisTelefonu { get; set; }

        [Column("ofis_telefonu_detay")]
        [StringLength(200)]
        public string? OfisTelefonuDetay { get; set; }

        [Column("cep_telefonu")]
        public bool CepTelefonu { get; set; }

        [Column("cep_telefonu_detay")]
        [StringLength(200)]
        public string? CepTelefonuDetay { get; set; }

        [Column("dizustu_bilgisayar")]
        public bool DizustuBilgisayar { get; set; }

        [Column("dizustu_bilgisayar_detay")]
        [StringLength(200)]
        public string? DizustuBilgisayarDetay { get; set; }

        [Column("yemek_karti")]
        public bool YemekKarti { get; set; }

        [Column("yemek_karti_detay")]
        [StringLength(200)]
        public string? YemekKartiDetay { get; set; }

        [Column("klavye")]
        public bool Klavye { get; set; }

        [Column("mouse")]
        public bool Mouse { get; set; }

        [Column("bilgisayar_cantasi")]
        public bool BilgisayarCantasi { get; set; }

        [Column("bilgisayar_cantasi_detay")]
        [StringLength(200)]
        public string? BilgisayarCantasiDetay { get; set; }

        [Column("erisim_yetkileri")]
        [StringLength(500)]
        public string? ErisimYetkileri { get; set; }

        [Column("teslim_alma_notlari")]
        [StringLength(500)]
        public string? TeslimAlmaNotlari { get; set; }

        [Column("teslim_eden")]
        [StringLength(100)]
        public string? TeslimEden { get; set; }

        [Column("hazirlayan")]
        [StringLength(100)]
        public string? Hazirlayan { get; set; } = "ÇAĞLA YILDIRIM";

        [Column("onaylayan")]
        [StringLength(100)]
        public string? Onaylayan { get; set; } = "EMRE HACIEVLİYAGİL";

        [Column("teslim_durumu")]
        public bool TeslimDurumu { get; set; } = false;

        [Column("teslim_tarihi")]
        public DateTime? TeslimTarihi { get; set; }

        [Column("aktif")]
        public bool Aktif { get; set; } = true;

        [Column("olusturma_tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        [Column("guncelleme_tarihi")]
        public DateTime? GuncellemeTarihi { get; set; }

        [ForeignKey("PersonelId")]
        public virtual Personel? Personel { get; set; }
    }
}