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

        private async Task<HttpResponseMessage> CreateAccountAsync(string firstName, string lastName, bool isAdmin, string email, string password)
        {
            var request = new
            {
                firstName = firstName,
                lastName = lastName,
                isAdmin = isAdmin,
                email = email,
                password = password
            };

            return await _client.PostAsJsonAsync("/api/accounts", request);
        }

        private async Task<HttpResponseMessage> LoginAsync(string email, string password)
        {
            var request = new
            {
                email = email,
                password = password
            };

            return await _client.PostAsJsonAsync("/api/accounts/tokens", request);
        }

        private async Task<string> GetTokenForUserAsync(string firstName, string lastName, bool isAdmin, string email, string password)
        {
            await CreateAccountAsync(firstName, lastName, isAdmin, email, password);
            var response = await LoginAsync(email, password);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].AsValue().GetValue<string>();
        }

        private async Task<HttpResponseMessage> GetCountriesAsync(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.GetAsync("/api/countries");
        }

        private async Task<HttpResponseMessage> CreateCountryAsync(string token, string name, string shortName)
        {
            var request = new
            {
                name = name,
                shortName = shortName
            };

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.PostAsJsonAsync("/api/countries", request);
        }

        private async Task<HttpResponseMessage> GetCountryByIdAsync(string token, int id)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.GetAsync($"/api/countries/{id}");
        }

        private async Task<HttpResponseMessage> UpdateCountryAsync(string token, int id, string name, string shortName)
        {
            var request = new
            {
                name = name,
                shortName = shortName
            };

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.PutAsJsonAsync($"/api/countries/{id}", request);
        }

        private async Task<HttpResponseMessage> DeleteCountryAsync(string token, int id)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.DeleteAsync($"/api/countries/{id}");
        }

        [Fact]
        public async Task TC011_Get_Countries_When_No_Authorization_Returns_Unauthorized()
        {
            // arrange
            // no token provided

            // act
            var response = await GetCountriesAsync(null);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Get_Countries_When_Valid_Authorization_Returns_OK()
        {
            // arrange
            var user = "user12";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // act
            var response = await GetCountriesAsync(token);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_Country_When_Valid_Data_Returns_Created()
        {
            // arrange
            var user = "user13";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // act
            var response = await CreateCountryAsync(token, "Brazil", "BR");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_shortName = body["shortName"].AsValue().GetValue<string>();
            var body_id = body["id"].AsValue().GetValue<int>();
            Assert.Equal("Brazil", body_name);
            Assert.Equal("BR", body_shortName);
            Assert.True(body_id > 0);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_Country_When_Invalid_Name_Returns_BadRequest()
        {
            // arrange
            var user = "user14";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // act
            var response = await CreateCountryAsync(token, "", "BR");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Get_Country_When_Valid_ID_Returns_OK()
        {
            // arrange
            var user = "user15";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            var createCountryResponse = await CreateCountryAsync(token, "Brazil", "BR");
            var createCountryBody = await createCountryResponse.Content.ReadFromJsonAsync<JsonObject>();
            var countryId = createCountryBody["id"].AsValue().GetValue<int>();

            // act
            var response = await GetCountryByIdAsync(token, countryId);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            Assert.Equal(countryId, body_id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Get_Country_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            var user = "user16";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // act
            var response = await GetCountryByIdAsync(token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Update_Country_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            var user = "user17";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            var createCountryResponse = await CreateCountryAsync(token, "Brazil", "BR");
            var createCountryBody = await createCountryResponse.Content.ReadFromJsonAsync<JsonObject>();
            var countryId = createCountryBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateCountryAsync(token, countryId, "Brazil (Updated)", "BR");

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Update_Country_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            var user = "user18";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // act
            var response = await UpdateCountryAsync(token, 9999999, "Brazil (Updated)", "BR");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Delete_Country_When_Valid_ID_Returns_NoContent()
        {
            // arrange
            var user = "user19";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: true, // Assuming admin can delete
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            var createCountryResponse = await CreateCountryAsync(token, "Brazil", "BR");
            var createCountryBody = await createCountryResponse.Content.ReadFromJsonAsync<JsonObject>();
            var countryId = createCountryBody["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteCountryAsync(token, countryId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Delete_Country_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            var user = "user20";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: true, // Assuming admin can delete
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // act
            var response = await DeleteCountryAsync(token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}