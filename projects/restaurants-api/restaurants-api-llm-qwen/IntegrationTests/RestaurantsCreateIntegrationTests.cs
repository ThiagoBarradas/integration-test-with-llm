// File: RestaurantsCreateIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class RestaurantsCreateIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantsCreateIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<JsonDocument> CreateRestaurantAsync(string name, string description, string category, bool hasDelivery, string contactEmail, string contactNumber, string city, string street, string postalCode)
        {
            var request = new
            {
                name = name,
                description = description,
                category = category,
                hasDelivery = hasDelivery,
                contactEmail = contactEmail,
                contactNumber = contactNumber,
                city = city,
                street = street,
                postalCode = postalCode
            };

            var response = await _client.PostAsJsonAsync("/api/restaurants", request);
            return await response.Content.ReadFromJsonAsync<JsonDocument>();
        }

        [Fact]
        public async Task TC032_Post_Restaurant_When_Valid_Data_Returns_Created()
        {
            // arrange
            var request = new
            {
                name = "Pizza Place",
                description = "Delicious pizzas",
                category = "Italian",
                hasDelivery = true,
                contactEmail = "contact@example.com",
                contactNumber = "123456789",
                city = "Example City",
                street = "Example Street",
                postalCode = "12345"
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/restaurants", request);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("Pizza Place", responseBody.RootElement.GetProperty("name").GetString());
            Assert.Equal("Delicious pizzas", responseBody.RootElement.GetProperty("description").GetString());
            Assert.Equal("Italian", responseBody.RootElement.GetProperty("category").GetString());
            Assert.Equal(true, responseBody.RootElement.GetProperty("hasDelivery").GetBoolean());
            Assert.Equal("Example City", responseBody.RootElement.GetProperty("city").GetString());
            Assert.Equal("Example Street", responseBody.RootElement.GetProperty("street").GetString());
            Assert.Equal("12345", responseBody.RootElement.GetProperty("postalCode").GetString());
        }

        [Fact]
        public async Task TC033_Post_Restaurant_When_Invalid_ContactEmail_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                name = "Pizza Place",
                description = "Delicious pizzas",
                category = "Italian",
                hasDelivery = true,
                contactEmail = "invalid-email",
                contactNumber = "123456789",
                city = "Example City",
                street = "Example Street",
                postalCode = "12345"
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/restaurants", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Post_Restaurant_When_Invalid_ContactNumber_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                name = "Pizza Place",
                description = "Delicious pizzas",
                category = "Italian",
                hasDelivery = true,
                contactEmail = "contact@example.com",
                contactNumber = "invalid-number",
                city = "Example City",
                street = "Example Street",
                postalCode = "12345"
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/restaurants", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Post_Restaurant_When_Invalid_PostalCode_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                name = "Pizza Place",
                description = "Delicious pizzas",
                category = "Italian",
                hasDelivery = true,
                contactEmail = "contact@example.com",
                contactNumber = "123456789",
                city = "Example City",
                street = "Example Street",
                postalCode = "invalid-code"
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/restaurants", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
