// File: AccountIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;

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
        public async Task TC001_Create_Account_When_Valid_Data_Returns_OK()
        {
            // arrange
            var user = "user1";

            // act
            var response = await CreateAccountAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_Account_When_Invalid_FirstName_Returns_BadRequest()
        {
            // arrange
            var user = "user2";

            // act
            var response = await CreateAccountAsync(
                firstName: "",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Account_When_Invalid_LastName_Returns_BadRequest()
        {
            // arrange
            var user = "user3";

            // act
            var response = await CreateAccountAsync(
                firstName: $"{user}John",
                lastName: "",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Account_When_Invalid_Email_Returns_BadRequest()
        {
            // arrange
            var user = "user4";

            // act
            var response = await CreateAccountAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: "invalid-email",
                password: "Pass123!");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Account_When_Invalid_Password_Returns_BadRequest()
        {
            // arrange
            var user = "user5";

            // act
            var response = await CreateAccountAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "invalid");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Login_When_Valid_Credentials_Returns_OK()
        {
            // arrange
            var user = "user6";
            var email = $"{user}john.doe@example.com";
            var password = "Pass123!";
            await CreateAccountAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: email,
                password: password);

            // act
            var response = await LoginAsync(email, password);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_userId = body["userId"].AsValue().GetValue<string>();
            var body_token = body["token"].AsValue().GetValue<string>();
            var body_refreshToken = body["refreshToken"].AsValue().GetValue<string>();
            Assert.False(String.IsNullOrEmpty(body_userId));
            Assert.False(String.IsNullOrEmpty(body_token));
            Assert.False(String.IsNullOrEmpty(body_refreshToken));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Login_When_Invalid_Email_Returns_Unauthorized()
        {
            // arrange
            var user = "user7";
            var email = $"{user}john.doe@example.com";
            var password = "Pass123!";
            await CreateAccountAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: email,
                password: password);

            // act
            var response = await LoginAsync("invalid-email", password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Login_When_Invalid_Password_Returns_Unauthorized()
        {
            // arrange
            var user = "user8";
            var email = $"{user}john.doe@example.com";
            var password = "Pass123!";
            await CreateAccountAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: email,
                password: password);

            // act
            var response = await LoginAsync(email, "invalid");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Refresh_Token_When_Valid_Token_Returns_OK()
        {
            // arrange
            var user = "user9";
            var email = $"{user}john.doe@example.com";
            var password = "Pass123!";
            await CreateAccountAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: email,
                password: password);

            var loginResponse = await LoginAsync(email, password);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonObject>();
            var userId = loginBody["userId"].AsValue().GetValue<string>();
            var token = loginBody["token"].AsValue().GetValue<string>();
            var refreshToken = loginBody["refreshToken"].AsValue().GetValue<string>();

            // act
            var response = await RefreshTokenAsync(userId, token, refreshToken);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_userId = body["userId"].AsValue().GetValue<string>();
            var body_token = body["token"].AsValue().GetValue<string>();
            var body_refreshToken = body["refreshToken"].AsValue().GetValue<string>();
            Assert.False(String.IsNullOrEmpty(body_userId));
            Assert.False(String.IsNullOrEmpty(body_token));
            Assert.False(String.IsNullOrEmpty(body_refreshToken));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Refresh_Token_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var user = "user10";
            var email = $"{user}john.doe@example.com";
            var password = "Pass123!";
            await CreateAccountAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: email,
                password: password);

            // act
            var response = await RefreshTokenAsync("user-id", "invalid-token", "valid-refresh-token");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
