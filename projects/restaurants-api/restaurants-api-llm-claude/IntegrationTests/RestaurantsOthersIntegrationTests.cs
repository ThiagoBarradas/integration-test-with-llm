using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests
{
    public partial class RestaurantsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private async Task<int> CreateRestaurantAndGetIdAsync(string name)
        {
            var response = await CreateRestaurantAsync(
                name,
                $"Description",
                $"Category",
                true,
                $"{name.ToLower().Replace(" ", "")}@email.com",
                "12345678",
                $"City",
                $"Street",
                "12345"
            );

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        [Fact]
        public async Task TC025_Get_Restaurants_With_Valid_Query_Parameters_Returns_OK()
        {
            // arrange
            await CreateRestaurantAndGetIdAsync("Restaurant Test Get 1");
            await CreateRestaurantAndGetIdAsync("Restaurant Test Get 2");

            // act
            var response = await _client.GetAsync("/api/restaurants?pageNumber=1&pageSize=10&sortBy=Name&sortDirection=Ascending&searchPhrase=Restaurant");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body["items"]);
            Assert.True(body["totalPages"].AsValue().GetValue<int>() > 0);
            Assert.True(body["totalItemsCount"].AsValue().GetValue<int>() >= 0);
            Assert.True(body["itemsFrom"].AsValue().GetValue<int>() > 0);
            Assert.True(body["itemsTo"].AsValue().GetValue<int>() > 0);
        }

        [Fact]
        public async Task TC026_Get_Restaurants_With_Invalid_PageNumber_Returns_BadRequest()
        {
            // arrange & act
            var response = await _client.GetAsync("/api/restaurants?pageNumber=0&pageSize=10");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Get_Restaurants_With_Invalid_PageSize_Returns_BadRequest()
        {
            // arrange & act
            var response = await _client.GetAsync("/api/restaurants?pageNumber=1&pageSize=0");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Get_Restaurants_With_Invalid_SortBy_Returns_BadRequest()
        {
            // arrange & act
            var response = await _client.GetAsync("/api/restaurants?sortBy=InvalidField");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Get_Restaurant_By_Id_When_Found_Returns_OK()
        {
            // arrange
            var restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant Test Get By Id");

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(restaurantId, body["id"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC030_Get_Restaurant_By_Id_When_Not_Found_Returns_NotFound()
        {
            // arrange & act
            var response = await _client.GetAsync("/api/restaurants/999999");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Update_Restaurant_When_Valid_Data_Returns_OK()
        {
            // arrange
            var restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant Test Update");

            var updateRequest = new
            {
                name = "Updated Restaurant Name",
                description = "Updated Description",
                hasDelivery = false
            };

            // act
            var response = await _client.PatchAsJsonAsync($"/api/restaurants/{restaurantId}", updateRequest);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(restaurantId, body["id"].AsValue().GetValue<int>());
            Assert.Equal(updateRequest.name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(updateRequest.description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(updateRequest.hasDelivery, body["hasDelivery"].AsValue().GetValue<bool>());
        }

        [Fact]
        public async Task TC032_Update_Restaurant_When_Not_Found_Returns_NotFound()
        {
            // arrange
            var updateRequest = new
            {
                name = "Updated Restaurant Name",
                description = "Updated Description",
                hasDelivery = false
            };

            // act
            var response = await _client.PatchAsJsonAsync("/api/restaurants/999999", updateRequest);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Delete_Restaurant_When_Found_Returns_NoContent()
        {
            // arrange
            var restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant Test Delete");

            // act
            var response = await _client.DeleteAsync($"/api/restaurants/{restaurantId}");

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Delete_Restaurant_When_Not_Found_Returns_NotFound()
        {
            // arrange & act
            var response = await _client.DeleteAsync("/api/restaurants/999999");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Upload_Logo_When_Valid_Image_Returns_OK()
        {
            // arrange
            var restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant Test Logo Upload");

            var imageContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }); // JPEG magic numbers
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(imageContent, "file", "test-image.jpg");

            // act
            var response = await _client.PostAsync($"/api/restaurants/{restaurantId}/logo", multipartContent);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body["logoSasUrl"]);
        }

        [Fact]
        public async Task TC036_Upload_Logo_When_Invalid_File_Type_Returns_UnsupportedMediaType()
        {
            // arrange
            var restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant Test Logo Invalid");

            var textContent = new StringContent("not an image");
            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(textContent, "file", "test.txt");

            // act
            var response = await _client.PostAsync($"/api/restaurants/{restaurantId}/logo", multipartContent);

            // assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Upload_Logo_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            var imageContent = new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }); // JPEG magic numbers
            imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(imageContent, "file", "test-image.jpg");

            // act
            var response = await _client.PostAsync("/api/restaurants/999999/logo", multipartContent);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
