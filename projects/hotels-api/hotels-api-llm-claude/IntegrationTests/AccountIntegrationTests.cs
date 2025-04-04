using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
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

        private async Task<string> CreateUserAndGetTokenAsync(string firstName, string lastName, string email, string password, bool isAdmin)
        {
            var request = new
            {
                firstName = firstName,
                lastName = lastName,
                email = email,
                password = password,
                isAdmin = isAdmin
            };

            await _client.PostAsJsonAsync("/api/accounts", request);

            var loginRequest = new
            {
                email = email,
                password = password
            };

            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].AsValue().GetValue<string>();
        }

        [Fact]
        public async Task TC001_Create_User_When_Valid_Data_Returns_OK()
        {
            // arrange
            var request = new
            {
                firstName = "John1",
                lastName = "Doe1",
                email = "john1@test.com",
                password = "Test@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_When_Empty_FirstName_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "",
                lastName = "Doe2",
                email = "john2@test.com",
                password = "Test@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_When_Null_FirstName_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = (string)null,
                lastName = "Doe3",
                email = "john3@test.com",
                password = "Test@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_When_Empty_LastName_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John4",
                lastName = "",
                email = "john4@test.com",
                password = "Test@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_When_Null_LastName_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John5",
                lastName = (string)null,
                email = "john5@test.com",
                password = "Test@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_User_When_Invalid_Email_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John6",
                lastName = "Doe6",
                email = "invalid_email",
                password = "Test@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_User_When_Empty_Email_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John7",
                lastName = "Doe7",
                email = "",
                password = "Test@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_User_When_Null_Email_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John8",
                lastName = "Doe8",
                email = (string)null,
                password = "Test@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_User_When_Password_Too_Short_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John9",
                lastName = "Doe9",
                email = "john9@test.com",
                password = "T@1a", // 4 chars - below minimum of 6
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_User_When_Password_At_Minimum_Length_Returns_OK()
        {
            // arrange
            var request = new
            {
                firstName = "John10",
                lastName = "Doe10",
                email = "john10@test.com",
                password = "Test@1", // 6 chars - minimum
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_User_When_Password_At_Maximum_Length_Returns_OK()
        {
            // arrange
            var request = new
            {
                firstName = "John11",
                lastName = "Doe11",
                email = "john11@test.com",
                password = "Test@1234567890", // 15 chars - maximum
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_User_When_Password_Too_Long_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John12",
                lastName = "Doe12",
                email = "john12@test.com",
                password = "Test@123456789012", // 16 chars - above maximum of 15
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_User_When_Password_Missing_Number_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John13",
                lastName = "Doe13",
                email = "john13@test.com",
                password = "Test@abc",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_User_When_Password_Missing_Uppercase_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John14",
                lastName = "Doe14",
                email = "john14@test.com",
                password = "test@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_User_When_Password_Missing_Lowercase_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John15",
                lastName = "Doe15",
                email = "john15@test.com",
                password = "TEST@123",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Create_User_When_Password_Missing_Special_Character_Returns_BadRequest()
        {
            // arrange
            var request = new
            {
                firstName = "John16",
                lastName = "Doe16",
                email = "john16@test.com",
                password = "Test1234",
                isAdmin = false
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Login_When_Valid_Credentials_Returns_OK()
        {
            // arrange
            var email = "john17@test.com";
            var password = "Test@123";

            await _client.PostAsJsonAsync("/api/accounts", new
            {
                firstName = "John17",
                lastName = "Doe17",
                email = email,
                password = password,
                isAdmin = false
            });

            var loginRequest = new
            {
                email = email,
                password = password
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body["userId"].AsValue().GetValue<string>());
            Assert.NotNull(body["token"].AsValue().GetValue<string>());
            Assert.NotNull(body["refreshToken"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC018_Login_When_Invalid_Email_Returns_BadRequest()
        {
            // arrange
            var loginRequest = new
            {
                email = "invalid_email",
                password = "Test@123"
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Login_When_Empty_Email_Returns_BadRequest()
        {
            // arrange
            var loginRequest = new
            {
                email = "",
                password = "Test@123"
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Login_When_Empty_Password_Returns_BadRequest()
        {
            // arrange
            var loginRequest = new
            {
                email = "john20@test.com",
                password = ""
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Login_When_Wrong_Password_Returns_Unauthorized()
        {
            // arrange
            var email = "john21@test.com";

            await _client.PostAsJsonAsync("/api/accounts", new
            {
                firstName = "John21",
                lastName = "Doe21",
                email = email,
                password = "Test@123",
                isAdmin = false
            });

            var loginRequest = new
            {
                email = email,
                password = "WrongPass@123"
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Login_When_User_Not_Exists_Returns_Unauthorized()
        {
            // arrange
            var loginRequest = new
            {
                email = "nonexistent@test.com",
                password = "Test@123"
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Refresh_Token_When_Valid_Token_Returns_OK()
        {
            // arrange
            var email = "john23@test.com";
            var password = "Test@123";

            await _client.PostAsJsonAsync("/api/accounts", new
            {
                firstName = "John23",
                lastName = "Doe23",
                email = email,
                password = password,
                isAdmin = false
            });

            var loginResponse = await _client.PostAsJsonAsync("/api/accounts/tokens", new
            {
                email = email,
                password = password
            });

            var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonObject>();
            var refreshRequest = new
            {
                userId = loginBody["userId"].AsValue().GetValue<string>(),
                token = loginBody["token"].AsValue().GetValue<string>(),
                refreshToken = loginBody["refreshToken"].AsValue().GetValue<string>()
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/refreshtokens", refreshRequest);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body["userId"].AsValue().GetValue<string>());
            Assert.NotNull(body["token"].AsValue().GetValue<string>());
            Assert.NotNull(body["refreshToken"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC024_Refresh_Token_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var refreshRequest = new
            {
                userId = "some-user-id",
                token = "invalid-token",
                refreshToken = "invalid-refresh-token"
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/accounts/refreshtokens", refreshRequest);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
