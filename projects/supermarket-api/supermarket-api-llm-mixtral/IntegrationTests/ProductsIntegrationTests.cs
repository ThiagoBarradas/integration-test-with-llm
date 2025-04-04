// File: ProductsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
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

        private async Task<HttpResponseMessage> CreateCategoryAsync(string name)
        {
            var request = new
            {
                name = name
            };

            return await _client.PostAsJsonAsync("/api/categories", request);
        }

        private async Task<HttpResponseMessage> CreateProductAsync(int categoryId, string name, int? quantityInPackage, string unitOfMeasurement)
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

        [Fact]
        public async Task TC019_List_All_Products_Returns_OK()
        {
            // arrange
            var categoryResponse1 = await CreateCategoryAsync("Category1");
            var categoryBody1 = await categoryResponse1.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId1 = categoryBody1["id"].AsValue().GetValue<int>();

            var categoryResponse2 = await CreateCategoryAsync("Category2");
            var categoryBody2 = await categoryResponse2.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId2 = categoryBody2["id"].AsValue().GetValue<int>();

            await CreateProductAsync(categoryId1, "Product1", 10, "Unity");
            await CreateProductAsync(categoryId2, "Product2", 20, "Unity");

            // act
            var response = await _client.GetAsync("/api/products?categoryId=1&page=1&itemsPerPage=10");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);            
        }

        [Fact]
        public async Task TC020_List_Products_When_Category_ID_Is_Null_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();

            await CreateProductAsync(categoryId, "Product1", 10, "Unity");

            // act
            var response = await _client.GetAsync("/api/products?categoryId=null&page=1&itemsPerPage=10");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_List_Products_When_Page_Is_Null_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();

            await CreateProductAsync(categoryId, "Product1", 10, "Unity");

            // act
            var response = await _client.GetAsync("/api/products?categoryId=1&page=null&itemsPerPage=10");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_List_Products_When_Items_Per_Page_Is_Null_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();

            await CreateProductAsync(categoryId, "Product1", 10, "Unity");

            // act
            var response = await _client.GetAsync("/api/products?categoryId=1&page=1&itemsPerPage=null");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_List_Products_When_Page_Is_Zero_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();

            await CreateProductAsync(categoryId, "Product1", 10, "Unity");

            // act
            var response = await _client.GetAsync("/api/products?categoryId=1&page=0&itemsPerPage=10");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_List_Products_When_Items_Per_Page_Is_Zero_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();

            await CreateProductAsync(categoryId, "Product1", 10, "Unity");

            // act
            var response = await _client.GetAsync("/api/products?categoryId=1&page=1&itemsPerPage=0");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Save_Product_When_Valid_Data_Returns_OK()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(quantityInPackage, body["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal(unitOfMeasurement, body["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC026_Save_Product_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = null;
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Save_Product_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Save_Product_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Save_Product_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = new string('a', 51);
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Save_Product_When_Name_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "a";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(quantityInPackage, body["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal(unitOfMeasurement, body["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC031_Save_Product_When_Name_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = new string('a', 50);
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(quantityInPackage, body["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal(unitOfMeasurement, body["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC032_Save_Product_When_Quantity_In_Package_Is_Null_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int? quantityInPackage = null;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Save_Product_When_Quantity_In_Package_Is_Below_Minimum_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = -1;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Save_Product_When_Quantity_In_Package_Is_Above_Maximum_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 101;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Save_Product_When_Quantity_In_Package_Has_Minimum_Value_Returns_OK()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 0;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(quantityInPackage, body["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal(unitOfMeasurement, body["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC036_Save_Product_When_Quantity_In_Package_Has_Maximum_Value_Returns_OK()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 100;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(quantityInPackage, body["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal(unitOfMeasurement, body["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC037_Save_Product_When_Unit_Of_Measurement_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 10;
            string unitOfMeasurement = "InvalidUnit";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Update_Product_When_Valid_Data_Returns_OK()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            var createResponse = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var productBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = productBody["id"].AsValue().GetValue<int>();

            string updatedName = "UpdatedProduct";
            int updatedQuantityInPackage = 20;
            string updatedUnitOfMeasurement = "Unity";

            // act
            var response = await UpdateProductAsync(productId, categoryId, updatedName, updatedQuantityInPackage, updatedUnitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updatedBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(updatedName, updatedBody["name"].AsValue().GetValue<string>());
            Assert.Equal(updatedQuantityInPackage, updatedBody["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal(updatedUnitOfMeasurement, updatedBody["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC039_Update_Product_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            var createResponse = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var productBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = productBody["id"].AsValue().GetValue<int>();

            string updatedName = null;
            int updatedQuantityInPackage = 20;
            string updatedUnitOfMeasurement = "Unity";

            // act
            var response = await UpdateProductAsync(productId, categoryId, updatedName, updatedQuantityInPackage, updatedUnitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Update_Product_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            var createResponse = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var productBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = productBody["id"].AsValue().GetValue<int>();

            string updatedName = "";
            int updatedQuantityInPackage = 20;
            string updatedUnitOfMeasurement = "Unity";

            // act
            var response = await UpdateProductAsync(productId, categoryId, updatedName, updatedQuantityInPackage, updatedUnitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Update_Product_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            var createResponse = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var productBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = productBody["id"].AsValue().GetValue<int>();

            string updatedName = "";
            int updatedQuantityInPackage = 20;
            string updatedUnitOfMeasurement = "Unity";

            // act
            var response = await UpdateProductAsync(productId, categoryId, updatedName, updatedQuantityInPackage, updatedUnitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Update_Product_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            var createResponse = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var productBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = productBody["id"].AsValue().GetValue<int>();

            string updatedName = new string('a', 51);
            int updatedQuantityInPackage = 20;
            string updatedUnitOfMeasurement = "Unity";

            // act
            var response = await UpdateProductAsync(productId, categoryId, updatedName, updatedQuantityInPackage, updatedUnitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Update_Product_When_Name_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            var createResponse = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var productBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = productBody["id"].AsValue().GetValue<int>();

            string updatedName = "a";
            int updatedQuantityInPackage = 20;
            string updatedUnitOfMeasurement = "Unity";

            // act
            var response = await UpdateProductAsync(productId, categoryId, updatedName, updatedQuantityInPackage, updatedUnitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updatedBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(updatedName, updatedBody["name"].AsValue().GetValue<string>());
            Assert.Equal(updatedQuantityInPackage, updatedBody["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal(updatedUnitOfMeasurement, updatedBody["unitOfMeasurement"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC044_Update_Product_When_Name_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            var categoryResponse = await CreateCategoryAsync("Category1");
            var categoryBody = await categoryResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = categoryBody["id"].AsValue().GetValue<int>();
            string name = "ValidProduct";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            var createResponse = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var productBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int productId = productBody["id"].AsValue().GetValue<int>();

            string updatedName = new string('a', 50);
            int updatedQuantityInPackage = 20;
            string updatedUnitOfMeasurement = "Unity";

            // act
            var response = await UpdateProductAsync(productId, categoryId, updatedName, updatedQuantityInPackage, updatedUnitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updatedBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(updatedName, updatedBody["name"].AsValue().GetValue<string>());
            Assert.Equal(updatedQuantityInPackage, updatedBody["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal(updatedUnitOfMeasurement, updatedBody["unitOfMeasurement"].AsValue().GetValue<string>());
        }
    }
}