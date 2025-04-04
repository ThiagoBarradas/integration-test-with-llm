﻿// File: UrlShortenIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class UrlShortenIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UrlShortenIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateShortUrlAsync(string url, bool hasQrCode, int? expireMinutes = null)
        {
            var requestBody = new
            {
                url = url,
                expireMinutes = expireMinutes,
                hasQrCode = hasQrCode
            };

            return await _client.PostAsJsonAsync("/api/url-shorts", requestBody);
        }

        private async Task<string> CreateShortUrlTokenAsync(string url, bool hasQrCode, int? expireMinutes = null)
        {
            var response = await CreateShortUrlAsync(url, hasQrCode, expireMinutes);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].AsValue().GetValue<string>();
        }

        [Fact]
        public async Task TC001_Get_URL_By_Token_When_Valid_Token_Returns_OK()
        {
            // arrange
            string url = "http://example.com";
            bool hasQrCode = true;
            string token = await CreateShortUrlTokenAsync(url, hasQrCode); // precondition 1

            // act
            var response = await _client.GetAsync($"/{token}");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_url = body["url"].AsValue().GetValue<string>();
            Assert.Equal(url, body_url);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Get_URL_By_Token_When_Invalid_Token_Returns_NotFound()
        {
            // arrange
            string token = "invalidToken"; // invalid token

            // act
            var response = await _client.GetAsync($"/{token}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Get_URL_By_Token_When_Null_Token_Returns_NotFound()
        {
            // arrange
            string null_token = null; // null token

            // act
            var response = await _client.GetAsync($"/{null_token}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Get_URL_By_Token_When_Empty_Token_Returns_NotFound()
        {
            // arrange
            string empty_token = ""; // empty token

            // act
            var response = await _client.GetAsync($"/{empty_token}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Short_URL_When_Valid_Data_Returns_Created()
        {
            // arrange
            string url = "http://example.com";
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(url, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            var body_shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            var body_qrCode = body.ContainsKey("qrCode") ? body["qrCode"].AsValue().GetValue<string>() : null;
            Assert.True(!string.IsNullOrEmpty(body_token));
            Assert.True(!string.IsNullOrEmpty(body_shortenedUrl));
            Assert.True(hasQrCode ? !string.IsNullOrEmpty(body_qrCode) : body_qrCode == null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Short_URL_When_Invalid_URL_Returns_BadRequest()
        {
            // arrange
            string invalid_url = "invalidUrl";
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(invalid_url, hasQrCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Short_URL_When_URL_Is_Null_Returns_BadRequest()
        {
            // arrange
            string null_url = null;
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(null_url!, hasQrCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Short_URL_When_URL_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string empty_url = "";
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(empty_url, hasQrCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Short_URL_When_Expire_Minutes_Is_Negative_Returns_BadRequest()
        {
            // arrange
            string url = "http://example.com";
            bool hasQrCode = true;
            int? expire_minutes = -1;

            // act
            var response = await CreateShortUrlAsync(url, hasQrCode, expire_minutes);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Short_URL_When_Has_QR_Code_Is_Null_Returns_BadRequest()
        {
            // arrange
            string url = "http://example.com";
            bool? null_has_qrcode = null;

            // act
            var request = new
            {
                url = url,
                expireMinutes = (int?)null,
                hasQrCode = null_has_qrcode
            };
            var response = await _client.PostAsJsonAsync("/api/url-shorts", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Short_URL_When_Expire_Minutes_Is_Zero_Returns_Created()
        {
            // arrange
            string url = "http://example.com";
            bool hasQrCode = true;
            int? expire_minutes = 0;

            // act
            var response = await CreateShortUrlAsync(url, hasQrCode, expire_minutes);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            var body_shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            var body_qrCode = body.ContainsKey("qrCode") ? body["qrCode"].AsValue().GetValue<string>() : null;
            Assert.True(!string.IsNullOrEmpty(body_token));
            Assert.True(!string.IsNullOrEmpty(body_shortenedUrl));
            Assert.True(hasQrCode ? !string.IsNullOrEmpty(body_qrCode) : body_qrCode == null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_Short_URL_When_Expire_Minutes_Is_Positive_Returns_Created()
        {
            // arrange
            string url = "http://example.com";
            bool hasQrCode = true;
            int? expire_minutes = 30;

            // act
            var response = await CreateShortUrlAsync(url, hasQrCode, expire_minutes);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            var body_shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            var body_qrCode = body.ContainsKey("qrCode") ? body["qrCode"].AsValue().GetValue<string>() : null;
            Assert.True(!string.IsNullOrEmpty(body_token));
            Assert.True(!string.IsNullOrEmpty(body_shortenedUrl));
            Assert.True(hasQrCode ? !string.IsNullOrEmpty(body_qrCode) : body_qrCode == null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_Short_URL_When_Has_QR_Code_Is_False_Returns_Created_Without_QR_Code()
        {
            // arrange
            string url = "http://example.com";
            bool hasQrCode = false;

            // act
            var response = await CreateShortUrlAsync(url, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            var body_shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            var body_qrCode = body.ContainsKey("qrCode") ? body["qrCode"].AsValue().GetValue<string>() : null;
            Assert.True(!string.IsNullOrEmpty(body_token));
            Assert.True(!string.IsNullOrEmpty(body_shortenedUrl));
            Assert.True(body_qrCode == null);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_Short_URL_When_Has_QR_Code_Is_True_Returns_Created_With_QR_Code()
        {
            // arrange
            string url = "http://example.com";
            bool hasQrCode = true;

            // act
            var response = await CreateShortUrlAsync(url, hasQrCode);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            var body_shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            var body_qrCode = body.ContainsKey("qrCode") ? body["qrCode"].AsValue().GetValue<string>() : null;
            Assert.True(!string.IsNullOrEmpty(body_token));
            Assert.True(!string.IsNullOrEmpty(body_shortenedUrl));
            Assert.True(!string.IsNullOrEmpty(body_qrCode));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }

    public class TestIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TestIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task TC015_Get_Token_When_Valid_Request_Returns_OK()
        {
            // arrange

            // act
            var response = await _client.GetAsync("/test");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            Assert.True(!string.IsNullOrEmpty(body_token));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
