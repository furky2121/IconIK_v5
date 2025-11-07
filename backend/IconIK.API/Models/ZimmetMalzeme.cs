using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IconIK.API.Models
{
    [Table("zimmet_malzemeler")]
    public class ZimmetMalzeme
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("zimmet_id")]
        public int ZimmetId { get; set; }

        [Column("zimmet_stok_id")]
        public int ZimmetStokId { get; set; }

        [Column("miktar")]
        public int Miktar { get; set; } = 1;

        [Column("olusturma_tarihi")]
        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        [ForeignKey("ZimmetId")]
        public virtual Zimmet? Zimmet { get; set; }

        [ForeignKey("ZimmetStokId")]
        public virtual ZimmetStok? ZimmetStok { get; set; }
    }
}