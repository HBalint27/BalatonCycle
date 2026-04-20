using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Controllers;
using Projekt.Model;

namespace TestProject1
{
    [TestClass]
    public sealed class ErtekelesControllerTest
    {
        private ErtekelesController? _sut;
        private Context _context;

        [TestInitialize]
        public void Initialize()
        {
            // Meghívjuk a segédosztályodat az adatbázis létrehozásához
            _context = DbContextHelper.CreateDbContext();
            _sut = new ErtekelesController(_context);
        }

        // 1. POZITÍV TESZT: Összes adat lekérése
        [TestMethod]
        public async Task Get_ReturnsOkResult_WithAllData()
        {
            var result = await _sut!.Get();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // 2. NEGATÍV TESZT: Lekérés olyan ID-val, ami nem létezik
        [TestMethod]
        public async Task GetById_InvalidId_ReturnsNotFound()
        {
            int nonExistingId = 999;

            var result = await _sut!.Get(nonExistingId);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // 3. POZITÍV TESZT: Új értékelés sikeres hozzáadása
        [TestMethod]
        public async Task Post_ValidData_ReturnsCreatedAtAction()
        {           
            var ujErtekeles = new Ertekeles
            {
                Szoveg = "Minden szuper volt!",
                Pont = 5
            };
            int tesztSzid = 1;
                        
            var result = await _sut!.Post(tesztSzid, ujErtekeles);
                        
            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
        }

        // 4. POZITÍV TESZT: Sikeres módosítás
        [TestMethod]
        public async Task Put_ExistingId_ReturnsOk()
        {
            var e = new Ertekeles { Szoveg = "Eredeti", Pont = 4 };
            _context.Ertekelesek.Add(e);
            await _context.SaveChangesAsync();

            var modositott = new Ertekeles { Szoveg = "Frissitve", Pont = 1 };

            var result = await _sut!.Put(e.Eid, modositott);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var val = (result as OkObjectResult)!.Value as Ertekeles;
            Assert.AreEqual("Frissitve", val!.Szoveg);
        }

        // 5. NEGATÍV TESZT: Módosítás nem létezõ ID-val
        [TestMethod]
        public async Task Put_NonExistingId_ReturnsNotFound()
        {
            var result = await _sut!.Put(999, new Ertekeles { Szoveg = "Hiba" });
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // 6. POZITÍV TESZT: Sikeres törlés
        [TestMethod]
        public async Task Delete_ExistingId_ReturnsOk()
        {
            var e = new Ertekeles { Szoveg = "Törlendõ", Pont = 1 };
            _context.Ertekelesek.Add(e);
            await _context.SaveChangesAsync();

            var result = await _sut!.Delete(e.Eid);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // 7. NEGATÍV TESZT: Törlés nem létezõ ID-val
        [TestMethod]
        public async Task Delete_NonExistingId_ReturnsNotFound()
        {
            var result = await _sut!.Delete(999);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Ertekeles_ChainTest()
        {
            // CREATE
            var uj = new Ertekeles { Szoveg = "Lánc teszt", Pont = 5, Datum = DateTime.Now };
            var postRes = await _sut!.Post(1, uj) as CreatedAtActionResult;
            Assert.IsNotNull(postRes);
            var mentett = postRes.Value as Ertekeles;
            Assert.IsNotNull(mentett);

            // UPDATE 
            mentett.Szoveg = "Módosított lánc";
            var putRes = await _sut.Put(mentett.Eid, mentett) as OkObjectResult;
            Assert.IsNotNull(putRes);

            // GET
            var getRes = await _sut.Get(mentett.Eid) as OkObjectResult;
            var lekerdezett = getRes!.Value as Ertekeles;
            Assert.AreEqual("Módosított lánc", lekerdezett!.Szoveg);

            //  DELETE 
            var delRes = await _sut.Delete(mentett.Eid) as OkObjectResult;
            Assert.IsNotNull(delRes);

            // VERIFY 
            var finalGet = await _sut.Get(mentett.Eid);
            Assert.IsInstanceOfType(finalGet, typeof(NotFoundResult));
        }
    }
}
