using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MySqlX.XDevAPI.Relational;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt.Model
{
    [Table("ertekeles")]
    public class Ertekeles
    {
        [Key]
        public int Eid { get; set; }
        [Column("Szoveg")]
        public string? Szoveg { get; set; }
        [Column("Datum")]
        public DateTime Datum { get; set; }
        [Column("Pont")]
        public int Pont {  get; set; }
        [Column("Szid")]
        public int Szid { get; set; }
        [ForeignKey("Szid")]
        public Szallas? Szallas { get; set; }
               
    }
}
