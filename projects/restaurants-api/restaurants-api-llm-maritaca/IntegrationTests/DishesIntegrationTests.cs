// File: DishesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.IO;
using Restaurants.Application.Dishes.Dtos;

namespace IntegrationTests
{
    public class DishesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DishesIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateDishAsync(int restaurantId, JsonObject requestBody)
        {
            return await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", requestBody);
        }

        private async Task<HttpResponseMessage> GetDishesAsync(int restaurantId)
        {
            return await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes");
        }

        private async Task<HttpResponseMessage> GetDishByIdAsync(int restaurantId, int dishId)
        {
            return await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{dishId}");
        }

        [Fact]
        public async Task TC101_Create_Dish_With_Valid_Data_Returns_Created()
        {
            // arrange
            int restaurantId = 1; // Assume this is a valid restaurantId created prior to this test
            var requestBody = new JsonObject
            {
                { "name", "Sample Dish" },
                { "description", "Sample description" },
                { "price", 19.95 },
                { "kiloCalories", 1200 }
            };

            // act
            var response = await CreateDishAsync(restaurantId, requestBody);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var dishResponse = await response.Content.ReadFromJsonAsync<DishDto>();
            Assert.NotNull(dishResponse);
            Assert.Equal(requestBody["name"].AsValue().GetValue<string>(), dishResponse.Name);
            Assert.Equal(requestBody["description"].AsValue().GetValue<string>(), dishResponse.Description);
            Assert.Equal(requestBody["price"].AsValue().GetValue<decimal>(), dishResponse.Price);
            Assert.Equal(requestBody["kiloCalories"].AsValue().GetValue<int>(), dishResponse.KiloCalories);
        }

        [Fact]
        public async Task TC102_Create_Dish_With_Missing_Name_Returns_BadRequest()
        {
            // arrange
            int restaurantId = 1;
            var requestBody = new JsonObject
            {
                { "description", "Sample description" },
                { "price", 19.95 },
                { "kiloCalories", 1200 }
            };

            // act
            var response = await CreateDishAsync(restaurantId, requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC103_Create_Dish_With_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            int restaurantId = 1;
            var requestBody = new JsonObject
            {
                { "name", "D" }, // name with 2 characters
                { "description", "Sample description" },
                { "price", 19.95 },
                { "kiloCalories", 1200 }
            };

            // act
            var response = await CreateDishAsync(restaurantId, requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC104_Create_Dish_With_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            int restaurantId = 1;
            var requestBody = new JsonObject
            {
                { "name", "ThisNameIsTooLong" }, // name with 17 characters
                { "description", "Sample description" },
                { "price", 19.95 },
                { "kiloCalories", 1200 }
            };

            // act
            var response = await CreateDishAsync(restaurantId, requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC105_Create_Dish_With_Empty_Description_Returns_BadRequest()
        {
            // arrange
            int restaurantId = 1;
            var requestBody = new JsonObject
            {
                { "name", "Sample Dish" },
                { "description", "" }, // empty description
                { "price", 19.95 },
                { "kiloCalories", 1200 }
            };

            // act
            var response = await CreateDishAsync(restaurantId, requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC106_Create_Dish_With_Description_Too_Long_Returns_BadRequest()
        {
            // arrange
            int restaurantId = 1;
            var requestBody = new JsonObject
            {
                { "name", "Sample Dish" },
                { "description", "ThisIsADescriptionThatIsWayTooLongForTheConstraintsExceeding32Chars" }, // description with 33 characters
                { "price", 19.95 },
                { "kiloCalories", 1200 }
            };

            // act
            var response = await CreateDishAsync(restaurantId, requestBody);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC107_Get_Dishes_By_Restaurant_When_Valid_Data_Returns_OK()
        {
            // arrange
            int restaurantId = 1; // Assume this is a valid restaurantId created prior to this test

            // act
            var response = await GetDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var dishes = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(dishes);
            Assert.NotNull(dishes["items"]);
        }

        [Fact]
        public async Task TC108_Get_Dishes_By_Restaurant_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int restaurantId = 999999; // Assume this is an invalid restaurantId

            // act
            var response = await GetDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC111_Get_Dish_By_Id_When_Valid_Data_Returns_OK()
        {
            // arrange
            int restaurantId = 1; // Assume this is a valid restaurantId
            // Assume that there is a dish with ID created prior to this test
            int dishId = 1;

            // act
            var response = await GetDishByIdAsync(restaurantId, dishId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var dish = await response.Content.ReadFromJsonAsync<DishDto>();
            Assert.NotNull(dish);
        }

        [Fact]
        public async Task TC112_Get_Dish_By_Id_When_Dish_Not_Found_Returns_NotFound()
        {
            // arrange
            int restaurantId = 1; // Assume this is a valid restaurantId
            int dishId = 999999; // Assume this is an invalid dishId

            // act
            var response = await GetDishByIdAsync(restaurantId, dishId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
