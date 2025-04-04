// File: AccountIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests
{
    public class AccountIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AccountIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateUserAsync(string email, string firstName, string lastName, bool isAdmin, string password)
        {
            var request = new
            {
                email = email,
                firstName = firstName,
                lastName = lastName,
                isAdmin = isAdmin,
                password = password
            };

            return await _client.PostAsJsonAsync("/api/accounts", request);
        }

        private async Task<HttpResponseMessage> AuthenticateUserAsync(string email, string password)
        {
            var request = new
            {
                email = email,
                password = password
            };

            return await _client.PostAsJsonAsync("/api/accounts/tokens", request);
        }

        private async Task<string> CreateUserAndGetTokenAsync(string email, string firstName, string lastName, bool isAdmin, string password)
        {
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);
            await response.Content.ReadFromJsonAsync<JsonObject>();
            response = await AuthenticateUserAsync(email, password);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            return token;
        }

        private async Task<JsonObject> CreateUserAndGetResponseAsync(string email, string firstName, string lastName, bool isAdmin, string password)
        {
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);
            await response.Content.ReadFromJsonAsync<JsonObject>();
            response = await AuthenticateUserAsync(email, password);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body;
        }

        [Fact]
        public async Task TC001_Create_Account_When_Valid_Data_Returns_OK()
        {
            // arrange
            string email = "valid@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_Account_When_Email_Already_Exists_Returns_BadRequest()
        {
            // arrange
            string email = "duplicado@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Account_When_Invalid_Email_Format_Returns_BadRequest()
        {
            // arrange
            string email = "invalido!@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Account_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = null;
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Account_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Account_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Account_When_Password_Is_Null_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = null;

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Account_When_Password_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Account_When_Password_Too_Short_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Account_When_Password_Too_Long_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123456789101112!";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Account_When_Password_Missing_Uppercase_Letter_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "senha123!";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_Account_When_Password_Missing_Lowercase_Letter_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "SENHA123!";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_Account_When_Password_Missing_Digit_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha!Az";

            // act
            var response = await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Authenticate_Account_When_Valid_Data_Returns_OK()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await AuthenticateUserAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Authenticate_Account_When_Invalid_Password_Returns_Unauthorized()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await AuthenticateUserAsync(email, "Senha123");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Authenticate_Account_When_Invalid_Email_Returns_Unauthorized()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            await CreateUserAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await AuthenticateUserAsync("invalido@email.com", password);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Refresh_Token_When_Valid_Data_Returns_OK()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            var body = await CreateUserAndGetResponseAsync(email, firstName, lastName, isAdmin, password);
            var token = body["token"].AsValue().GetValue<string>();
            var refreshToken = body["refreshToken"].AsValue().GetValue<string>();
            var userId = body["userId"].AsValue().GetValue<string>();

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/refreshtokens", 
                new { 
                    token = token,
                    refreshToken = refreshToken,
                    userId = userId
                }
            );

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
