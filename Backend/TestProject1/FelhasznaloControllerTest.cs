using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Controllers;
using Projekt.Model;

namespace TestProject1
{
    [TestClass]
    public sealed class FelhasznaloControllerTest
    {
        private FelhasznaloController? _sut;
        private Context _context;

        [TestInitialize]
        public void Initialize()
        {
            _context = DbContextHelper.CreateDbContext();
            _sut = new FelhasznaloController(_context);
        }

        // Felhasználók listájának lekérése
        [TestMethod]
        public async Task Get_ReturnsOkResult_WithUsers()
        {
        
            var result = await _sut!.Get();

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Regisztráció már létezõ e-mail címmel
        [TestMethod]
        public async Task Post_DuplicateEmail_ReturnsBadRequest()
        {
          
            var ujFelhasznalo = new Felhasznalo
            {
                Email = "test@test.hu", 
                Jelszo = "Valami123",
                Nev = "Hiba Teszt"
            };

            var result = await _sut!.Post(ujFelhasznalo);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        //  Sikeres törlés
        [TestMethod]
        public async Task Delete_ExistingId_ReturnsOk()
        {
           
            int existingId = 1; 
            
            var result = await _sut!.Delete(existingId);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Regisztráció jelszó nélkül
        [TestMethod]
        public async Task Post_NoPassword_ReturnsBadRequest()
        {
            var hibaUser = new Felhasznalo { Email = "no@pass.hu", Jelszo = "" };
            var result = await _sut!.Post(hibaUser);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        // Lekérdezés nem létezõ ID-val
        [TestMethod]
        public async Task GetById_NonExisting_ReturnsNotFound()
        {
            var result = await _sut!.Get(999, false);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        // Put nem létezõ ID-val
        [TestMethod]
        public async Task Put_NonExisting_ReturnsNotFound()
        {
            var result = await _sut!.Put(999, new Felhasznalo { Nev = "Senki" });
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task Felhasznalo_ChainTest()
        {
            // CREATE 
            var ujUser = new Felhasznalo
            {
                Nev = "Lánc Elek",
                Email = "lanc@elek.hu",
                Jelszo = "Titok123",
                Lakcim = "Budapest"
            };

            var createRes = await _sut!.Post(ujUser) as CreatedAtActionResult;
            Assert.IsNotNull(createRes);
            var mentett = createRes.Value as Felhasznalo;
            Assert.IsNotNull(mentett);
            Assert.IsNull(mentett.Jelszo);

            // READ
            var getRes = await _sut.Get(mentett.Fid, true) as OkObjectResult;
            Assert.IsNotNull(getRes);
            var lekerdezett = getRes.Value as Felhasznalo;
            Assert.AreEqual("Lánc Elek", lekerdezett!.Nev);

            // UPDATE
            lekerdezett.Nev = "Módosított Elek";
            lekerdezett.Lakcim = "Debrecen";

            var putRes = await _sut.Put(lekerdezett.Fid, lekerdezett) as OkObjectResult;
            Assert.IsNotNull(putRes);
            var modositott = putRes.Value as Felhasznalo;
            Assert.AreEqual("Módosított Elek", modositott!.Nev);

            // DELETE
            var delRes = await _sut.Delete(modositott.Fid) as OkObjectResult;
            Assert.IsNotNull(delRes);

            // VERIFY
            var finalGet = await _sut.Get(modositott.Fid, false);
            Assert.IsInstanceOfType(finalGet, typeof(NotFoundResult));
        }
    }
}