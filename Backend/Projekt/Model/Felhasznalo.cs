using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Projekt.Model
{
    [Table("felhasznalo")]
    public class Felhasznalo
    {
        [Key]
        public int Fid { get; set; }
        [Column("Nev")]
        public string? Nev { get; set; }
        [Column("Email")]
        public string? Email { get; set; }
        [Column("Jelszo")]
        public string? Jelszo { get; set; }
        [Column("Lakcim")]
        public string? Lakcim { get; set; }
        [Column("Telefonszam")]
        public long Telefonszam { get; set; }
        [Column("Statusz")]
        public string? Statusz { get; set; } = "User";
    }
}
