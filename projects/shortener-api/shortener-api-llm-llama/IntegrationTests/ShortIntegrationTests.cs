﻿// File: UrlShortenIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

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

        private async Task<HttpResponseMessage> GetUrlByToken(string token)
        {
            return await _client.GetAsync($"/{token}");
        }

        [Fact]
        public async Task TC001_Get_Url_By_Token_When_Token_Is_Empty_Returns_NotFound()
        {
            // act
            var response = await GetUrlByToken("");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Get_Url_By_Token_When_Token_Is_Null_Returns_NotFound()
        {
            // act
            var response = await GetUrlByToken(null);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Get_Url_By_Token_When_Token_Is_Invalid_Returns_NotFound()
        {
            // act
            var response = await GetUrlByToken("invalid");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Get_Url_By_Token_When_Token_Is_Valid_Returns_OK()
        {
            // arrange
            var requestBody = new
            {
                url = "http://example.com",
                hasQrCode = true
            };
            var response1 = await _client.PostAsJsonAsync("/api/url-shorts", requestBody);
            var body = await response1.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();

            // act
            var response2 = await GetUrlByToken(token);

            // assert
            body = await response2.Content.ReadFromJsonAsync<JsonObject>();
            var url = body["url"].AsValue().GetValue<string>();
            Assert.Equal(requestBody.url, url);
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        }

        private async Task<HttpResponseMessage> CreateShortUrlAsync(string url, bool? hasQrCode, int? expireMinutes)
        {
            var requestBody = new
            {
                url,
                hasQrCode,
                expireMinutes
            };

            return await _client.PostAsJsonAsync("/api/url-shorts", requestBody);
        }

        [Fact]
        public async Task TC005_Create_Speed_When_Url_Is_Empty_String_Returns_BadRequest()
        {
            // act
            var response = await CreateShortUrlAsync("", true, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Speed_When_Url_Is_Null_Returns_BadRequest()
        {
            // act
            var response = await CreateShortUrlAsync(null, true, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Speed_When_Url_Is_Too_Short_Returns_BadRequest()
        {
            // act
            var response = await CreateShortUrlAsync("x", true, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Speed_When_Url_Is_Invalid_Returns_BadRequest()
        {
            // act
            var response = await CreateShortUrlAsync("invalid-url", true, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Speed_When_HasQrCode_Is_Null_Returns_BadRequest()
        {
            // act
            var response = await CreateShortUrlAsync("http://example.com", null, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Speed_When_HasQrCode_Is_False_Returns_OK()
        {
            // act
            var response = await CreateShortUrlAsync("http://example.com", false, null);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            var shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            Assert.NotNull(token);
            Assert.NotNull(shortenedUrl);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Speed_When_HasQrCode_Is_True_Returns_OK()
        {
            // act
            var response = await CreateShortUrlAsync("http://example.com", true, null);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            var shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            var qrCode = body["qrCode"].AsValue().GetValue<string>();
            Assert.NotNull(token);
            Assert.NotNull(shortenedUrl);
            Assert.NotNull(qrCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_Speed_When_ExpireMinutes_Is_Null_Returns_OK()
        {
            // act
            var response = await CreateShortUrlAsync("http://example.com", true, null);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            var shortenedUrl = body["shortenedUrl"].AsValue().GetValue<string>();
            var qrCode = body["qrCode"].AsValue().GetValue<string>();
            Assert.NotNull(token);
            Assert.NotNull(shortenedUrl);
            Assert.NotNull(qrCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_Speed_When_ExpireMinutes_Is_Zero_Returns_BadRequest()
        {
            // act
            var response = await CreateShortUrlAsync("http://example.com", true, 0);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_Speed_When_ExpireMinutes_Is_Below_Minimum_Returns_BadRequest()
        {
            // act
            var response = await CreateShortUrlAsync("http://example.com", true, -1);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        private async Task<HttpResponseMessage> GetTest()
        {
            return await _client.GetAsync("/test");
        }

        [Fact]
        public async Task TC015_Get_Test_Returns_OK()
        {
            // act
            var response = await GetTest();

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            Assert.NotNull(token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
