// File: CategoriesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using Supermarket.API;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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

        private async Task<HttpResponseMessage> ListCategoriesAsync()
        {
            return await _client.GetAsync("/api/categories");
        }

        private async Task<HttpResponseMessage> SaveCategoryAsync(string name)
        {
            var requestBody = new
            {
                name = name
            };

            return await _client.PostAsJsonAsync("/api/categories", requestBody);
        }

        private async Task<HttpResponseMessage> UpdateCategoryAsync(int id, string name)
        {
            var requestBody = new
            {
                name = name
            };

            return await _client.PutAsJsonAsync($"/api/categories/{id}", requestBody);
        }

        private async Task<HttpResponseMessage> DeleteCategoryAsync(int id)
        {
            return await _client.DeleteAsync($"/api/categories/{id}");
        }

        [Fact]
        public async Task TC001_List_Categories_Returns_OK()
        {
            // Act
            var response = await ListCategoriesAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Save_Category_When_Valid_Data_Returns_OK()
        {
            // Act
            var response = await SaveCategoryAsync("Valid Category");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Save_Category_When_Name_Is_Null_Returns_BadRequest()
        {
            // Act
            var response = await SaveCategoryAsync(null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Save_Category_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // Act
            var response = await SaveCategoryAsync("");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Save_Category_When_Name_Too_Short_Returns_BadRequest()
        {
            // Act
            var response = await SaveCategoryAsync("");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Save_Category_When_Name_Too_Long_Returns_BadRequest()
        {
            // Act
            var response = await SaveCategoryAsync("This is a very long name for category, lets test it.");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Save_Category_When_Name_Has_Minimum_Size_Returns_OK()
        {
            // Act
            var response = await SaveCategoryAsync("C");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Save_Category_When_Name_Has_Maximum_Size_Returns_OK()
        {
            // Act
            var response = await SaveCategoryAsync("This is category with maximum size of 30.");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Update_Category_When_Valid_Data_Returns_OK()
        {
            // Arrange
            var id = await CreateCategoryId();

            // Act
            var response = await UpdateCategoryAsync(id, "Valid Category");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Update_Category_When_ID_Not_Found_Returns_NotFound()
        {
            // Arrange
            var id = 9999;

            // Act
            var response = await UpdateCategoryAsync(id, "Valid Category");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Update_Category_When_Name_Is_Null_Returns_BadRequest()
        {
            // Arrange
            var id = await CreateCategoryId();

            // Act
            var response = await UpdateCategoryAsync(id, null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Update_Category_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // Arrange
            var id = await CreateCategoryId();

            // Act
            var response = await UpdateCategoryAsync(id, "");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Update_Category_When_Name_Too_Short_Returns_BadRequest()
        {
            // Arrange
            var id = await CreateCategoryId();

            // Act
            var response = await UpdateCategoryAsync(id, "");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Update_Category_When_Name_Too_Long_Returns_BadRequest()
        {
            // Arrange
            var id = await CreateCategoryId();

            // Act
            var response = await UpdateCategoryAsync(id, "This is a very long name for category, lets test it.");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Update_Category_When_Name_Has_Minimum_Size_Returns_OK()
        {
            // Arrange
            var id = await CreateCategoryId();

            // Act
            var response = await UpdateCategoryAsync(id, "C");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Update_Category_When_Name_Has_Maximum_Size_Returns_OK()
        {
            // Arrange
            var id = await CreateCategoryId();

            // Act
            var response = await UpdateCategoryAsync(id, "This is category with maximum size of 30.");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Delete_Category_When_ID_Found_Returns_OK()
        {
            // Arrange
            var id = await CreateCategoryId();

            // Act
            var response = await DeleteCategoryAsync(id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Delete_Category_When_ID_Not_Found_Returns_NotFound()
        {
            // Act
            var response = await DeleteCategoryAsync(9999);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<int> CreateCategoryId()
        {
            var requestBody = new
            {
                name = "Valid Category"
            };

            var response = await _client.PostAsJsonAsync("/api/categories", requestBody);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }
    }
}