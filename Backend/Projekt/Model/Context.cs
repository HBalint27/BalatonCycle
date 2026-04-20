using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Projekt.Model
{
    public class Context : DbContext
    {
        public DbSet<Ertekeles> Ertekelesek { get; set; }
        public DbSet<Felhasznalo> Felhasznalok { get; set; }
        public DbSet<Szallas> Szallasok { get; set; }
        public DbSet<Szoba> Szobak { get; set; }
        public DbSet<Szolgaltatasok> Szolgaltatasok { get; set; }
        public DbSet<Tulajdonos> Tulajdonosok { get; set; }
        public DbSet<Foglal> Foglalasok { get; set; }
        public DbSet<Szallas_szolgaltatas> Szallas_Szolgaltatas { get; set; }
        public DbSet<Kepek> Kepek { get; set; }

        // --- ADDED THESE TWO LINES TO FIX THE 10 ERRORS ---
        public DbSet<Szallas_ertekeles> Szallas_Ertekeles { get; set; }
        public DbSet<Szallas_szoba> Szallas_Szoba { get; set; }

        public Context(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Foglal>()
                .HasKey(f => new { f.Fid, f.Szid, f.ErkezesNap });

            modelBuilder.Entity<Szallas_szolgaltatas>()
                .HasKey(x => new { x.Szid, x.Szoid });

            /* * Note: Because Szallas_ertekeles and Szallas_szoba are likely "junction tables" 
             * (kapcsolótáblák) just like Foglal and Szallas_szolgaltatas, you *might* need to 
             * define their composite keys here later using .HasKey() if Entity Framework 
             * throws a new error about them missing a Primary Key. But for now, let's see 
             * if adding the DbSets above lets the project compile!
             */
            // --- ADD THESE NEW LINES ---

            // Assuming the keys are Szid (Szállás ID) and Eid (Értékelés ID). 
            // Check your Szallas_ertekeles.cs file to make sure these property names match exactly!
            modelBuilder.Entity<Szallas_ertekeles>()
                .HasKey(x => new { x.Szid, x.Eid });

            // Assuming the keys are Szid and SzobId. 
            // Check your Szallas_szoba.cs file to make sure these property names match exactly!
            modelBuilder.Entity<Szallas_szoba>()
                .HasKey(x => new { x.Szid, x.Sid });
        }
    }
}