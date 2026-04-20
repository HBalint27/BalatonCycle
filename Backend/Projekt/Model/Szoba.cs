using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // A List-hez szükséges

namespace Projekt.Model
{
    [Table("szoba")]
    public class Szoba
    {
        [Key]
        public int Sid { get; set; }
        [Column("Statusz")]
        public string? Statusz { get; set; }
        [Column("szid")]
        public int Szid { get; set; }

        [ForeignKey("Szid")]
        public virtual Szallas? Szallas { get; set; }

        public virtual ICollection<Kepek> Kepek { get; set; } = new List<Kepek>();
    }
}