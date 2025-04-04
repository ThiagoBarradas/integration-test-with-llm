// File: AccountIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;

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

        private async Task<HttpResponseMessage> CreateUserAsync(string firstName, string lastName, string email, string password, bool isAdmin)
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

        private async Task<JsonDocument> AuthenticateUserAsync(string email, string password)
        {
            var request = new
            {
                email = email,
                password = password
            };
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", request);
            return await response.Content.ReadFromJsonAsync<JsonDocument>();
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

        [Theory]
        [InlineData("John", "Doe", "john.doe@example.com", "P@ssw0rd", false)]
        public async Task TC001_Create_User_With_Valid_Data_Returns_OK(string firstName, string lastName, string email, string password, bool isAdmin)
        {
            // act
            var response = await CreateUserAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("Jane", "Doe", "jane.doeexample.com", "P@ssw0rd", false)]
        public async Task TC002_Create_User_With_Invalid_Email_Returns_BadRequest(string firstName, string lastName, string email, string password, bool isAdmin)
        {
            // act
            var response = await CreateUserAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("Alice", "Smith", "alice.smith@example.com", "Password1234567890", false)]
        public async Task TC003_Create_User_With_Too_Long_Password_Returns_BadRequest(string firstName, string lastName, string email, string password, bool isAdmin)
        {
            // act
            var response = await CreateUserAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("Bob", "Johnson", "bob.johnson@example.com", "pass123", false)] // Missing upper case letter
        public async Task TC004_Create_User_With_Password_Missing_Requirement_Returns_BadRequest(string firstName, string lastName, string email, string password, bool isAdmin)
        {
            // act
            var response = await CreateUserAsync(firstName, lastName, email, password, isAdmin);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("valid.user@example.com", "V@lidP4ssw0rd")]
        public async Task TC005_Authenticate_User_With_Valid_Credentials_Returns_OK(string email, string password)
        {
            // arrange
            await CreateUserAsync("Some", "User", email, password, false);

            // act
            var jsonDocument = await AuthenticateUserAsync(email, password);

            // assert
            Assert.NotNull(jsonDocument);
            Assert.NotNull(jsonDocument.RootElement.GetProperty("token").ToString());
            Assert.NotNull(jsonDocument.RootElement.GetProperty("userId").ToString());
            Assert.NotNull(jsonDocument.RootElement.GetProperty("refreshToken").ToString());
        }

        [Theory]
        [InlineData("invalid.user@example.com", "InvalidP@ss")]
        public async Task TC006_Authenticate_User_With_Invalid_Credentials_Returns_Unauthorized(string email, string password)
        {
            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", new { email = email, password = password });

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("valid.user@example.com", "V@lidP4ssw0rd")]
        public async Task TC007_Refresh_Token_With_Valid_Credentials_Returns_OK(string email, string password)
        {
            // arrange
            await CreateUserAsync("Some", "User", email, password, false);
            var authenticateResponse = await AuthenticateUserAsync("valid.user@example.com", "V@lidP4ssw0rd");
            var userId = authenticateResponse.RootElement.GetProperty("userId").ToString();
            var token = authenticateResponse.RootElement.GetProperty("token").ToString();
            var refreshToken = authenticateResponse.RootElement.GetProperty("refreshToken").ToString();

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            var jsonDocument = await response.Content.ReadFromJsonAsync<JsonDocument>();
            Assert.NotNull(jsonDocument);
            Assert.NotNull(jsonDocument.RootElement.GetProperty("token").ToString());
            Assert.NotNull(jsonDocument.RootElement.GetProperty("userId").ToString());
            Assert.NotNull(jsonDocument.RootElement.GetProperty("refreshToken").ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("invalid_user_id", "invalid_token", "invalid_refresh_token")]
        public async Task TC008_Refresh_Token_With_Invalid_Credentials_Returns_Unauthorized(string invalidUserId, string invalidToken, string invalidRefreshToken)
        {
            // act
            var response = await RefreshTokenAsync(invalidUserId, invalidToken, invalidRefreshToken);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
