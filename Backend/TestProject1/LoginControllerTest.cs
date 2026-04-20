using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Projekt.Controllers;
using Projekt.Auth;
using Projekt.Model;
using Microsoft.Extensions.Configuration;

namespace TestProject1
{
    [TestClass]
    public sealed class LoginControllerTest
    {
        private LoginController? _sut;
        private Context _context;
        private TokenManager _tokenManager;

        [TestInitialize]
        public void Initialize()
        {
            _context = DbContextHelper.CreateDbContext();

            // A TokenManager titkosítás
            var inMemorySettings = new Dictionary<string, string> {
                {"JWT:Key", "AzÉnNagyonTitkosKulcsom12345678 ;-]"}, 
                {"JWT:Issuer", "HBSZ"},
                {"JWT:Audience", "13A"}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _tokenManager = new TokenManager(configuration);
            _sut = new LoginController(_context, _tokenManager);
        }

        //  Sikeres bejelentkezés helyes adatokkal
        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
          
            string email = "test@test.hu";
            string password = "helyes_jelszo";

            var result = await _sut!.Login(email, password);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        // Hibás jelszó elutasítása
        [TestMethod]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            
            string email = "test@test.hu";
            string wrongPassword = "rossz_jelszo";

            var result = await _sut!.Login(email, wrongPassword);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        // Nem létezõ felhasználó elutasítása
        [TestMethod]
        public async Task Login_NonExistingUser_ReturnsUnauthorized()
        {

            string nonExistingEmail = "nincsilyen@felhasznalo.hu";
            string password = "barmilyen_jelszo";

            var result = await _sut!.Login(nonExistingEmail, password);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        // Teszteljük, hogy üres mezõkkel is Unauthorized-ot ad
        [TestMethod]
        public async Task Login_EmptyFields_ReturnsUnauthorized()
        {
            var result = await _sut!.Login("", "");
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        // Ellenõrizzük, hogy a kapott objektum típusa string
        [TestMethod]
        public async Task Login_ReturnValue_IsStringToken()
        {
            var result = await _sut!.Login("test@test.hu", "helyes_jelszo") as OkObjectResult;
            Assert.IsInstanceOfType(result!.Value, typeof(string));
        }

        [TestMethod]
        public async Task Login_ChainTest()
        {
            // Próbálkozás rossz jelszóval
            var wrongPassResult = await _sut!.Login("test@test.hu", "rossz_jelszo");
            Assert.IsInstanceOfType(wrongPassResult, typeof(UnauthorizedResult));

            // Próbálkozás nem létezõ emaillel
            var wrongEmailResult = await _sut.Login("nemletezik@teszt.hu", "helyes_jelszo");
            Assert.IsInstanceOfType(wrongEmailResult, typeof(UnauthorizedResult));

            // Sikeres belépés
            var successResult = await _sut.Login("test@test.hu", "helyes_jelszo") as OkObjectResult;
            Assert.IsNotNull(successResult);

            // Token ellenõrzése
            var token = successResult.Value as string;
            Assert.IsFalse(string.IsNullOrEmpty(token), "A token nem lett legenerálva.");
        }
    }
}