// File: RestaurantsIntegrationTests2.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IntegrationTests
{
    public class RestaurantsIntegrationTests2 : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantsIntegrationTests2(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
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

        private async Task<HttpResponseMessage> UploadRestaurantLogoAsync(int id, HttpContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/restaurants/{id}/logo")
            {
                Content = content
            };
            return await _client.SendAsync(request);
        }

        private async Task<int> CreateRestaurantAndGetIdAsync(string name, string description, string category, bool hasDelivery, string contactEmail, string contactNumber, string city, string street, string postalCode)
        {
            var requestBody = new
            {
                name = name.Substring(0, (name.Length > 32 ? 32 : name.Length)),
                description = description,
                category = category,
                hasDelivery = hasDelivery,
                contactEmail = contactEmail,
                contactNumber = contactNumber,
                city = city,
                street = street,
                postalCode = postalCode
            };
            var response = await _client.PostAsJsonAsync("/api/restaurants", requestBody);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            response.EnsureSuccessStatusCode();
            return body["id"].AsValue().GetValue<int>();
        }

        [Fact]
        public async Task TC050_Get_Restaurant_Valid_ID()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantGetValidId", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");

            // act
            var response = await GetRestaurantAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC051_Get_Restaurant_Invalid_ID()
        {
            // arrange
            int restaurantId = 999999;

            // act
            var response = await GetRestaurantAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC052_Update_Restaurant_Valid_Data()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantUpdateValidData", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Updated Restaurant Name";
            string description = "Updated Restaurant Description";
            bool hasDelivery = false;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC053_Update_Restaurant_Missing_Name()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantUpdateMissingName", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = null;
            string description = "Updated Restaurant Description";
            bool hasDelivery = false;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC054_Update_Restaurant_Name_Too_Short()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantUpdateNameTooShort", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Up";
            string description = "Updated Restaurant Description";
            bool hasDelivery = false;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC055_Update_Restaurant_Name_Too_Long()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantUpdateNameTooLong", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "ThisUpdatedRestaurantNameIsWayTooLongToMeetTheRequirementsOfTheTest";
            string description = "Updated Restaurant Description";
            bool hasDelivery = false;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC056_Update_Restaurant_Missing_Description()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantUpdateMissingDescription", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Updated Restaurant Name";
            string description = null;
            bool hasDelivery = false;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC057_Update_Restaurant_Description_Too_Short()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantUpdateDescriptionTooShort", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Updated Restaurant Name";
            string description = "Up";
            bool hasDelivery = false;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC058_Update_Restaurant_Description_Too_Long()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantUpdateDescriptionTooLong", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Updated Restaurant Name";
            string description = "ThisUpdatedRestaurantDescriptionIsWayTooLongToMeetTheRequirementsOfTheTest";
            bool hasDelivery = false;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC059_Update_Restaurant_Invalid_ID()
        {
            // arrange
            int restaurantId = 999999;
            string name = "Updated Restaurant Name";
            string description = "Updated Restaurant Description";
            bool hasDelivery = false;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC060_Delete_Restaurant_Valid_ID()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantDeleteValidId", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");

            // act
            var response = await DeleteRestaurantAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC061_Delete_Restaurant_Invalid_ID()
        {
            // arrange
            int restaurantId = 999999;

            // act
            var response = await DeleteRestaurantAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC062_Upload_Restaurant_Logo_Valid_Data()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantUploadValidLogo", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            var fileContent = new byte[] { 0x01, 0x02, 0x03 }; // Simulate image file
            var streamContent = new StreamContent(new MemoryStream(fileContent));
            using var content = new MultipartFormDataContent
            {
                { streamContent, "file", "test.jpg" }
            };

            // act
            var response = await UploadRestaurantLogoAsync(restaurantId, content);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC063_Upload_Restaurant_Logo_Invalid_ID()
        {
            // arrange
            int restaurantId = 999999;
            var fileContent = new byte[] { 0x01, 0x02, 0x03 }; // Simulate image file
            var streamContent = new StreamContent(new MemoryStream(fileContent));
            using var content = new MultipartFormDataContent
            {
                { streamContent, "file", "test.jpg" }
            };

            // act
            var response = await UploadRestaurantLogoAsync(restaurantId, content);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC064_Upload_Restaurant_Logo_Invalid_File_Type()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("RestaurantUploadInvalidFileType", "Restaurant Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            var fileContent = new byte[] { 0x01, 0x02, 0x03 }; // Simulate text file
            var streamContent = new StreamContent(new MemoryStream(fileContent));
            using var content = new MultipartFormDataContent
            {
                { streamContent, "file", "test.txt" }
            };

            // act
            var response = await UploadRestaurantLogoAsync(restaurantId, content);

            // assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }
    }
}
