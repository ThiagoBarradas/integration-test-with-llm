// File: ProductsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Supermarket.API;

namespace IntegrationTests
{
    public class ProductsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProductsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetProductsAsync(int page, int itemsPerPage, int? categoryId = null)
        {
            var queryString = $"page={page}&itemsPerPage={itemsPerPage}";
            if (categoryId != null)
            {
                queryString += $"&categoryId={categoryId}";
            }
            return await _client.GetAsync($"/api/products?{queryString}");
        }

        private async Task<HttpResponseMessage> CreateProductAsync(int categoryId, string name, int quantityInPackage, string unitOfMeasurement)
        {
            var request = new
            {
                categoryId = categoryId,
                name = name,
                quantityInPackage = quantityInPackage,
                unitOfMeasurement = unitOfMeasurement
            };

            return await _client.PostAsJsonAsync("/api/products", request);
        }

        private async Task<HttpResponseMessage> UpdateProductAsync(int id, int categoryId, string name, int quantityInPackage, string unitOfMeasurement)
        {
            var request = new
            {
                categoryId = categoryId,
                name = name,
                quantityInPackage = quantityInPackage,
                unitOfMeasurement = unitOfMeasurement
            };

            return await _client.PutAsJsonAsync($"/api/products/{id}", request);
        }

        private async Task<HttpResponseMessage> DeleteProductAsync(int id)
        {
            return await _client.DeleteAsync($"/api/products/{id}");
        }

        private async Task<int> CreateCategoryAndGetIdAsync(string name)
        {
            var createResponse = await CreateCategoryAsync(name);
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> CreateCategoryAsync(string name)
        {
            var request = new
            {
                name = name
            };

            return await _client.PostAsJsonAsync("/api/categories", request);
        }

        [Fact]
        public async Task TC020_Get_Products_When_No_Products_Returns_OK()
        {
            // arrange

            // act
            var response = await GetProductsAsync(1, 10);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(0, responseBody["totalItems"].AsValue().GetValue<int>());
            Assert.Empty(responseBody["items"].AsArray());
        }

        [Fact]
        public async Task TC021_Get_Products_When_Products_Exist_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");
            var createProductResponse = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var createProductBody = await createProductResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = createProductBody["id"].AsValue().GetValue<int>();

            // act
            var response = await GetProductsAsync(1, 10);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var products = responseBody["items"].AsArray();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, responseBody["totalItems"].AsValue().GetValue<int>());
            Assert.NotEmpty(products);
            var product = products[0].AsObject();
            Assert.Equal(productId, product["id"].AsValue().GetValue<int>());
            Assert.Equal("TestProduct", product["name"].AsValue().GetValue<string>());
            Assert.Equal(10, product["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal("Unity", product["unitOfMeasurement"].AsValue().GetValue<string>());
            var category = product["category"].AsObject();
            Assert.Equal(categoryId, category["id"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC022_Get_Products_When_Valid_Query_Filter_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");
            var createProductResponse = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var createProductBody = await createProductResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = createProductBody["id"].AsValue().GetValue<int>();

            // act
            var response = await GetProductsAsync(1, 10, categoryId);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var products = responseBody["items"].AsArray();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(1, responseBody["totalItems"].AsValue().GetValue<int>());
            Assert.NotEmpty(products);
            var product = products[0].AsObject();
            Assert.Equal(productId, product["id"].AsValue().GetValue<int>());
            Assert.Equal("TestProduct", product["name"].AsValue().GetValue<string>());
            Assert.Equal(10, product["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal("Unity", product["unitOfMeasurement"].AsValue().GetValue<string>());
            var category = product["category"].AsObject();
            Assert.Equal(categoryId, category["id"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC023_Get_Products_When_Invalid_CategoryId_Returns_BadRequest()
        {
            // arrange

            // act
            var response = await GetProductsAsync(1, 10, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Get_Products_When_Page_Out_Of_Bounds_Returns_BadRequest()
        {
            // arrange

            // act
            var response = await GetProductsAsync(0, 10);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Get_Products_When_ItemsPerPage_Out_Of_Bounds_Returns_BadRequest()
        {
            // arrange

            // act
            var response = await GetProductsAsync(1, 0);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Create_Product_When_Valid_Data_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(responseBody["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal("TestProduct", responseBody["name"].AsValue().GetValue<string>());
            Assert.Equal(10, responseBody["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal("Unity", responseBody["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC027_Create_Product_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, null, 10, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Create_Product_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "", 10, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Create_Product_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "", 10, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Create_Product_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "TestProduct1234567890123456789012345678901", 10, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Create_Product_When_Name_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "T", 10, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(responseBody["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal("T", responseBody["name"].AsValue().GetValue<string>());
            Assert.Equal(10, responseBody["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal("Unity", responseBody["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC032_Create_Product_When_Name_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "TestProduct123456789012345678901234567890", 10, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(responseBody["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal("TestProduct123456789012345678901234567890", responseBody["name"].AsValue().GetValue<string>());
            Assert.Equal(10, responseBody["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal("Unity", responseBody["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC033_Create_Product_When_QuantityInPackage_Below_Minimum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "TestProduct", -1, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Create_Product_When_QuantityInPackage_Above_Maximum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "TestProduct", 101, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Create_Product_When_QuantityInPackage_Has_Minimum_Value_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "TestProduct", 0, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(responseBody["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal("TestProduct", responseBody["name"].AsValue().GetValue<string>());
            Assert.Equal(0, responseBody["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal("Unity", responseBody["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC036_Create_Product_When_QuantityInPackage_Has_Maximum_Value_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "TestProduct", 100, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(responseBody["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal("TestProduct", responseBody["name"].AsValue().GetValue<string>());
            Assert.Equal(100, responseBody["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal("Unity", responseBody["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC037_Create_Product_When_UnitOfMeasurement_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");

            // act
            var response = await CreateProductAsync(categoryId, "TestProduct", 10, "InvalidUnit");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Create_Product_When_CategoryId_Is_Invalid_Returns_BadRequest()
        {
            // arrange

            // act
            var response = await CreateProductAsync(9999999, "TestProduct", 10, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Update_Product_When_Valid_Data_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");
            var createProductResponse = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var createProductBody = await createProductResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = createProductBody["id"].AsValue().GetValue<int>();
            string updatedName = "UpdatedTestProduct";

            // act
            var response = await UpdateProductAsync(productId, categoryId, updatedName, 20, "Kilogram");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(productId, responseBody["id"].AsValue().GetValue<int>());
            Assert.Equal(updatedName, responseBody["name"].AsValue().GetValue<string>());
            Assert.Equal(20, responseBody["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal("Kilogram", responseBody["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC040_Update_Product_When_ID_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int productId = 9999999;

            // act
            var response = await UpdateProductAsync(productId, 1, "UpdatedTestProduct", 20, "Kilogram");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Update_Product_When_QuantityInPackage_Below_Minimum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");
            var createProductResponse = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var createProductBody = await createProductResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = createProductBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateProductAsync(productId, categoryId, "TestProduct", -1, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Update_Product_When_QuantityInPackage_Above_Maximum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");
            var createProductResponse = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var createProductBody = await createProductResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = createProductBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateProductAsync(productId, categoryId, "TestProduct", 101, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Update_Product_When_UnitOfMeasurement_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");
            var createProductResponse = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var createProductBody = await createProductResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = createProductBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateProductAsync(productId, categoryId, "TestProduct", 10, "InvalidUnit");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Update_Product_When_CategoryId_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");
            var createProductResponse = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var createProductBody = await createProductResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = createProductBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateProductAsync(productId, 9999999, "TestProduct", 10, "Unity");
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Delete_Product_When_Exists_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");
            var createProductResponse = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var createProductBody = await createProductResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = createProductBody["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteProductAsync(productId);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(productId, responseBody["id"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC046_Delete_Product_When_ID_Does_Not_Exist_Returns_BadRequest()
        {
            // arrange
            int productId = 9999999;

            // act
            var response = await DeleteProductAsync(productId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Delete_Product_When_Valid_Data_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("TestCategory");
            var createProductResponse = await CreateProductAsync(categoryId, "TestProduct", 10, "Unity");
            var createProductBody = await createProductResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = createProductBody["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteProductAsync(productId);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(productId, responseBody["id"].AsValue().GetValue<int>());
        }
    }
}