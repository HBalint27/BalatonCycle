using Projekt.Model;
using Microsoft.EntityFrameworkCore;
using Projekt.Auth; 

namespace TestProject1
{
    internal class DbContextHelper
    {
        public static Context CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
            .Options;

            var context = new Context(options);

            // 1. Értékelések
            context.Ertekelesek.AddRange(
                new Ertekeles { Eid = 1, Szoveg = "Szuper volt", Pont = 5, Datum = DateTime.Now },
                new Ertekeles { Eid = 2, Szoveg = "Elmegy", Pont = 3, Datum = DateTime.Now }
            );

            // 2. Felhasználók
            context.Felhasznalok.Add(new Felhasznalo
            {
                Fid = 1,
                Nev = "Teszt Elek",
                Email = "test@test.hu",
                Jelszo = PasswordHandler.HashPassword("helyes_jelszo")
            });

            // 3. Tulajdonosok
            context.Tulajdonosok.Add(new Tulajdonos
            {
                Tid = 1,
                Nev = "Gazda Gábor",
                Email = "gazda@gmail.com"
            });

            // 4. Szállások
            context.Szallasok.Add(new Szallas
            {
                Szid = 1,
                SzallasCime = "Teszt utca 1.",
                Ar = 15000,
                Tid = 1
            });

            // 5. Szobák
            context.Szobak.Add(new Szoba
            {
                Sid = 1,               
                Leiras = "Kényelmes szoba",
                Statusz = "Szabad"
            });

            // 6. Szolgáltatások
            context.Szolgaltatasok.Add(new Szolgaltatasok
            {
                Szoid = 1,
                Nev = "Wifi"                
            });

            // 7. Foglalások
            context.Foglalasok.Add(new Foglal
            {
                Fid = 1,
                Szid = 1,
                ErkezesNap = new DateTime(2026, 05, 10)
            });

            // 8. Kapcsolótábla (Szállás-Szolgáltatás)
            context.Szallas_Szolgaltatas.Add(new Szallas_szolgaltatas
            {
                Szid = 1,
                Szoid = 1
            });

            context.SaveChanges();
            return context;
        }
    }
}
