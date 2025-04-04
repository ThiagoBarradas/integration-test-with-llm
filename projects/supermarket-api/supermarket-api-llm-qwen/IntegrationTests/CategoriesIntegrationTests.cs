// File: CategoriesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using System;
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
            var request = new
            {
                name = name
            };

            return await _client.PostAsJsonAsync("/api/categories", request);
        }

        private async Task<HttpResponseMessage> UpdateCategoryAsync(int id, string name)
        {
            var request = new
            {
                name = name
            };

            return await _client.PutAsJsonAsync($"/api/categories/{id}", request);
        }

        private async Task<HttpResponseMessage> DeleteCategoryAsync(int id)
        {
            return await _client.DeleteAsync($"/api/categories/{id}");
        }

        [Fact]
        public async Task TC001_Get_Categories_When_No_Categories_Returns_OK()
        {
            // arrange

            // act
            var response = await GetCategoriesAsync();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<List<JsonObject>>();
            Assert.Empty(body);
        }

        [Fact]
        public async Task TC002_Get_Categories_When_Categories_Exist_Returns_OK()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("TestCategory");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();

            // act
            var response = await GetCategoriesAsync();
            var responseBody = await response.Content.ReadFromJsonAsync<List<JsonObject>>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(responseBody);
            var category = responseBody.Find(c => c["id"].AsValue().GetValue<int>() == categoryId);
            Assert.NotNull(category);
            Assert.Equal("TestCategory", category["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC003_Create_Category_When_Valid_Data_Returns_OK()
        {
            // arrange
            string name = "TestCategory";

            // act
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC004_Create_Category_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string name = null;

            // act
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Category_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string name = "";

            // act
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Category_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "";

            // act
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Category_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "TestCategory123456789012345678901";

            // act
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Category_When_Name_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            string name = "T";

            // act
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC009_Create_Category_When_Name_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            string name = "TestCategory12345678901234567890";

            // act
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC010_Update_Category_When_Valid_Data_Returns_OK()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("TestCategory");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();
            string updatedName = "UpdatedTestCategory";

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(categoryId, responseBody["id"].AsValue().GetValue<int>());
            Assert.Equal(updatedName, responseBody["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC011_Update_Category_When_ID_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int categoryId = 9999999;
            string updatedName = "UpdatedTestCategory";

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Update_Category_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("TestCategory");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();
            string updatedName = null;

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Update_Category_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("TestCategory");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();
            string updatedName = "";

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Update_Category_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("TestCategory");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();
            string updatedName = "";

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Update_Category_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("TestCategory");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();
            string updatedName = "TestCategory123456789012345678901";

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Update_Category_When_Name_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("TestCategory");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();
            string updatedName = "T";

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(categoryId, responseBody["id"].AsValue().GetValue<int>());
            Assert.Equal(updatedName, responseBody["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC017_Update_Category_When_Name_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("TestCategory");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();
            string updatedName = "TestCategory12345678901234567890";

            // act
            var response = await UpdateCategoryAsync(categoryId, updatedName);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(categoryId, responseBody["id"].AsValue().GetValue<int>());
            Assert.Equal(updatedName, responseBody["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC018_Delete_Category_When_Exists_Returns_OK()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("TestCategory");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteCategoryAsync(categoryId);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(categoryId, responseBody["id"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC019_Delete_Category_When_ID_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int categoryId = 9999999;

            // act
            var response = await DeleteCategoryAsync(categoryId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}