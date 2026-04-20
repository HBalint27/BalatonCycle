using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt.Model
{    [Table("kepek")]
    public class Kepek
    {
        [Key]
        public int Kid { get; set; }
        [Column("Sid")]
        public int Sid { get; set; }
        [Column("Fajlnev")]
        public string? Fajlnev { get; set; }
        [Column("feltoltve_ekkor")]
        public DateTime FeltoltveEkkor { get; set; }

        [ForeignKey("Sid")]
        public Szoba? Szoba { get; set; }
    }
}