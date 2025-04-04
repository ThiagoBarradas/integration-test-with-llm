// File: RestaurantEndpointsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using Restaurants.Application.Restaurants.Dtos;

namespace IntegrationTests
{
    public class RestaurantEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetRestaurantByIdAsync(int id)
        {
            return await _client.GetAsync($"/api/restaurants/{id}");
        }

        private async Task<HttpResponseMessage> PatchRestaurantAsync(int id, JsonObject requestBody)
        {
            var response = await _client.PatchAsJsonAsync($"/api/restaurants/{id}", requestBody);
            return response;
        }

        private async Task<HttpResponseMessage> DeleteRestaurantAsync(int id)
        {
            return await _client.DeleteAsync($"/api/restaurants/{id}");
        }

        private async Task<HttpResponseMessage> UpdateRestaurantLogoAsync(int id, HttpContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/restaurants/{id}/logo")
            {
                Content = content
            };
            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> CreateRestaurantAsync(JsonObject requestBody)
        {
            return await _client.PostAsJsonAsync("/api/restaurants", requestBody);
        }

        [Fact]
        public async Task TC131_Create_Restaurant_With_Valid_Data_And_Retrieve_By_Id()
        {
            // arrange
            var requestBody = new JsonObject
            {
                { "name", "ValidRestaurant1" },
                { "description", "Valid description" },
                { "category", "Valid Category" },
                { "hasDelivery", true },
                { "contactEmail", "contact1@example.com" },
                { "contactNumber", "1234567890" },
                { "city", "Valid City" },
                { "street", "Valid Street" },
                { "postalCode", "12345" }
            };
            var response = await CreateRestaurantAsync(requestBody);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var restaurantResponse = await response.Content.ReadFromJsonAsync<RestaurantDto>();
            Assert.NotNull(restaurantResponse);
            int restaurantId = restaurantResponse.Id;

            // act
            var getResponse = await GetRestaurantByIdAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var retrievedRestaurant = await getResponse.Content.ReadFromJsonAsync<RestaurantDto>();
            Assert.Equal(requestBody["name"].AsValue().GetValue<string>(), retrievedRestaurant.Name);
        }

        [Fact]
        public async Task TC132_Delete_Restaurant_When_Valid_Id_Returns_NoContent()
        {
            // arrange
            var requestBody = new JsonObject
            {
                { "name", "DeletableRestaurant" },
                { "description", "Sample description" },
                { "category", "Sample Category" },
                { "hasDelivery", true },
                { "contactEmail", "contact@example.com" },
                { "contactNumber", "1234567890" },
                { "city", "Sample City" },
                { "street", "Sample Street" },
                { "postalCode", "12345" }
            };
            var response = await CreateRestaurantAsync(requestBody);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var restaurantResponse = await response.Content.ReadFromJsonAsync<RestaurantDto>();
            Assert.NotNull(restaurantResponse);
            int restaurantId = restaurantResponse.Id;

            // act
            var deleteResponse = await DeleteRestaurantAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task TC133_Delete_Restaurant_When_Id_Not_Found_Returns_NotFound()
        {
            // arrange
            int invalidRestaurantId = 999999; // Assume this ID does not exist

            // act
            var deleteResponse = await DeleteRestaurantAsync(invalidRestaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task TC134_Patch_Restaurant_When_Valid_Id_And_Data_Returns_OK()
        {
            // arrange
            var requestBody = new JsonObject
            {
                { "name", "PatchableRestaurant" },
                { "description", "Updated description" },
                { "hasDelivery", false }
            };
            var createResponse = await CreateRestaurantAsync(requestBody);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var restaurantResponse = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>();
            Assert.NotNull(restaurantResponse);
            int restaurantId = restaurantResponse.Id;

            // act
            var patchResponse = await PatchRestaurantAsync(restaurantId, requestBody);

            // assert
            Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            var updatedRestaurant = await patchResponse.Content.ReadFromJsonAsync<RestaurantDto>();
            Assert.Equal(requestBody["description"].AsValue().GetValue<string>(), updatedRestaurant.Description);
            Assert.Equal(requestBody["hasDelivery"].AsValue().GetValue<bool>(), updatedRestaurant.HasDelivery);
        }

        [Fact]
        public async Task TC135_Patch_Restaurant_When_Id_Not_Found_Returns_NotFound()
        {
            // arrange
            int invalidRestaurantId = 999999;
            var requestBody = new JsonObject
            {
                { "description", "Updated description" },
                { "hasDelivery", false }
            };

            // act
            var patchResponse = await PatchRestaurantAsync(invalidRestaurantId, requestBody);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, patchResponse.StatusCode);
        }

        [Fact]
        public async Task TC136_Update_Restaurant_Logo_With_Valid_Image_File_Returns_OK()
        {
            // arrange
            var requestBody = new JsonObject
            {
                { "name", "LogoRestaurant" },
                { "description", "Sample description" },
                { "category", "Sample Category" },
                { "hasDelivery", true },
                { "contactEmail", "contact@example.com" },
                { "contactNumber", "1234567890" },
                { "city", "Sample City" },
                { "street", "Sample Street" },
                { "postalCode", "12345" }
            };
            var createResponse = await CreateRestaurantAsync(requestBody);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var restaurantResponse = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>();
            Assert.NotNull(restaurantResponse);
            int restaurantId = restaurantResponse.Id;
            var fileContent = new byte[] { 0x01, 0x02, 0x03 }; // Simulate image file content
            using var streamContent = new StreamContent(new MemoryStream(fileContent));
            using var multipartContent = new MultipartFormDataContent
            {
                { streamContent, "file", "image.jpg" }
            };

            // act
            var updateLogoResponse = await UpdateRestaurantLogoAsync(restaurantId, multipartContent);

            // assert
            Assert.Equal(HttpStatusCode.OK, updateLogoResponse.StatusCode);
            var updatedRestaurant = await updateLogoResponse.Content.ReadFromJsonAsync<RestaurantDto>();
            Assert.NotNull(updatedRestaurant);
        }

        [Fact]
        public async Task TC137_Update_Restaurant_Logo_With_Unsupported_Media_Type_Returns_UnsupportedMediaType()
        {
            // arrange
            int restaurantId = 1; // Assume this is a valid restaurantId created prior to this test
            var jsonContent = JsonContent.Create(new { file = "not-an-image" });

            // act
            var updateLogoResponse = await UpdateRestaurantLogoAsync(restaurantId, jsonContent);

            // assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, updateLogoResponse.StatusCode);
        }
    }
}
