using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class AccountIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AccountIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateAccountAsync(string firstName, string lastName, string email, string password, bool isAdmin)
        {
            var request = new
            {
                firstName = firstName,
                lastName = lastName,
                email = email,
                password = password,
                isAdmin = isAdmin
            };
            return await _client.PostAsJsonAsync("/api/accounts", request);
        }

        private async Task<HttpResponseMessage> LoginAsync(string email, string password)
        {
            var request = new
            {
                email = email,
                password = password
            };
            return await _client.PostAsJsonAsync("/api/accounts/tokens", request);
        }

        private async Task<HttpResponseMessage> RefreshTokenAsync(string userId, string token, string refreshToken)
        {
            var request = new
            {
                userId = userId,
                token = token,
                refreshToken = refreshToken
            };
            return await _client.PostAsJsonAsync("/api/accounts/refreshtokens", request);
        }


        [Fact]
        public async Task TC001_Create_Account_With_Valid_Data_Returns_OK()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe1@example.com";
            string password = "Password123!";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_Account_With_Missing_FirstName_Returns_BadRequest()
        {
            // arrange
            string firstName = "";
            string lastName = "Doe";
            string email = "john.doe2@example.com";
            string password = "Password123!";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Account_With_Missing_LastName_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "";
            string email = "john.doe3@example.com";
            string password = "Password123!";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Account_With_Missing_Email_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "";
            string password = "Password123!";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Account_With_Missing_Password_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe5@example.com";
            string password = "";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Account_With_Invalid_Email_Format_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "invalid-email";
            string password = "Password123!";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Account_With_Password_Less_Than_Minimum_Length_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe7@example.com";
            string password = "Pass";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Account_With_Password_Greater_Than_Maximum_Length_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe8@example.com";
            string password = "ThisIsAVeryLongPasswordThatExceedsTheMaximumLengthOfFifteenCharacters";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Account_With_Password_Without_Number_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe9@example.com";
            string password = "Password";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Account_With_Password_Without_Lowercase_Letter_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe10@example.com";
            string password = "PASSWORD123!";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Account_With_Password_Without_Uppercase_Letter_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe11@example.com";
            string password = "password123!";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_Account_With_Password_Without_Non_alphanumeric_Character_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe12@example.com";
            string password = "Password123";
            bool isAdmin = false;

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_Account_With_Duplicate_Email_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            string email = "john.doe13@example.com";
            string password = "Password123!";
            bool isAdmin = false;
            await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // act
            var response = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Login_With_Valid_Credentials_Returns_OK()
        {
            // arrange
            string email = "john.doe14@example.com";
            string password = "Password123!";
            await CreateAccountAsync("John", "Doe", email, password, false);

            // act
            var response = await LoginAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotEmpty(body["userId"].ToString());
            Assert.NotEmpty(body["token"].ToString());
            Assert.NotEmpty(body["refreshToken"].ToString());
        }

        [Fact]
        public async Task TC015_Login_With_Invalid_Credentials_Returns_Unauthorized()
        {
            // arrange
            string email = "wrong.email@example.com";
            string password = "WrongPassword";

            // act
            var response = await LoginAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Login_With_Missing_Email_Returns_BadRequest()
        {
            // arrange
            string email = "";
            string password = "Password123!";

            // act
            var response = await LoginAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Login_With_Missing_Password_Returns_BadRequest()
        {
            // arrange
            string email = "john.doe17@example.com";
            string password = "";

            // act
            var response = await LoginAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Refresh_Token_With_Valid_Token_Returns_OK()
        {
            // arrange
            string email = "john.doe18@example.com";
            string password = "Password123!";
            await CreateAccountAsync("John", "Doe", email, password, false);
            var loginResponse = await LoginAsync(email, password);
            var body = await loginResponse.Content.ReadFromJsonAsync<JsonObject>();
            string userId = body["userId"].GetValue<string>();
            string token = body["token"].GetValue<string>();
            string refreshToken = body["refreshToken"].GetValue<string>();

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var refreshedBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotEmpty(refreshedBody["userId"].ToString());
            Assert.NotEmpty(refreshedBody["token"].ToString());
            Assert.NotEmpty(refreshedBody["refreshToken"].ToString());

        }

        [Fact]
        public async Task TC019_Refresh_Token_With_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            string userId = "invalidUserId";
            string token = "invalidToken";
            string refreshToken = "invalidRefreshToken";

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Refresh_Token_With_Missing_Token_Returns_BadRequest()
        {
            // arrange
            string userId = "";
            string token = "";
            string refreshToken = "";

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
