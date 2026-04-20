using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt.Model
{
    [Table("szallas_ertekeles")]
    public class Szallas_ertekeles
    {        
        [Column("Szid")]
        public int Szid { get; set; }       
        [Column("Eid")]
        public int Eid { get; set; }

        [ForeignKey("Szid")]
        public Szallas? Szallas { get; set; }
        [ForeignKey("Eid")]
        public Ertekeles? Ertekeles { get; set; }
    }
}
