using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Projekt.Model
{
    [Table("szallas")]
    public class Szallas
    {
        [Key]
        public int Szid { get; set; }

        // --- NEW COLUMNS INSTEAD OF SzallasCime ---
        [Column("Nev")]
        public string? Nev { get; set; }

        [Column("Iranyitoszam")]
        public int Iranyitoszam { get; set; }

        [Column("Telepules")]
        public string? Telepules { get; set; }

        [Column("Utca")]
        public string? Utca { get; set; }

        [Column("Hazszam")]
        public string? Hazszam { get; set; }
        // ------------------------------------------

        [Column("Ar")]
        public int Ar { get; set; }

        [Column("Tid")]
        public int Tid { get; set; }

        [Column("lat")]
        public double? Lat { get; set; }

        [Column("lon")]
        public double? Lon { get; set; }

        [Column("leiras")]
        public string? Leiras { get; set; }

        [Column("szallaskep")]
        public string? Szallaskep { get; set; }

        [ForeignKey("Tid")]
        public virtual Tulajdonos? Tulajdonos { get; set; }

        public virtual ICollection<Szoba> Szobak { get; set; } = new List<Szoba>();

        public virtual ICollection<Szallas_szolgaltatas> SzallasSzolgaltatasok { get; set; } = new List<Szallas_szolgaltatas>();
        public virtual ICollection<Ertekeles> Ertekelesek { get; set; } = new List<Ertekeles>();

        public virtual ICollection<Foglal> Foglalasok { get; set; } = new List<Foglal>();
    }
}