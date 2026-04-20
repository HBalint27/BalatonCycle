using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt.Model
{
    [Table("szallas_szolgaltatas")]
    public class Szallas_szolgaltatas
    {
        [Column("Szid")]
        public int Szid { get; set; }
        [Column("Szoid")]
        public int Szoid { get; set; }

        [ForeignKey("Szid")]
        public Szallas? Szallas { get; set; }
        [ForeignKey("Szoid")]
        public Szolgaltatasok? Szolgaltatasok { get; set; }
    }
}
