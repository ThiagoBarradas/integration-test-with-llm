// File: CountriesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;

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

        private async Task<HttpResponseMessage> CreateCountryAsync(string token, string name, string shortName)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(new {
                    name = name,
                    shortName = shortName
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            return response;
        }

        private async Task<HttpResponseMessage> CreateUserAsync(string firstName, string lastName, string email, string password, bool isAdmin)
        {
            var request = new
            {
                firstName = firstName,
                lastName = lastName,
                email = email,
                password = password,
                isAdmin = isAdmin
            };

            return await _client.PostAsJsonAsync("/api/accounts", request);
        }

        private async Task<string> AuthenticateAsync()
        {
            var authenticateRequest = new
            {
                email = $"admin.user.{Guid.NewGuid()}@example.com",
                password = "V@lidP4ssw0rd"
            };
            await CreateUserAsync("Some", "User", authenticateRequest.email, authenticateRequest.password, true);
            var authenticateResponse = await _client.PostAsJsonAsync("/api/accounts/tokens", authenticateRequest);
            var content = await authenticateResponse.Content.ReadFromJsonAsync<JsonDocument>();
            return content!.RootElement.GetProperty("token").ToString();
        }

        private async Task<HttpResponseMessage> GetCountryByIdAsync(int id)
        {
            var response = await _client.GetAsync($"/api/countries/{id}");
            return response;
        }

        private async Task<HttpResponseMessage> UpdateCountryAsync(string token, int id, string name, string shortName)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/countries/{id}")
            {
                Content = JsonContent.Create(new
                {
                    name = name,
                    shortName = shortName
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            return response;
        }

        private async Task<HttpResponseMessage> DeleteCountryAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/countries/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            return response;
        }

        [Fact]
        public async Task TC010_Create_Country_With_Valid_Data_Returns_Created()
        {
            // arrange
            var token = await AuthenticateAsync();

            // act
            var response = await CreateCountryAsync(token, "Test Country", "TC");

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<JsonDocument>();
            Assert.NotNull(content);
            Assert.Equal("Test Country", content.RootElement.GetProperty("name").ToString());
        }

        [Fact]
        public async Task TC011_Create_Country_With_Empty_Name_Returns_BadRequest()
        {
            // arrange
            var token = await AuthenticateAsync();

            // act
            var response = await CreateCountryAsync(token, "", "TC");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_G1_Get_Country_By_Id_With_Valid_Id_Returns_OK()
        {
            // arrange
            var token = await AuthenticateAsync();
            var createResponse = await CreateCountryAsync(token, "Test Country", "TC");
            var content = await createResponse.Content.ReadFromJsonAsync<JsonDocument>();
            int countryId = content!.RootElement.GetProperty("id").GetInt32();

            // act
            var getResponse = await GetCountryByIdAsync(countryId);

            // assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getCountryContent = await getResponse.Content.ReadFromJsonAsync<JsonDocument>();
            Assert.NotNull(getCountryContent);
            Assert.Equal("Test Country", getCountryContent.RootElement.GetProperty("name").ToString());
        }

        [Fact]
        public async Task TC013_Get_Country_By_Id_With_Invalid_Id_Returns_NotFound()
        {
            // act
            var response = await GetCountryByIdAsync(999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Update_Country_With_Valid_Data_Returns_NoContent()
        {
            // arrange
            var token = await AuthenticateAsync();
            var createResponse = await CreateCountryAsync(token, "Test Country", "TC");
            var content = await createResponse.Content.ReadFromJsonAsync<JsonDocument>();
            int countryId = content!.RootElement.GetProperty("id").GetInt32();

            // act
            var updateResponse = await UpdateCountryAsync(token, countryId, "Updated Country", "UC");

            // assert
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        }

        [Fact]
        public async Task TC015_Update_Country_With_Invalid_Id_Returns_NotFound()
        {
            // arrange
            var token = await AuthenticateAsync();

            // act
            var response = await UpdateCountryAsync(token, 999999, "Updated Country", "UC");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Delete_Country_With_Valid_Id_Returns_NoContent()
        {
            // arrange
            var token = await AuthenticateAsync();
            var createResponse = await CreateCountryAsync(token, "Test Country", "TC");
            var content = await createResponse.Content.ReadFromJsonAsync<JsonDocument>();
            int countryId = content!.RootElement.GetProperty("id").GetInt32();

            // act
            var deleteResponse = await DeleteCountryAsync(token, countryId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task TC017_Delete_Country_With_Invalid_Id_Returns_NotFound()
        {
            // arrange
            var token = await AuthenticateAsync();

            // act
            var response = await DeleteCountryAsync(token, 999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}