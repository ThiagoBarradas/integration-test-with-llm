// File: HotelsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Cryptography.X509Certificates;

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

        private async Task<HttpResponseMessage> CreateHotelAsync(string token, string name, string address, double? rating, int countryId)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(new
                {
                    name = name,
                    address = address,
                    rating = rating,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            return response;
        }

        private async Task<HttpResponseMessage> GetHotelByIdAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            return response;
        }

        private async Task<HttpResponseMessage> GetHotelAllAsync(string token, string query)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels/all?{query}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            return response;
        }

        private async Task<HttpResponseMessage> UpdateHotelAsync(string token, int id, string name, string address, double? rating, int countryId)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{id}")
            {
                Content = JsonContent.Create(new
                {
                    name = name,
                    address = address,
                    rating = rating,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            return response;
        }

        private async Task<HttpResponseMessage> DeleteHotelAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/hotels/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            return response;
        }

        [Fact]
        public async Task TC018_Get_All_Hotels_Returns_OK()
        {
            // arrange
            var token = await AuthenticateAsync();

            // act
            var response = await GetHotelAllAsync(token, null);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(content);
        }

        [Fact]
        public async Task TC019_G2_Get_Paged_List_Of_Hotels_Returns_OK()
        {
            // arrange
            var token = await AuthenticateAsync();

            // act
            var response = await GetHotelAllAsync(token, "pageNumber=1&pageSize=10");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<JsonDocument>();
            Assert.NotNull(content);
        }

        [Fact]
        public async Task TC020_Create_Hotel_With_Valid_Data_Returns_Created()
        {
            // arrange
            var token = await AuthenticateAsync();

            // act
            var response = await CreateHotelAsync(token, "New Hotel", "123 Main St", 4.5, 1);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<JsonDocument>();
            Assert.NotNull(content);
            Assert.Equal("New Hotel", content.RootElement.GetProperty("name").ToString());
        }

        [Fact]
        public async Task TC021_Update_Hotel_With_Valid_Data_Returns_NoContent()
        {
            // arrange
            var token = await AuthenticateAsync();
            var createResponse = await CreateHotelAsync(token, "Test Hotel", "456 Main St", 4.8, 1);
            var content = await createResponse.Content.ReadFromJsonAsync<JsonDocument>();
            int hotelId = content!.RootElement.GetProperty("id").GetInt32();

            // act
            var updateResponse = await UpdateHotelAsync(token, hotelId, "Updated Hotel", "789 Main St", 4.9, 1);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        }

        [Fact]
        public async Task TC022_Update_Hotel_With_Invalid_Id_Returns_NotFound()
        {
            // arrange
            var token = await AuthenticateAsync(); 

            // act
            var response = await UpdateHotelAsync(token, 999999, "Updated Hotel", "789 Main St", 4.9, 1);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Delete_Hotel_With_Valid_Id_Returns_NoContent()
        {
            // arrange
            var token = await AuthenticateAsync();
            var createResponse = await CreateHotelAsync(token, "Test Hotel", "456 Main St", 4.8, 1);
            var content = await createResponse.Content.ReadFromJsonAsync<JsonDocument>();
            int hotelId = content!.RootElement.GetProperty("id").GetInt32();

            // act
            var deleteResponse = await DeleteHotelAsync(token, hotelId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task TC024_Delete_Hotel_With_Invalid_Id_Returns_NotFound()
        {
            // arrange
            var token = await AuthenticateAsync();

            // act
            var response = await DeleteHotelAsync(token, 999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}