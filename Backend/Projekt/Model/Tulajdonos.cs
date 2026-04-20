using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt.Model
{
    [Table("tulajdonos")]
    public class Tulajdonos
    {
        [Key]
        public int Tid { get; set; }
        [Column("Nev")]
        public string? Nev { get; set; }
        [Column("Email")]
        public string? Email { get; set; }
        [Column("Telefonszam")]
        public long Telefonszam { get; set; }
        [Column("fid")]
        public int Fid { get; set; }

        [ForeignKey("Fid")]
        public virtual Felhasznalo? Felhasznalo { get; set; }

        public virtual ICollection<Szallas> Szallasok { get; set; } = new List<Szallas>();
    }
}
