// File: AccountIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

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

        private async Task<HttpResponseMessage> CreateAccountAsync(string firstName, string lastName, bool isAdmin, string email, string password)
        {
            var request = new
            {
                firstName = firstName,
                lastName = lastName,
                isAdmin = isAdmin,
                email = email,
                password = password
            };

            return await _client.PostAsJsonAsync("/api/accounts", request);
        }

        private async Task<HttpResponseMessage> GenerateTokenAsync(string email, string password)
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
        public async Task TC001_Create_Account_When_Valid_Data_Returns_OK()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.001@example.com";
            string password = "Password1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_Account_When_First_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = null;
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.002@example.com";
            string password = "Password1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Account_When_First_Name_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.003@example.com";
            string password = "Password1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Account_When_Last_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = null;
            bool isAdmin = true;
            string email = "john.doe.004@example.com";
            string password = "Password1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Account_When_Last_Name_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "";
            bool isAdmin = true;
            string email = "john.doe.005@example.com";
            string password = "Password1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Account_When_Email_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = null;
            string password = "Password1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Account_When_Email_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "";
            string password = "Password1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Account_When_Email_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "invalid-email";
            string password = "Password1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Account_When_Password_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.009@example.com";
            string password = null;

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Account_When_Password_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.010@example.com";
            string password = "";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Account_When_Password_Too_Short_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.011@example.com";
            string password = "Pass1";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_Account_When_Password_Too_Long_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.012@example.com";
            string password = "Password1!Extra!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_Account_When_Password_Has_Minimum_Length_Returns_OK()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.013@example.com";
            string password = "Pass1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_Account_When_Password_Has_Maximum_Length_Returns_OK()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.014@example.com";
            string password = "Password1!Extra";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_Account_When_Password_Missing_Number_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.015@example.com";
            string password = "Password!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Create_Account_When_Password_Missing_Lowercase_Letter_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.016@example.com";
            string password = "PASSWORD1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_Account_When_Password_Missing_Uppercase_Letter_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.017@example.com";
            string password = "password1!";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_Account_When_Password_Missing_Non_Alphanumeric_Character_Returns_BadRequest()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.018@example.com";
            string password = "Password1";

            // act
            var response = await CreateAccountAsync(firstName, lastName, isAdmin, email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Generate_Token_When_Valid_Data_Returns_OK()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.019@example.com";
            string password = "Password1!";
            await CreateAccountAsync(firstName, lastName, isAdmin, email, password); // precondition 1

            // act
            var response = await GenerateTokenAsync(email, password);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_userId = body["userId"].AsValue().GetValue<string>();
            var body_token = body["token"].AsValue().GetValue<string>();
            var body_refreshToken = body["refreshToken"].AsValue().GetValue<string>();
            Assert.NotNull(body_userId);
            Assert.NotNull(body_token);
            Assert.NotNull(body_refreshToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Generate_Token_When_Email_Is_Null_Returns_BadRequest()
        {
            // arrange
            string email = null;
            string password = "Password1!";

            // act
            var response = await GenerateTokenAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Generate_Token_When_Email_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string email = "";
            string password = "Password1!";

            // act
            var response = await GenerateTokenAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Generate_Token_When_Email_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            string email = "invalid-email";
            string password = "Password1!";

            // act
            var response = await GenerateTokenAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Generate_Token_When_Password_Is_Null_Returns_BadRequest()
        {
            // arrange
            string email = "john.doe.020@example.com";
            string password = null;

            // act
            var response = await GenerateTokenAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Generate_Token_When_Password_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string email = "john.doe.021@example.com";
            string password = "";

            // act
            var response = await GenerateTokenAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Generate_Token_When_User_Does_Not_Exist_Returns_Unauthorized()
        {
            // arrange
            string email = "nonexistent.user.025@example.com";
            string password = "Password1!";

            // act
            var response = await GenerateTokenAsync(email, password);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Generate_Token_When_Password_Is_Incorrect_Returns_Unauthorized()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.026@example.com";
            string password = "Password1!";
            await CreateAccountAsync(firstName, lastName, isAdmin, email, password); // precondition 1

            // act
            var response = await GenerateTokenAsync(email, "WrongPassword1!");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Refresh_Token_When_Valid_Data_Returns_OK()
        {
            // arrange
            string firstName = "John";
            string lastName = "Doe";
            bool isAdmin = true;
            string email = "john.doe.027@example.com";
            string password = "Password1!";
            await CreateAccountAsync(firstName, lastName, isAdmin, email, password); // precondition 1
            var tokenResponse = await GenerateTokenAsync(email, password);
            var tokenBody = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>();
            string userId = tokenBody["userId"].AsValue().GetValue<string>();
            string token = tokenBody["token"].AsValue().GetValue<string>();
            string refreshToken = tokenBody["refreshToken"].AsValue().GetValue<string>();

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_userId = body["userId"].AsValue().GetValue<string>();
            var body_token = body["token"].AsValue().GetValue<string>();
            var body_refreshToken = body["refreshToken"].AsValue().GetValue<string>();
            Assert.NotNull(body_userId);
            Assert.NotNull(body_token);
            Assert.NotNull(body_refreshToken);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Refresh_Token_When_User_Id_Is_Null_Returns_BadRequest()
        {
            // arrange
            string userId = null;
            string token = "valid-token";
            string refreshToken = "valid-refresh-token";

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Refresh_Token_When_Token_Is_Null_Returns_BadRequest()
        {
            // arrange
            string userId = "valid-user-id";
            string token = null;
            string refreshToken = "valid-refresh-token";

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Refresh_Token_When_Refresh_Token_Is_Null_Returns_BadRequest()
        {
            // arrange
            string userId = "valid-user-id";
            string token = "valid-token";
            string refreshToken = null;

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Refresh_Token_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string userId = "valid-user-id";
            string token = "invalid-token";
            string refreshToken = "valid-refresh-token";

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Refresh_Token_When_Refresh_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string userId = "valid-user-id";
            string token = "valid-token";
            string refreshToken = "invalid-refresh-token";

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
