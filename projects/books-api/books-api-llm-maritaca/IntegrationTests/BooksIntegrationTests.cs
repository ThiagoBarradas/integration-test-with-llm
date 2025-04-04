// File: BooksIntegrationTests.cs

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using IntegrationTests; // Make sure this is the correct namespace
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace IntegrationTests
{
    public class BooksIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public BooksIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetAllBooksAsync()
        {
            return await _client.GetAsync("/books");
        }

        private async Task<HttpResponseMessage> CreateBookAsync(string title, string isbn, string description, string author)
        {
            var book = new
            {
                title = title,
                isbn = isbn,
                description = description,
                author = author
            };

            return await _client.PostAsJsonAsync("/books", book);
        }

        private async Task<HttpResponseMessage> GetBookByIdAsync(int id)
        {
            return await _client.GetAsync($"/books/{id}");
        }

        private async Task<HttpResponseMessage> UpdateBookAsync(int id, string title, string isbn, string description, string author)
        {
            var book = new
            {
                title = title,
                isbn = isbn,
                description = description,
                author = author
            };

            return await _client.PutAsJsonAsync($"/books/{id}", book);
        }

        private async Task<HttpResponseMessage> DeleteBookAsync(int id)
        {
            return await _client.DeleteAsync($"/books/{id}");
        }

        [Fact]
        public async Task TC001_GetAllBooks_WhenNoFilter_ReturnsOK()
        {
            // act
            var response = await GetAllBooksAsync();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(content);
            Assert.False(content.Any());
        }

        [Fact]
        public async Task TC002_CreateBook_WithValidData_ReturnsOK()
        {
            // arrange
            var title = "Sample Title";
            var isbn = "123-456789-1230";
            var description = "Sample Description";
            var author = "Sample Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(content);
            Assert.Equal(title, content["title"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC003_CreateBook_WithMissingRequiredField_ReturnsBadRequest()
        {
            // arrange
            var isbn = "123-456789-1230";
            var description = "Sample Description";
            var author = "Sample Author";

            // act
            var response = await CreateBookAsync(null, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_CreateBook_WithEmptyStringForTitle_ReturnsBadRequest()
        {
            // arrange
            var title = "";
            var isbn = "123-456789-1230";
            var description = "Sample Description";
            var author = "Sample Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_GetBookById_WithValidId_ReturnsOK()
        {
            // arrange
            var title = "Sample Title";
            var isbn = "123-456789-1230";
            var description = "Sample Description";
            var author = "Sample Author";
            var createResponse = await CreateBookAsync(title, isbn, description, author);
            var content = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var bookId = content["id"].AsValue().GetValue<int>();

            // act
            var getResponse = await GetBookByIdAsync(bookId);

            // assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getBookContent = await getResponse.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(bookId, getBookContent["id"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC006_GetBookById_WithInvalidId_ReturnsNotFound()
        {
            // act
            var response = await GetBookByIdAsync(999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC007_UpdateBook_WithValidData_ReturnsNoContent()
        {
            // arrange
            var title = "Sample Title";
            var isbn = "123-456789-1230";
            var description = "Sample Description";
            var author = "Sample Author";
            var createResponse = await CreateBookAsync(title, isbn, description, author);
            var content = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var bookId = content["id"].AsValue().GetValue<int>();

            var newTitle = "Updated Title";
            var newIsbn = "123-456789-1231";
            var newDescription = "Updated Description";
            var newAuthor = "Updated Author";

            // act
            var updateResponse = await UpdateBookAsync(bookId, newTitle, newIsbn, newDescription, newAuthor);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        }

        [Fact]
        public async Task TC008_UpdateBook_WithMissingRequiredField_ReturnsBadRequest()
        {
            // arrange
            var isbn = "123-456789-1230";
            var description = "Sample Description";
            var author = "Sample Author";
            var createResponse = await CreateBookAsync("Sample Title", isbn, description, author);
            var content = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var bookId = content["id"].AsValue().GetValue<int>();

            // act
            var updateResponse = await UpdateBookAsync(bookId, null, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        }

        [Fact]
        public async Task TC009_UpdateBook_WithNonexistentId_ReturnsNotFound()
        {
            // arrange
            var title = "Updated Title";
            var isbn = "123-456789-1231";
            var description = "Updated Description";
            var author = "Updated Author";

            // act
            var response = await UpdateBookAsync(999999, title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC010_DeleteBookById_WithValidId_ReturnsNoContent()
        {
            // arrange
            var title = "Sample Title";
            var isbn = "123-456789-1230";
            var description = "Sample Description";
            var author = "Sample Author";
            var createResponse = await CreateBookAsync(title, isbn, description, author);
            var content = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var bookId = content["id"].AsValue().GetValue<int>();

            // act
            var deleteResponse = await DeleteBookAsync(bookId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task TC011_DeleteBookById_WithInvalidId_ReturnsNotFound()
        {
            // act
            var response = await DeleteBookAsync(999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}