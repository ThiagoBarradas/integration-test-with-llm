﻿using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class UrlIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UrlIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> CreateShortUrl(string url, int? expireMinutes, bool hasQrCode)
        {
            var requestBody = new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            };

            var response = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            return body["token"].ToString();
        }

        [Fact]
        public async Task TC001_Get_Url_When_Valid_Token_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            int? expireMinutes = null;
            bool hasQrCode = true;

            var validToken = await CreateShortUrl(url, expireMinutes, hasQrCode);

            // act
            var response = await _client.GetAsync($"/{validToken}");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var retrievedUrl = body["url"].ToString();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(url, retrievedUrl);
        }

        [Fact]
        public async Task TC002_Get_Url_When_Invalid_Token_Returns_NotFound()
        {
            // arrange
            string invalidToken = "invalidToken123";

            // act
            var response = await _client.GetAsync($"/{invalidToken}");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(404, body["statusCode"].GetValue<int>());
        }

        [Fact]
        public async Task TC003_Create_Short_Url_When_Valid_Data_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            int? expireMinutes = null;
            bool hasQrCode = true;

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            });

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].ToString();
            var shortenedUrl = body["shortenedUrl"].ToString();
            var qrCode = body["qrCode"].ToString();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.False(string.IsNullOrWhiteSpace(shortenedUrl));
            Assert.False(string.IsNullOrWhiteSpace(qrCode));
        }

        [Fact]
        public async Task TC004_Create_Short_Url_When_Invalid_URL_Returns_BadRequest()
        {
            // arrange
            string invalidUrl = "invalid-url";
            int? expireMinutes = null;
            bool hasQrCode = false;

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new
            {
                url = invalidUrl,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            });

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, body["statusCode"].GetValue<int>());
        }

        [Fact]
        public async Task TC005_Create_Short_Url_When_Null_URL_Returns_BadRequest()
        {
            // arrange
            string nullUrl = null;
            int? expireMinutes = null;
            bool hasQrCode = true;

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new
            {
                url = nullUrl,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            });

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, body["statusCode"].GetValue<int>());
        }

        [Fact]
        public async Task TC006_Create_Short_Url_When_Empty_URL_Returns_BadRequest()
        {
            // arrange
            string emptyUrl = "";
            int? expireMinutes = null;
            bool hasQrCode = true;

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new
            {
                url = emptyUrl,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            });

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(400, body["statusCode"].GetValue<int>());
        }

        [Fact]
        public async Task TC007_Create_Short_Url_When_ExpireMinutes_Null_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            int? expireMinutes = null;
            bool hasQrCode = false;

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            });

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].ToString();
            var shortenedUrl = body["shortenedUrl"].ToString();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.False(string.IsNullOrWhiteSpace(shortenedUrl));
            Assert.Null(body["qrCode"]);
        }

        [Fact]
        public async Task TC008_Create_Short_Url_When_ExpireMinutes_Minimum_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            int? expireMinutes = 1;
            bool hasQrCode = true;

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            });

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].ToString();
            var shortenedUrl = body["shortenedUrl"].ToString();
            var qrCode = body["qrCode"].ToString();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.False(string.IsNullOrWhiteSpace(shortenedUrl));
            Assert.False(string.IsNullOrWhiteSpace(qrCode));
        }

        [Fact]
        public async Task TC009_Create_Short_Url_When_QRCode_Disabled_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            int? expireMinutes = 10;
            bool hasQrCode = false;

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            });

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].ToString();
            var shortenedUrl = body["shortenedUrl"].ToString();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.False(string.IsNullOrWhiteSpace(shortenedUrl));
            Assert.Null(body["qrCode"]);
        }

        [Fact]
        public async Task TC010_Create_Short_Url_When_QRCode_Enabled_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            int? expireMinutes = 10;
            bool hasQrCode = true;

            // act
            var response = await _client.PostAsJsonAsync("/api/url-shorts", new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            });

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].ToString();
            var shortenedUrl = body["shortenedUrl"].ToString();
            var qrCode = body["qrCode"].ToString();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.False(string.IsNullOrWhiteSpace(shortenedUrl));
            Assert.False(string.IsNullOrWhiteSpace(qrCode));
        }

        [Fact]
        public async Task TC011_Test_Endpoint_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/test");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].ToString();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(token == null || token.Length > 0);
        }
    }
}