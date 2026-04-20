using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Controllers;
using Projekt.Model;

namespace TestProject1
{
    [TestClass]
    public sealed class Szallas_SzolgaltatasControllerTest
    {
        private Szallas_SzolgaltatasController? _sut;
        private Context _context;

        [TestInitialize]
        public void Initialize()
        {
            _context = DbContextHelper.CreateDbContext();
            _sut = new Szallas_SzolgaltatasController(_context);
        }

        // Kapcsolat lekérése létezõ páros alapján
        [TestMethod]
        public async Task GetByIds_ExistingPair_ReturnsOk()
        {
       
            int szid = 1;
            int szoid = 1;

            var result = await _sut!.Get(szid, szoid);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Lekérés olyan párossal, ami nem létezik
        [TestMethod]
        public async Task GetByIds_NonExistingPair_ReturnsNotFound()
        {
      
            int szid = 999;
            int szoid = 888;

            var result = await _sut!.Get(szid, szoid);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        //  Új szolgáltatás-szállás összerendelés törlése
        [TestMethod]
        public async Task Delete_ExistingPair_ReturnsOk()
        {
           
            int szid = 1;
            int szoid = 1;

            var result = await _sut!.Delete(szid, szoid);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var maradvany = await _context.Szallas_Szolgaltatas
                .AnyAsync(x => x.Szid == szid && x.Szoid == szoid);
            Assert.IsFalse(maradvany);
        }

        // Listázásnál az Include-olt objektumok meglétének ellenõrzése
        [TestMethod]
        public async Task Get_ReturnsList_WithIncludedObjects()
        {
            var result = await _sut!.Get() as OkObjectResult;
            var lista = result!.Value as List<Szallas_szolgaltatas>;

            Assert.IsNotNull(lista);
            Assert.IsTrue(lista.Count > 0);

            var elso = lista[0];
            Assert.IsNotNull(elso.Szallas, "A Szallas objektum nem lett betöltve (Include hiba)!");
        }

        // Törlés nem létezõ párosra
        [TestMethod]
        public async Task Delete_NonExisting_ReturnsNotFound()
        {
            var result = await _sut!.Delete(999, 999);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task SzallasSzolgaltatas_ChainTest()
        {
            // CREATE
            var ujKapcsolat = new Szallas_szolgaltatas { Szid = 1, Szoid = 2 };

            var postRes = await _sut!.Post(ujKapcsolat) as ObjectResult;
            Assert.IsNotNull(postRes);
            Assert.AreEqual(201, postRes.StatusCode);

            // READ 
            var getRes = await _sut.Get(1, 2) as OkObjectResult;
            Assert.IsNotNull(getRes);
            var lekerdezett = getRes.Value as Szallas_szolgaltatas;
            Assert.AreEqual(1, lekerdezett!.Szid);
            Assert.AreEqual(2, lekerdezett.Szoid);

            // DELETE
            var delRes = await _sut.Delete(1, 2) as OkObjectResult;
            Assert.IsNotNull(delRes);

            // VERIFY
            var finalGet = await _sut.Get(1, 2);
            Assert.IsInstanceOfType(finalGet, typeof(NotFoundResult));
        }
    }
}