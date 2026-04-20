using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Controllers;
using Projekt.Model;
using Microsoft.EntityFrameworkCore;

namespace TestProject1
{
    [TestClass]
    public sealed class SzolgaltatasokControllerTest
    {
        private SzolgaltatasokController? _sut;
        private Context _context;

        [TestInitialize]
        public void Initialize()
        {
            _context = DbContextHelper.CreateDbContext();
            _sut = new SzolgaltatasokController(_context);
        }

        // Szolgáltatás lekérése létezõ ID alapján
        [TestMethod]
        public async Task GetById_ExistingService_ReturnsOk()
        {
         
            int id = 1;

            var result = await _sut!.Get(id);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        //Új szolgáltatás sikeres mentése
        [TestMethod]
        public async Task Post_ValidService_ReturnsCreatedAtAction()
        {
            
            var ujSzolgaltatas = new Szolgaltatasok
            {
                Nev = "Reggeli",
                Szoid = 1
            };

            var result = await _sut!.Post(ujSzolgaltatas);

            Assert.IsInstanceOfType(result, typeof(CreatedAtActionResult));

            var mentett = await _context.Szolgaltatasok.FirstOrDefaultAsync(s => s.Nev == "Reggeli");
            Assert.IsNotNull(mentett);
        }

        //  Törlési kísérlet nem létezõ ID-val
        [TestMethod]
        public async Task Delete_NonExistingId_ReturnsNotFound()
        {
            
            int invalidId = 8888;

            var result = await _sut!.Delete(invalidId);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Összes szolgáltatás lekérése
        [TestMethod]
        public async Task Get_AllServices_ReturnsOk()
        {
            var result = await _sut!.Get();
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = result as OkObjectResult;
            var lista = okResult!.Value as List<Szolgaltatasok>;
            Assert.IsNotNull(lista);
            Assert.IsTrue(lista.Count > 0);
        }

        // Módosítás nem létezõ ID-val
        [TestMethod]
        public async Task Put_NonExistingId_ReturnsNotFound()
        {
            var hibaSz = new Szolgaltatasok { Nev = "Hiba" };
            var result = await _sut!.Put(9999, hibaSz);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Szolgaltatasok_ChainTest()
        {
            // CREATE 
            var ujSz = new Szolgaltatasok { Nev = "Szauna", Szoid = 1 };
            var postRes = await _sut!.Post(ujSz) as CreatedAtActionResult;
            Assert.IsNotNull(postRes);
            var mentett = postRes.Value as Szolgaltatasok;
            Assert.IsNotNull(mentett);
            Assert.IsTrue(mentett.Szoid > 0);

            // UPDATE 
            mentett.Nev = "Finn Szauna";
            var putRes = await _sut.Put(mentett.Szoid, mentett) as OkObjectResult;
            Assert.IsNotNull(putRes);

            // READ 
            var getRes = await _sut.Get(mentett.Szoid) as OkObjectResult;
            var lekerdezett = getRes!.Value as Szolgaltatasok;
            Assert.AreEqual("Finn Szauna", lekerdezett!.Nev);

            // DELETE 
            var delRes = await _sut.Delete(mentett.Szoid) as OkObjectResult;
            Assert.IsNotNull(delRes);

            // VERIFY 
            var finalGet = await _sut.Get(mentett.Szoid);
            Assert.IsInstanceOfType(finalGet, typeof(NotFoundResult));
        }
    }
}