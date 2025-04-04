// File: BookIntegrationTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Net.Http;
using System.Text;

namespace IntegrationTests
{
    public class BookIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BookIntegrationTests(WebApplicationFactory<Program> factory)
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
            var book = new
            {
                title = title,
                isbn = isbn,
                description = description,
                author = author
            };
            return await _client.PostAsJsonAsync("/books", book);
        }

        private async Task<int> CreateBookAndGetIdAsync(string title, string isbn, string description, string author)
        {
            var response = await CreateBookAsync(title, isbn, description, author);
            var book = await response.Content.ReadFromJsonAsync<JsonObject>();
            return book["id"].AsValue().GetValue<int>();
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

        private async Task<HttpResponseMessage> DeleteBookByIdAsync(int id)
        {
            return await _client.DeleteAsync($"/books/{id}");
        }


        [Fact]
        public async Task TC001_Get_All_Books_Returns_200_OK()
        {
            // Arrange

            // Act
            var response = await GetAllBooksAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_Book_With_Valid_Data_Returns_200_OK()
        {
            // Arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync(title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Book_With_Missing_Title_Returns_400_BadRequest()
        {
            // Arrange
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync("", isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Book_With_Missing_Isbn_Returns_400_BadRequest()
        {
            // Arrange
            string title = "Valid Title";
            string description = "Valid Description";
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync(title, "", description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Book_With_Missing_Description_Returns_400_BadRequest()
        {
            // Arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync(title, isbn, "", author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Book_With_Missing_Author_Returns_400_BadRequest()
        {
            // Arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "Valid Description";

            // Act
            var response = await CreateBookAsync(title, isbn, description, "");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Book_With_Empty_Title_Returns_400_BadRequest()
        {
            // Arrange
            string title = "";
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync(title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Book_With_Empty_Isbn_Returns_400_BadRequest()
        {
            // Arrange
            string title = "Valid Title";
            string isbn = "";
            string description = "Valid Description";
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync(title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Book_With_Empty_Description_Returns_400_BadRequest()
        {
            // Arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "";
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync(title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Book_With_Empty_Author_Returns_400_BadRequest()
        {
            // Arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "";

            // Act
            var response = await CreateBookAsync(title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Get_Book_By_Id_With_Valid_Id_Returns_200_OK()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");

            // Act
            var response = await GetBookByIdAsync(id);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Get_Book_By_Id_With_Invalid_Id_Returns_404_NotFound()
        {
            // Arrange
            int id = 99999;

            // Act
            var response = await GetBookByIdAsync(id);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Update_Book_With_Valid_Data_Returns_204_NoContent()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string isbn = "Updated ISBN";
            string description = "Updated Description";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Update_Book_With_Invalid_Id_Returns_404_NotFound()
        {
            // Arrange
            int id = 99999;
            string title = "Updated Title";
            string isbn = "Updated ISBN";
            string description = "Updated Description";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Update_Book_With_Missing_Title_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string isbn = "Updated ISBN";
            string description = "Updated Description";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, "", isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Update_Book_With_Missing_Isbn_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string description = "Updated Description";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, "", description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Update_Book_With_Missing_Description_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string isbn = "Updated ISBN";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, "", author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Update_Book_With_Missing_Author_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string isbn = "Updated ISBN";
            string description = "Updated Description";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, "");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Update_Book_With_Empty_Title_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "";
            string isbn = "Updated ISBN";
            string description = "Updated Description";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Update_Book_With_Empty_Isbn_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string isbn = "";
            string description = "Updated Description";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Update_Book_With_Empty_Description_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string isbn = "Updated ISBN";
            string description = "";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Update_Book_With_Empty_Author_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string isbn = "Updated ISBN";
            string description = "Updated Description";
            string author = "";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Delete_Book_With_Valid_Id_Returns_204_NoContent()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");

            // Act
            var response = await DeleteBookByIdAsync(id);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Delete_Book_With_Invalid_Id_Returns_404_NotFound()
        {
            // Arrange
            int id = 99999;

            // Act
            var response = await DeleteBookByIdAsync(id);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Book_With_Null_Title_Returns_400_BadRequest()
        {
            // Arrange
            string title = null;
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync(title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Create_Book_With_Null_Isbn_Returns_400_BadRequest()
        {
            // Arrange
            string title = "Valid Title";
            string isbn = null;
            string description = "Valid Description";
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync(title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Create_Book_With_Null_Description_Returns_400_BadRequest()
        {
            // Arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = null;
            string author = "Valid Author";

            // Act
            var response = await CreateBookAsync(title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Create_Book_With_Null_Author_Returns_400_BadRequest()
        {
            // Arrange
            string title = "Valid Title";
            string isbn = "Valid ISBN";
            string description = "Valid Description";
            string author = null;

            // Act
            var response = await CreateBookAsync(title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Update_Book_With_Null_Title_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = null;
            string isbn = "Updated ISBN";
            string description = "Updated Description";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Update_Book_With_Null_Isbn_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string isbn = null;
            string description = "Updated Description";
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Update_Book_With_Null_Description_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string isbn = "Updated ISBN";
            string description = null;
            string author = "Updated Author";

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Update_Book_With_Null_Author_Returns_400_BadRequest()
        {
            // Arrange
            int id = await CreateBookAndGetIdAsync("Title 1", "ISBN 1", "Description 1", "Author 1");
            string title = "Updated Title";
            string isbn = "Updated ISBN";
            string description = "Updated Description";
            string author = null;

            // Act
            var response = await UpdateBookAsync(id, title, isbn, description, author);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}