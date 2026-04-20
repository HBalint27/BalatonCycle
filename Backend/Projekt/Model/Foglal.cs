using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt.Model
{
    [Table("foglal")]
    public class Foglal
    {
        [Column("Fid")]
        public int Fid {  get; set; }
        [Column("Szid")]
        public int Szid { get; set; }
        [Column("ErkezesNap")]
        public DateTime ErkezesNap { get; set; }

        [ForeignKey("Fid")]
        public Felhasznalo? Felhasznalo { get; set; }
        [ForeignKey("Szid")]
        public Szallas? Szallas { get; set; }
    }
}
