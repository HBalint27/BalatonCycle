using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt.Model
{
    [Table("szolgaltatasok")]
    public class Szolgaltatasok
    {
        [Key]
        public int Szoid { get; set; }
        [Column("Nev")]
        public string? Nev { get; set; }
        
       
    }
}
