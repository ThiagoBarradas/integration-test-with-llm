// File: HotelsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class HotelsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public HotelsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private string GenerateRandomString() => $"Hotel_{Guid.NewGuid()}";

        private async Task<string> CreateAuthenticatedUserAndGetTokenAsync()
        {
            string email = $"user{Guid.NewGuid()}@example.com";
            string password = "Password1!";

            // Create user
            var createUserRequest = new
            {
                firstName = "John",
                lastName = "Doe",
                email = email,
                isAdmin = true, // Hotels may require admin access
                password = password
            };
            await _client.PostAsJsonAsync("/api/accounts", createUserRequest);

            // Authenticate and get token
            var authenticateRequest = new
            {
                email = email,
                password = password
            };
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", authenticateRequest);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].ToString();
        }

        private async Task<int> CreateCountryAndGetIdAsync(string token)
        {
            var requestBody = new
            {
                name = GenerateRandomString(),
                shortName = "CT"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(requestBody),
                Headers = { { "Authorization", $"Bearer {token}" } }
            };
            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].GetValue<int>();
        }

        private async Task<int> CreateHotelAndGetIdAsync(string token, int countryId)
        {
            var requestBody = new
            {
                name = GenerateRandomString(),
                address = "123 Hotel Address",
                rating = 4.5,
                countryId = countryId
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(requestBody),
                Headers = { { "Authorization", $"Bearer {token}" } }
            };
            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].GetValue<int>();
        }

        [Fact]
        public async Task TC015_Get_All_Hotels_When_Valid_Data_Returns_OK()
        {
            // Arrange
            var token = await CreateAuthenticatedUserAndGetTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels/all")
            {
                Headers = { { "Authorization", $"Bearer {token}" } }
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
        }

        [Fact]
        public async Task TC016_Create_Hotel_When_Valid_Data_Returns_Created()
        {
            // Arrange
            var token = await CreateAuthenticatedUserAndGetTokenAsync();
            int countryId = await CreateCountryAndGetIdAsync(token);
            var requestBody = new
            {
                name = GenerateRandomString(),
                address = "123 Hotel Address",
                rating = 4.5,
                countryId = countryId
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(requestBody),
                Headers = { { "Authorization", $"Bearer {token}" } }
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body?["id"]);
            Assert.Equal(requestBody.name, body?["name"].ToString());
            Assert.Equal(requestBody.address, body?["address"].ToString());
            Assert.Equal(requestBody.rating, body?["rating"].GetValue<double>());
            Assert.Equal(countryId, body?["countryId"].GetValue<int>());
        }

        [Fact]
        public async Task TC017_Create_Hotel_When_Missing_Required_Fields_Returns_BadRequest()
        {
            // Arrange
            var token = await CreateAuthenticatedUserAndGetTokenAsync();
            int countryId = await CreateCountryAndGetIdAsync(token);
            var requestBody = new
            {
                name = (string?)null,
                address = "123 Hotel Address",
                rating = 4.5,
                countryId = countryId
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(requestBody),
                Headers = { { "Authorization", $"Bearer {token}" } }
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}