// File: DishesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Xunit;

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

        private async Task<HttpResponseMessage> CreateDishAsync(int restaurantId, string name, string description, double? price, int? kiloCalories)
        {
            var requestBody = new
            {
                name = name,
                description = description,
                price = price,
                kiloCalories = kiloCalories
            };

            return await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", requestBody);
        }

        private async Task<HttpResponseMessage> GetDishesAsync(int restaurantId)
        {
            return await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes");
        }

        private async Task<HttpResponseMessage> DeleteDishesAsync(int restaurantId)
        {
            return await _client.DeleteAsync($"/api/restaurants/{restaurantId}/dishes");
        }

        private async Task<HttpResponseMessage> GetDishByIdAsync(int restaurantId, int dishId)
        {
            return await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{dishId}");
        }

        private async Task<int> CreateRestaurantAsync()
        {
            var requestBody = new
            {
                name = "ValidName",
                description = "Valid description",
                category = "ValidCategory",
                hasDelivery = true,
                contactEmail = "valid@example.com",
                contactNumber = "1234567890",
                city = "ValidCity",
                street = "ValidStreet",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync("/api/restaurants", requestBody);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        [Fact]
        public async Task TC001_Create_Dish_When_Valid_Data_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "Valid description";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(price, body["price"].AsValue().GetValue<double>());
            Assert.Equal(kiloCalories, body["kiloCalories"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC002_Create_Dish_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = null;
            string description = "Valid description";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Dish_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "";
            string description = "Valid description";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Dish_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ab";
            string description = "Valid description";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Dish_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ThisIsAVeryLongRestaurantName123";
            string description = "Valid description";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Dish_When_Name_Has_Minimum_Size_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "abc";
            string description = "Valid description";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(price, body["price"].AsValue().GetValue<double>());
            Assert.Equal(kiloCalories, body["kiloCalories"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC007_Create_Dish_When_Name_Has_Maximum_Size_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ThisIsAVeryLong1";
            string description = "Valid description";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(price, body["price"].AsValue().GetValue<double>());
            Assert.Equal(kiloCalories, body["kiloCalories"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC008_Create_Dish_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = null;
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Dish_When_Description_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Dish_When_Description_Too_Short_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "ab";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Dish_When_Description_Too_Long_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "ThisIsAVeryLongDescription1234567";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_Dish_When_Description_Has_Minimum_Size_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "abc";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(price, body["price"].AsValue().GetValue<double>());
            Assert.Equal(kiloCalories, body["kiloCalories"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC013_Create_Dish_When_Description_Has_Maximum_Size_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "ThisIsAVeryLongDescription1234";
            double price = 10.5;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(price, body["price"].AsValue().GetValue<double>());
            Assert.Equal(kiloCalories, body["kiloCalories"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC014_Create_Dish_When_Price_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "Valid description";
            double? price = null;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_Dish_When_Price_Is_Zero_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "Valid description";
            double price = 0.0;
            int kiloCalories = 300;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);
            
            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(price, body["price"].AsValue().GetValue<double>());
            Assert.Equal(kiloCalories, body["kiloCalories"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC016_Create_Dish_When_KiloCalories_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "Valid description";
            double price = 10.5;
            int? kiloCalories = null;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_Dish_When_KiloCalories_Is_Zero_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "Valid description";
            double price = 10.5;
            int kiloCalories = 0;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(price, body["price"].AsValue().GetValue<double>());
            Assert.Equal(kiloCalories, body["kiloCalories"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC018_Create_Dish_When_KiloCalories_Is_Negative_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "Valid description";
            double price = 10.5;
            int kiloCalories = -1;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Get_Dish_When_Valid_Data_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "ValidDescription";
            double price = 10;
            int kiloCalories = 30;
            await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // act
            var response = await GetDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.True(body.Count > 0);
            foreach (var dish in body)
            {
                Assert.True(dish["id"].AsValue().GetValue<int>() > 0);
                Assert.NotNull(dish["name"].AsValue().GetValue<string>());
                Assert.NotNull(dish["description"].AsValue().GetValue<string>());
                Assert.True(dish["price"].AsValue().GetValue<double>() > 0);
                Assert.True(dish["kiloCalories"].AsValue().GetValue<int>() >= 0);
            }
        }

        [Fact]
        public async Task TC020_Get_Dish_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int restaurantId = 9999999; // Invalid restaurant ID

            // act
            var response = await GetDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Delete_Dish_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();

            // act
            var response = await DeleteDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Delete_Dish_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int restaurantId = 9999999; // Invalid restaurant ID

            // act
            var response = await DeleteDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Get_Dish_By_Id_When_Valid_Data_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAsync();
            string name = "ValidDishName";
            string description = "ValidDescription";
            double price = 10;
            int kiloCalories = 30;
            var createDishResponse = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);
            var dishId = (await createDishResponse.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();

            // act
            var response = await GetDishByIdAsync(restaurantId, dishId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(dishId, body["id"].AsValue().GetValue<int>());
            Assert.NotNull(body["name"].AsValue().GetValue<string>());
            Assert.NotNull(body["description"].AsValue().GetValue<string>());
            Assert.True(body["price"].AsValue().GetValue<double>() > 0);
            Assert.True(body["kiloCalories"].AsValue().GetValue<int>() >= 0);
        }

        [Fact]
        public async Task TC024_Get_Dish_By_Id_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int restaurantId = 9999999; // Invalid restaurant ID
            int dishId = 1; // Valid dish ID

            // act
            var response = await GetDishByIdAsync(restaurantId, dishId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}