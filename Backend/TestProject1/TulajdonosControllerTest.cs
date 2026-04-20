using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Controllers;
using Projekt.Model;
using Microsoft.EntityFrameworkCore;

namespace TestProject1
{
    [TestClass]
    public sealed class TulajdonosControllerTest
    {
        private TulajdonosController? _sut;
        private Context _context;

        [TestInitialize]
        public void Initialize()
        {
            _context = DbContextHelper.CreateDbContext();
            _sut = new TulajdonosController(_context);
        }

        // Tulajdonos lekérése kiterjesztett adatokkal 
        [TestMethod]
        public async Task GetById_WithExtension_ReturnsOk()
        {           
            
            int id = 1;

            var result = await _sut!.Get(id, true);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Új tulajdonos mentése név nélkül
        [TestMethod]
        public async Task Post_MissingName_ReturnsBadRequest()
        {           
            var hibasTulaj = new Tulajdonos { Nev = "" }; 

            var result = await _sut!.Post(0, hibasTulaj);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // Tulajdonos sikeres törlése
        [TestMethod]
        public async Task Delete_ExistingTulajdonos_ReturnsOk()
        {

            int id = 1;

            var result = await _sut!.Delete(id);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var kereses = await _context.Tulajdonosok.FindAsync(id);
            Assert.IsNull(kereses);
        }

        // Összes tulajdonos listázása
        [TestMethod]
        public async Task Get_AllOwners_ReturnsOk()
        {
            var result = await _sut!.Get();
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Lekérés nem létezõ ID-val
        [TestMethod]
        public async Task GetById_NonExisting_ReturnsNotFound()
        {
            var result = await _sut!.Get(9999, false);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Módosítás nem létezõ ID-val
        [TestMethod]
        public async Task Put_NonExisting_ReturnsNotFound()
        {
            var hibaTulaj = new Tulajdonos { Nev = "Senki" };
            var result = await _sut!.Put(9999, hibaTulaj);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Tulajdonos_ChainTest()
        {
            // CREATE
            var ujTulaj = new Tulajdonos { Nev = "Lánc Lajos", Email = "lajos@teszt.hu" };

            // POST (szid, tulajdonos)
            var createRes = await _sut!.Post(1, ujTulaj) as CreatedAtActionResult;
            Assert.IsNotNull(createRes);
            var mentett = createRes.Value as Tulajdonos;
            Assert.IsNotNull(mentett);

            // READ
            var getRes = await _sut.Get(mentett.Tid, true) as OkObjectResult;
            Assert.IsNotNull(getRes);
            var lekerdezett = getRes.Value as Tulajdonos;
            Assert.AreEqual("Lánc Lajos", lekerdezett!.Nev);

            // UPDATE 
            lekerdezett.Email = "uj_lajos@teszt.hu";
            var putRes = await _sut.Put(lekerdezett.Tid, lekerdezett) as OkObjectResult;
            Assert.IsNotNull(putRes);

            // DELETE 
            var delRes = await _sut.Delete(lekerdezett.Tid) as OkObjectResult;
            Assert.IsNotNull(delRes);

            // VERIFY 
            var finalGet = await _sut.Get(lekerdezett.Tid, false);
            Assert.IsInstanceOfType(finalGet, typeof(NotFoundResult));
        }
    }
}