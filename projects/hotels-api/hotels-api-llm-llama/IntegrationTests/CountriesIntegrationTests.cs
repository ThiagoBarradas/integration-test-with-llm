// File: CountriesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace IntegrationTests
{
    public class CountriesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CountriesIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> CreateUserAndGetTokenAsync(string email, string firstName, string lastName, bool isAdmin, string password)
        {
            var request = new
            {
                email = email,
                firstName = firstName,
                lastName = lastName,
                isAdmin = isAdmin,
                password = password
            };

            await _client.PostAsJsonAsync("/api/accounts", request);

            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", new
            {
                email = email,
                password = password
            });

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            return token;
        }

        private async Task<HttpResponseMessage> CreateCountryAsync(string token, string name, string shortName)
        {
            var request = new
            {
                name = name,
                shortName = shortName
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(request)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<int> CreateCountryAndGetIdAsync(string token, string name, string shortName)
        {
            var response = await CreateCountryAsync(token, name, shortName);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> UpdateCountryAsync(string token, int id, string name, string shortName)
        {
            var request = new
            {
                name = name,
                shortName = shortName
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/countries/{id}")
            {
                Content = JsonContent.Create(request)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> GetCountryAsync(int id)
        {
            return await _client.GetAsync($"/api/countries/{id}");
        }

        private async Task<HttpResponseMessage> DeleteCountryAsync(string token, int id)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/countries/{id}")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", token)
                }
            };

            return await _client.SendAsync(requestMessage);
        }

        [Fact]
        public async Task TC019_Get_All_Countries_When_Valid_Data_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/api/countries");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Create_Country_When_Valid_Data_Returns_Created()
        {
            // arrange
            string email = "valido.user@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = false;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "PT";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Create_Country_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string email = "valido.admin@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = null;
            string shortName = "PT";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Create_Country_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "";
            string shortName = "PT";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Create_Country_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "";
            string shortName = "PT";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Create_Country_When_Short_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = null;
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Country_When_Short_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Create_Country_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "PT";
            var token = "InvalidToken";

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Create_Country_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "PT";

            // act
            var response = await CreateCountryAsync(null, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Get_Country_By_ID_When_Valid_Data_Returns_OK()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "PT";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);
            var id = await CreateCountryAndGetIdAsync(token, name, shortName);

            // act
            var response = await GetCountryAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Get_Country_By_ID_When_ID_Not_Exists_Returns_NotFound()
        {
            // act
            var response = await GetCountryAsync(999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Update_Country_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "PT";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);
            var id = await CreateCountryAndGetIdAsync(token, name, shortName);
            string newName = "Pa�s Atualizado";

            // act
            var response = await UpdateCountryAsync(token, id, newName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Update_Country_When_ID_Not_Exists_Returns_NotFound()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "PT";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);
            var id = 999;
            string newName = "Pa�s Atualizado";

            // act
            var response = await UpdateCountryAsync(token, id, newName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Update_Country_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "PT";
            var id = 1;
            string newName = "Pa�s Atualizado";
            var token = "InvalidToken";

            // act
            var response = await UpdateCountryAsync(token, id, newName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Update_Country_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "PT";
            var id = 1;
            string newName = "Pa�s Atualizado";

            // act
            var response = await UpdateCountryAsync(null, id, newName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Delete_Country_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Pa�s";
            string shortName = "PT";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);
            var id = await CreateCountryAndGetIdAsync(token, name, shortName);

            // act
            var response = await DeleteCountryAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Delete_Country_When_ID_Not_Exists_Returns_NotFound()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);
            var id = 999;

            // act
            var response = await DeleteCountryAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Delete_Country_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            var id = 1;
            var token = "InvalidToken";

            // act
            var response = await DeleteCountryAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}