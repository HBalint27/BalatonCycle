using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Controllers;
using Projekt.Model;

namespace TestProject1
{
    [TestClass]
    public sealed class FoglalControllerTest
    {
        private FoglalController? _sut;
        private Context _context;

        [TestInitialize]
        public void Initialize()
        {
            _context = DbContextHelper.CreateDbContext();
            _sut = new FoglalController(_context);
        }

        // Összes foglalás lekérése 
        [TestMethod]
        public async Task Get_ReturnsOkResult_WithBookings()
        {
     
            var result = await _sut!.Get();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Egy konkrét foglalás lekérése összetett kulccsal
        [TestMethod]
        public async Task GetByCompositeKey_ExistingBooking_ReturnsOk()
        {
 
            int fid = 1;
            int szid = 1;
            DateTime erkezes = new DateTime(2026, 05, 10);

            var result = await _sut!.Get(fid, szid, erkezes);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Törlési kísérlet nem létezõ adatokkal
        [TestMethod]
        public async Task Delete_NonExistingBooking_ReturnsNotFound()
        {
           
            int fid = 999;
            int szid = 999;
            DateTime erkezes = DateTime.Now;

            var result = await _sut!.Delete(fid, szid, erkezes);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Listázásnál ellenõrizzük, hogy a Helper adatai benne vannak-e
        [TestMethod]
        public async Task Get_ReturnsList_CheckCount()
        {
            var result = await _sut!.Get() as OkObjectResult;
            var lista = result!.Value as List<Foglal>;

            Assert.IsNotNull(lista);
            Assert.IsTrue(lista.Count >= 1);
        }

        // Egyedi lekérés nem létezõ dátummal
        [TestMethod]
        public async Task Get_InvalidDate_ReturnsNotFound()
        {            
            var result = await _sut!.Get(1, 1, new DateTime(1990, 01, 01));
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Foglal_ChainTest()
        {
            // CREATE 
            var ujFoglal = new Foglal
            {
                Fid = 1,
                Szid = 1,
                ErkezesNap = new DateTime(2025, 12, 24)
            };

            var postRes = await _sut!.Post(ujFoglal) as ObjectResult;
            Assert.IsNotNull(postRes);
            Assert.AreEqual(201, postRes.StatusCode);

            // READ 
            var getRes = await _sut.Get(1, 1, new DateTime(2025, 12, 24)) as OkObjectResult;
            Assert.IsNotNull(getRes);
            var lekerdezett = getRes.Value as Foglal;
            Assert.AreEqual(new DateTime(2025, 12, 24), lekerdezett!.ErkezesNap);

            // DELETE
            var delRes = await _sut.Delete(1, 1, new DateTime(2025, 12, 24)) as OkObjectResult;
            Assert.IsNotNull(delRes);

            // VERIFY 
            var finalGet = await _sut.Get(1, 1, new DateTime(2025, 12, 24));
            Assert.IsInstanceOfType(finalGet, typeof(NotFoundResult));
        }
    }
}