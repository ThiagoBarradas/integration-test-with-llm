// File: CategoriesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using Supermarket.API;

namespace IntegrationTests
{
    public class CategoriesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CategoriesIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetCategoriesAsync()
        {
            return await _client.GetAsync("/api/categories");
        }

        private async Task<HttpResponseMessage> CreateCategoryAsync(string name)
        {
            var request = new { name };
            return await _client.PostAsJsonAsync("/api/categories", request);
        }

        private async Task<HttpResponseMessage> UpdateCategoryAsync(int id, string name)
        {
            var request = new { name };
            return await _client.PutAsJsonAsync($"/api/categories/{id}", request);
        }

        private async Task<HttpResponseMessage> DeleteCategoryAsync(int id)
        {
            return await _client.DeleteAsync($"/api/categories/{id}");
        }

        [Fact]
        public async Task TC001_Get_Categories_When_No_Data_Exists_Returns_Empty()
        {
            // act
            var response = await GetCategoriesAsync();

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
            Assert.Empty(body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Get_Categories_When_Data_Exists_Returns_OK()
        {
            // arrange
            string categoryName1 = "Fruits";
            string categoryName2 = "Vegetables";

            await CreateCategoryAsync(categoryName1);
            await CreateCategoryAsync(categoryName2);

            // act
            var response = await GetCategoriesAsync();

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
            Assert.Equal(2, body.Count);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Save_Category_When_Valid_Data_Returns_OK()
        {
            // arrange
            string categoryName = "Beverages";

            // act
            var response = await CreateCategoryAsync(categoryName);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            var id = body["id"].AsValue().GetValue<int>();
            var name = body["name"].AsValue().GetValue<string>();
            Assert.True(id > 0);
            Assert.Equal(categoryName, name);
        }

        [Fact]
        public async Task TC004_Save_Category_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string categoryName = null;

            // act
            var response = await CreateCategoryAsync(categoryName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Save_Category_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string categoryName = "";

            // act
            var response = await CreateCategoryAsync(categoryName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Save_Category_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            string categoryName = new string('A', 31); // Name length: 31

            // act
            var response = await CreateCategoryAsync(categoryName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Save_Category_When_Name_Has_Minimum_Length_Returns_OK()
        {
            // arrange
            string categoryName = "A"; // Name length: 1

            // act
            var response = await CreateCategoryAsync(categoryName);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            var id = body["id"].AsValue().GetValue<int>();
            var name = body["name"].AsValue().GetValue<string>();
            Assert.True(id > 0);
            Assert.Equal(categoryName, name);
        }

        [Fact]
        public async Task TC008_Save_Category_When_Name_Has_Maximum_Length_Returns_OK()
        {
            // arrange
            string categoryName = new string('A', 30); // Name length: 30

            // act
            var response = await CreateCategoryAsync(categoryName);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            var id = body["id"].AsValue().GetValue<int>();
            var name = body["name"].AsValue().GetValue<string>();
            Assert.True(id > 0);
            Assert.Equal(categoryName, name);
        }

        [Fact]
        public async Task TC009_Update_Category_When_Valid_Data_Returns_OK()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("Fruits");
            var createdCategory = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = createdCategory["id"].AsValue().GetValue<int>();
            string updatedName = "Dry Fruits";

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal(categoryId, body["id"].AsValue().GetValue<int>());
            Assert.Equal(updatedName, body["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC010_Update_Category_When_Id_Not_Exists_Returns_NotFound()
        {
            // arrange
            int nonExistentId = 9999;
            string updatedName = "Dry Fruits";

            // act
            var response = await UpdateCategoryAsync(nonExistentId, updatedName);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Update_Category_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("Fruits");
            var createdCategory = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = createdCategory["id"].AsValue().GetValue<int>();
            string updatedName = null;

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Delete_Category_When_Id_Exists_Returns_OK()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("Fruits");
            var createdCategory = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = createdCategory["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteCategoryAsync(categoryId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Delete_Category_When_Id_Not_Exists_Returns_NotFound()
        {
            // arrange
            int nonExistentId = 9999;

            // act
            var response = await DeleteCategoryAsync(nonExistentId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}