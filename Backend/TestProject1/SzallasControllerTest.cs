using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Controllers;
using Projekt.Model;

namespace TestProject1
{
    [TestClass]
    public sealed class SzallasControllerTest
    {
        private SzallasController? _sut;
        private Context _context;

        [TestInitialize]
        public void Initialize()
        {
            _context = DbContextHelper.CreateDbContext();
            _sut = new SzallasController(_context);
        }

        // Szállás lekérése kiterjesztett adatok nélkül 
        [TestMethod]
        public async Task GetById_BasicData_ReturnsOk()
        {
           
            int id = 1;

            var result = await _sut!.Get(id, false);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Új szállás létrehozása
        [TestMethod]
        public async Task Post_ValidSzallas_ReturnsCreatedAtAction()
        {
           
            var ujSzallas = new Szallas
            {
                SzallasCime = "1051 Budapest, Teszt utca 1.",
                Ar = 25000,
                Tid = 1
            };

            var result = await _sut!.Post(ujSzallas);

            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));
        }

        // Módosítás nem létezõ ID-val
        [TestMethod]
        public async Task Put_NonExistingId_ReturnsNotFound()
        {
           
            int nonExistingId = 999;
            var modositott = new Szallas { SzallasCime = "Nem létezõ", Ar = 0 };

            var result = await _sut!.Put(nonExistingId, modositott);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Listázás (GetAll) ellenõrzése
        [TestMethod]
        public async Task Get_All_ReturnsList()
        {
            var result = await _sut!.Get() as OkObjectResult;
            var lista = result!.Value as List<Szallas>;

            Assert.IsNotNull(lista);
            Assert.IsTrue(lista.Count > 0); // A Helper alapból tesz bele szállást
        }

        // Törlés nem létezõ ID-val
        [TestMethod]
        public async Task Delete_NonExisting_ReturnsNotFound()
        {
            var result = await _sut!.Delete(9999);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Lekérés kiterjesztett adatokkal létezõ ID-ra
        [TestMethod]
        public async Task GetById_ExtendedData_ReturnsOk()
        {
            int existingId = 1;
            var result = await _sut!.Get(existingId, true);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Szallas_ChainTest()
        {
            // CREATE 
            var ujSzallas = new Szallas
            {
                SzallasCime = "Debrecen, Teszt u. 5",
                Ar = 20000,
                Tid = 1 
            };

            var createRes = await _sut!.Post(ujSzallas) as CreatedAtActionResult;
            Assert.IsNotNull(createRes);
            var mentett = createRes.Value as Szallas;
            Assert.IsNotNull(mentett);

            // READ
            var getRes = await _sut.Get(mentett.Szid, true) as OkObjectResult;
            Assert.IsNotNull(getRes);
            var lekerdezett = getRes.Value as Szallas;
            Assert.AreEqual("Debrecen, Teszt u. 5", lekerdezett!.SzallasCime);
            Assert.IsNotNull(lekerdezett.Tulajdonos);

            // UPDATE 
            lekerdezett.Ar = 30000;
            lekerdezett.SzallasCime = "Budapest, Hõsök tere 1";

            var putRes = await _sut.Put(lekerdezett.Szid, lekerdezett) as OkObjectResult;
            Assert.IsNotNull(putRes);
            var modositott = putRes.Value as Szallas;
            Assert.AreEqual(30000, modositott!.Ar);

            // DELETE 
            var delRes = await _sut.Delete(modositott.Szid) as OkObjectResult;
            Assert.IsNotNull(delRes);

            // VERIFY 
            var finalGet = await _sut.Get(modositott.Szid, false);
            Assert.IsInstanceOfType(finalGet, typeof(NotFoundResult));
        }
    }
}