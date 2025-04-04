﻿// File: UrlShortenEndpointIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class UrlShortenEndpointIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UrlShortenEndpointIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetUrlAsync(string token)
        {
            return await _client.GetAsync($"/{token}");
        }

        private async Task<HttpResponseMessage> CreateShortUrlAsync(string url, int? expireMinutes, bool? hasQrCode)
        {
            var request = new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            };

            return await _client.PostAsJsonAsync("/api/url-shorts", request);
        }

        [Fact]
        public async Task TC001_Get_Url_When_Token_Exists_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            int expireMinutes = 60;
            bool hasQrCode = true;

            // act
            var createResponse = await CreateShortUrlAsync(url, expireMinutes, hasQrCode);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            string token = createBody["token"].AsValue().GetValue<string>();

            var getResponse = await GetUrlAsync(token);

            // assert
            var getBody = await getResponse.Content.ReadFromJsonAsync<JsonObject>();
            var getUrl = getBody["url"].AsValue().GetValue<string>();
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.Equal(url, getUrl);
        }

        [Fact]
        public async Task TC002_Get_Url_When_Token_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            string nonExistingToken = "nonExistingToken";

            // act
            var response = await GetUrlAsync(nonExistingToken);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var message = body["message"].AsValue().GetValue<string>();
            var statusCode = body["statusCode"].AsValue().GetValue<int>();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(404, statusCode);
            Assert.NotNull(message);
        }

        [Fact]
        public async Task TC003_Get_Url_When_Token_Is_Null_Returns_NotFound()
        {
            // arrange
            string nullToken = null;

            // act
            var response = await GetUrlAsync(nullToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Get_Url_When_Token_Is_Empty_String_Returns_NotFound()
        {
            // arrange
            string emptyToken = "";

            // act
            var response = await GetUrlAsync(emptyToken);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Short_Url_When_Valid_Data_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            int expireMinutes = 60;
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            var shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            var qrCode = body["qrCode"].AsValue().GetValue<string>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(token);
            Assert.NotNull(shortenedUrl);
            Assert.NotNull(qrCode);
        }

        [Fact]
        public async Task TC006_Create_Short_Url_When_Url_Is_Null_Returns_BadRequest()
        {
            // arrange
            string url = null;
            int expireMinutes = 60;
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var message = body["message"].AsValue().GetValue<string>();
            var statusCode = body["statusCode"].AsValue().GetValue<int>();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, statusCode);
            Assert.NotNull(message);
        }

        [Fact]
        public async Task TC007_Create_Short_Url_When_Url_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string url = "";
            int expireMinutes = 60;
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var message = body["message"].AsValue().GetValue<string>();
            var statusCode = body["statusCode"].AsValue().GetValue<int>();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, statusCode);
            Assert.NotNull(message);
        }

        [Fact]
        public async Task TC008_Create_Short_Url_When_Expire_Minutes_Is_Null_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            int? expireMinutes = null;
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            var shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            var qrCode = body["qrCode"].AsValue().GetValue<string>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(token);
            Assert.NotNull(shortenedUrl);
            Assert.NotNull(qrCode);
        }

        [Fact]
        public async Task TC009_Create_Short_Url_When_Expire_Minutes_Is_Zero_Returns_BadRequest()
        {
            // arrange
            string url = "http://example.com";
            int expireMinutes = 0;
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var message = body["message"].AsValue().GetValue<string>();
            var statusCode = body["statusCode"].AsValue().GetValue<int>();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, statusCode);
            Assert.NotNull(message);
        }

        [Fact]
        public async Task TC010_Create_Short_Url_When_Expire_Minutes_Is_Negative_Returns_BadRequest()
        {
            // arrange
            string url = "http://example.com";
            int expireMinutes = -1;
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var message = body["message"].AsValue().GetValue<string>();
            var statusCode = body["statusCode"].AsValue().GetValue<int>();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, statusCode);
            Assert.NotNull(message);
        }

        [Fact]
        public async Task TC011_Create_Short_Url_When_Has_Qr_Code_Is_Null_Returns_BadRequest()
        {
            // arrange
            string url = "http://example.com";
            int expireMinutes = 60;
            bool? hasQrCode = null;

            // act
            var response = await CreateShortUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var message = body["message"].AsValue().GetValue<string>();
            var statusCode = body["statusCode"].AsValue().GetValue<int>();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, statusCode);
            Assert.NotNull(message);
        }

        [Fact]
        public async Task TC012_Create_Short_Url_When_Url_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            string url = "invalid-url";
            int expireMinutes = 60;
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(url, expireMinutes, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var message = body["message"].AsValue().GetValue<string>();
            var statusCode = body["statusCode"].AsValue().GetValue<int>();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, statusCode);
            Assert.NotNull(message);
        }
    }

    public class WebIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public WebIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task TC013_Get_Test_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/test");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(token);
        }
    }
}
