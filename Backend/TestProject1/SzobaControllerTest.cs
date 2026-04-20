using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Controllers;
using Projekt.Model;
using Microsoft.EntityFrameworkCore;

namespace TestProject1
{
    [TestClass]
    public sealed class SzobaControllerTest
    {
        private SzobaController? _sut;
        private Context _context;

        [TestInitialize]
        public void Initialize()
        {
            _context = DbContextHelper.CreateDbContext();
            _sut = new SzobaController(_context);
        }

        // Egy konkrét szoba lekérése ID alapján
        [TestMethod]
        public async Task GetById_ExistingRoom_ReturnsOk()
        {
          
            int id = 1;

            var result = await _sut!.Get(id);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Törlési kísérlet ismeretlen szoba ID-val
        [TestMethod]
        public async Task Delete_UnknownRoom_ReturnsNotFound()
        {
            
            int unknownId = 5555;

            var result = await _sut!.Delete(unknownId);

            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Szoba adatainak frissítése 
        [TestMethod]
        public async Task Put_UpdateRoomDetails_ReturnsOk()
        {
            
            int targetId = 1;
            var frissAdatok = new Szoba
            {
                Statusz = "Foglalt",
                Leiras = "Frissített leírás",
                Db = 2
            };

            var result = await _sut!.Put(targetId, frissAdatok);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
                    
            var szoba = await _context.Szobak.FindAsync(targetId);
            Assert.AreEqual("Frissített leírás", szoba?.Leiras);
        }

        // Listázás (GetAll)
        [TestMethod]
        public async Task Get_AllRooms_ReturnsOk()
        {
            var result = await _sut!.Get();
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Lekérés nem létezõ ID-val
        [TestMethod]
        public async Task GetById_NonExisting_ReturnsNotFound()
        {
            var result = await _sut!.Get(9999);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Post olyan szálláshoz, ami nem létezik 
        [TestMethod]
        public async Task Post_WithInvalidSzid_StillCreatesRoom()
        {
            var szoba = new Szoba { Leiras = "Magányos szoba" };
            var result = await _sut!.Post(999, szoba) as CreatedAtActionResult;

            Assert.IsNotNull(result);
            var mentett = result.Value as Szoba;
            Assert.IsNotNull(mentett);
            Assert.IsTrue(mentett.Sid > 0);
        }

        [TestMethod]
        public async Task Szoba_ChainTest()
        {
            // CREATE
            var ujSzoba = new Szoba
            {
                Leiras = "Lánc teszt szoba",
                Statusz = "Szabad",
                Db = 1
            };

            var createRes = await _sut!.Post(1, ujSzoba) as CreatedAtActionResult;
            Assert.IsNotNull(createRes);
            var mentett = createRes.Value as Szoba;
            Assert.IsNotNull(mentett);

            var szallas = await _context.Szallasok.FindAsync(1);
            Assert.AreEqual(mentett.Sid, szallas?.Sid, "A szállás nem kapta meg az új szoba ID-ját.");

            // UPDATE
            mentett.Statusz = "Karbantartás alatt";
            var putRes = await _sut.Put(mentett.Sid, mentett) as OkObjectResult;
            Assert.IsNotNull(putRes);
            var modositott = putRes.Value as Szoba;
            Assert.AreEqual("Karbantartás alatt", modositott!.Statusz);

            // DELETE 
            var delRes = await _sut.Delete(mentett.Sid) as OkObjectResult;
            Assert.IsNotNull(delRes);

            // VERIFY 
            var finalGet = await _sut.Get(mentett.Sid);
            Assert.IsInstanceOfType(finalGet, typeof(NotFoundResult));
        }
    }
}