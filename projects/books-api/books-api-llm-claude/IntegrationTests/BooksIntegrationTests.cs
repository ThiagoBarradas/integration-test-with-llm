using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Xunit.Extensions.Ordering;

namespace IntegrationTests
{
    public class BooksIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BooksIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetAllBooksAsync()
        {
            return await _client.GetAsync("/books");
        }

        private async Task<HttpResponseMessage> CreateBookAsync(string title, string isbn, string description, string author)
        {
            var request = new
            {
                title = title,
                isbn = isbn,
                description = description,
                author = author
            };

            return await _client.PostAsJsonAsync("/books", request);
        }

        private async Task<int> CreateBookAndGetIdAsync(string title, string isbn, string description, string author)
        {
            var response = await CreateBookAsync(title, isbn, description, author);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> GetBookByIdAsync(int id)
        {
            return await _client.GetAsync($"/books/{id}");
        }

        private async Task<HttpResponseMessage> UpdateBookAsync(int id, string title, string isbn, string description, string author)
        {
            var request = new
            {
                title = title,
                isbn = isbn,
                description = description,
                author = author
            };

            return await _client.PutAsJsonAsync($"/books/{id}", request);
        }

        private async Task<HttpResponseMessage> DeleteBookAsync(int id)
        {
            return await _client.DeleteAsync($"/books/{id}");
        }

        [Fact]
        public async Task TC001_Get_All_Books_When_Empty_Returns_Empty_Array()
        {
            // act
            var response = await GetAllBooksAsync();

            // assert
            var books = await response.Content.ReadFromJsonAsync<Book[]>();
            Assert.Empty(books);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Get_All_Books_When_Has_Books_Returns_Books_Array()
        {
            // arrange
            var title1 = "Book 1";
            var isbn1 = "123";
            var description1 = "Description 1";
            var author1 = "Author 1";
            var title2 = "Book 2";
            var isbn2 = "456";
            var description2 = "Description 2";
            var author2 = "Author 2";

            await CreateBookAsync(title1, isbn1, description1, author1);
            await CreateBookAsync(title2, isbn2, description2, author2);

            // act
            var response = await GetAllBooksAsync();

            // assert
            var books = await response.Content.ReadFromJsonAsync<Book[]>();
            Assert.Equal(2, books.Length);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.Equal(title1, books[0].Title);
            Assert.Equal(isbn1, books[0].ISBN);
            Assert.Equal(description1, books[0].Description);
            Assert.Equal(author1, books[0].Author);

            Assert.Equal(title2, books[1].Title);
            Assert.Equal(isbn2, books[1].ISBN);
            Assert.Equal(description2, books[1].Description);
            Assert.Equal(author2, books[1].Author);
        }

        [Fact]
        public async Task TC003_Create_Book_When_Valid_Data_Returns_Created_Book()
        {
            // arrange
            var title = "Valid Book";
            var isbn = "123-456";
            var description = "Valid Description";
            var author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            var book = await response.Content.ReadFromJsonAsync<Book>();
            Assert.True(book.Id > 0);
            Assert.Equal(title, book.Title);
            Assert.Equal(isbn, book.ISBN);
            Assert.Equal(description, book.Description);
            Assert.Equal(author, book.Author);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Book_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            string title = null;
            var isbn = "123-456";
            var description = "Valid Description";
            var author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Book_When_Title_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var title = "";
            var isbn = "123-456";
            var description = "Valid Description";
            var author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Book_When_ISBN_Is_Null_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Book";
            string isbn = null;
            var description = "Valid Description";
            var author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Book_When_ISBN_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Book";
            var isbn = "";
            var description = "Valid Description";
            var author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Book_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Book";
            var isbn = "123-456";
            string description = null;
            var author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Book_When_Description_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Book";
            var isbn = "123-456";
            var description = "";
            var author = "Valid Author";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Book_When_Author_Is_Null_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Book";
            var isbn = "123-456";
            var description = "Valid Description";
            string author = null;

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Book_When_Author_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var title = "Valid Book";
            var isbn = "123-456";
            var description = "Valid Description";
            var author = "";

            // act
            var response = await CreateBookAsync(title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Get_Book_By_Id_When_Book_Exists_Returns_Book()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            // act
            var response = await GetBookByIdAsync(id);

            // assert
            var book = await response.Content.ReadFromJsonAsync<Book>();
            Assert.Equal(id, book.Id);
            Assert.Equal(title, book.Title);
            Assert.Equal(isbn, book.ISBN);
            Assert.Equal(description, book.Description);
            Assert.Equal(author, book.Author);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Get_Book_By_Id_When_Book_Not_Exists_Returns_NotFound()
        {
            // arrange
            var nonExistentId = 999;

            // act
            var response = await GetBookByIdAsync(nonExistentId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Update_Book_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            var updatedTitle = "Updated Book";
            var updatedIsbn = "789-012";
            var updatedDescription = "Updated Description";
            var updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(id, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // verify update
            var getResponse = await GetBookByIdAsync(id);
            var updatedBook = await getResponse.Content.ReadFromJsonAsync<Book>();
            Assert.Equal(updatedTitle, updatedBook.Title);
            Assert.Equal(updatedIsbn, updatedBook.ISBN);
            Assert.Equal(updatedDescription, updatedBook.Description);
            Assert.Equal(updatedAuthor, updatedBook.Author);
        }

        [Fact]
        public async Task TC015_Update_Book_When_Book_Not_Exists_Returns_NotFound()
        {
            // arrange
            var nonExistentId = 999;
            var title = "Updated Book";
            var isbn = "789-012";
            var description = "Updated Description";
            var author = "Updated Author";

            // act
            var response = await UpdateBookAsync(nonExistentId, title, isbn, description, author);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Update_Book_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            string updatedTitle = null;
            var updatedIsbn = "789-012";
            var updatedDescription = "Updated Description";
            var updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(id, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Update_Book_When_Title_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            var updatedTitle = "";
            var updatedIsbn = "789-012";
            var updatedDescription = "Updated Description";
            var updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(id, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Update_Book_When_ISBN_Is_Null_Returns_BadRequest()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            var updatedTitle = "Updated Book";
            string updatedIsbn = null;
            var updatedDescription = "Updated Description";
            var updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(id, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Update_Book_When_ISBN_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            var updatedTitle = "Updated Book";
            var updatedIsbn = "";
            var updatedDescription = "Updated Description";
            var updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(id, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Update_Book_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            var updatedTitle = "Updated Book";
            var updatedIsbn = "789-012";
            string updatedDescription = null;
            var updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(id, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Update_Book_When_Description_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            var updatedTitle = "Updated Book";
            var updatedIsbn = "789-012";
            var updatedDescription = "";
            var updatedAuthor = "Updated Author";

            // act
            var response = await UpdateBookAsync(id, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Update_Book_When_Author_Is_Null_Returns_BadRequest()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            var updatedTitle = "Updated Book";
            var updatedIsbn = "789-012";
            var updatedDescription = "Updated Description";
            string updatedAuthor = null;

            // act
            var response = await UpdateBookAsync(id, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Update_Book_When_Author_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            var updatedTitle = "Updated Book";
            var updatedIsbn = "789-012";
            var updatedDescription = "Updated Description";
            var updatedAuthor = "";

            // act
            var response = await UpdateBookAsync(id, updatedTitle, updatedIsbn, updatedDescription, updatedAuthor);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Delete_Book_When_Book_Exists_Returns_NoContent()
        {
            // arrange
            var title = "Book 1";
            var isbn = "123";
            var description = "Description 1";
            var author = "Author 1";
            var id = await CreateBookAndGetIdAsync(title, isbn, description, author);

            // act
            var response = await DeleteBookAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // verify deletion
            var getResponse = await GetBookByIdAsync(id);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task TC025_Delete_Book_When_Book_Not_Exists_Returns_NotFound()
        {
            // arrange
            var nonExistentId = 999;

            // act
            var response = await DeleteBookAsync(nonExistentId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
    }
}
