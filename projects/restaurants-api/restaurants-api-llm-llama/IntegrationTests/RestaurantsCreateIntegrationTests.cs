// File: RestaurantsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace IntegrationTests
{
    public partial class RestaurantsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetRestaurantsAsync(string searchPhrase = null, int? pageNumber = null, int? pageSize = null, string sortBy = null, string sortDirection = null)
        {
            var queryParams = new Dictionary<string, object>();

            if (searchPhrase != null)
            {
                queryParams.Add("searchPhrase", searchPhrase);
            }

            if (pageNumber.HasValue)
            {
                queryParams.Add("pageNumber", pageNumber.Value);
            }

            if (pageSize.HasValue)
            {
                queryParams.Add("pageSize", pageSize.Value);
            }

            if (sortBy != null)
            {
                queryParams.Add("sortBy", sortBy);
            }

            if (sortDirection != null)
            {
                queryParams.Add("sortDirection", sortDirection);
            }

            var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"));
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/restaurants/?{queryString}");

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> CreateRestaurantAsync(string name, string description, string contactEmail, string contactNumber, string category, bool hasDelivery, string city, string street, string postalCode)
        {
            var requestBody = new
            {
                name = name,
                description = description,
                contactEmail = contactEmail,
                contactNumber = contactNumber,
                category = category,
                hasDelivery = hasDelivery,
                city = city,
                street = street,
                postalCode = postalCode
            };

            return await _client.PostAsJsonAsync("/api/restaurants", requestBody);
        }

        private async Task<HttpResponseMessage> GetRestaurantAsync(int id)
        {
            return await _client.GetAsync($"/api/restaurants/{id}");
        }

        private async Task<HttpResponseMessage> UpdateRestaurantAsync(int id, string name, string description, bool hasDelivery)
        {
            var requestBody = new
            {
                name = name,
                description = description,
                hasDelivery = hasDelivery
            };

            return await _client.PatchAsJsonAsync($"/api/restaurants/{id}", requestBody);
        }

        private async Task<HttpResponseMessage> DeleteRestaurantAsync(int id)
        {
            return await _client.DeleteAsync($"/api/restaurants/{id}");
        }

        [Fact]
        public async Task TC023_Create_Restaurant_When_Valid_Data_Returns_Created()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = "1234567890";
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Create_Restaurant_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            var name = (string)null;
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = "1234567890";
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Restaurant_When_Contact_Email_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "invalid_email";
            var contactNumber = "1234567890";
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Create_Restaurant_When_Contact_Number_Is_Null_Returns_BadRequest()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = (string)null;
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Create_Restaurant_When_Contact_Number_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = string.Empty;
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Create_Restaurant_When_Contact_Number_Contains_Non_Number_Characters_Returns_BadRequest()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = "123abc";
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}