using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt.Model
{
    [Table("szallas_szoba")]
    public class Szallas_szoba
    {
        [Column("Szid")]
        public int Szid { get; set; }
        [Column("Sid")]
        public int Sid { get; set; }

        [ForeignKey("Szid")]
        public Szallas? Szallas { get; set; }
        [ForeignKey("Sid")]
        public Szoba? Szoba { get; set; }
    }
}
