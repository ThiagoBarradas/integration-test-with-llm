// File: RestaurantsIntegrationTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text.Json.Nodes;
using System;

namespace IntegrationTests
{
    public class RestaurantsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateRestaurantAsync(string name, string description, string category, bool hasDelivery, string contactEmail, string contactNumber, string city, string street, string postalCode)
        {
            var requestBody = new
            {
                name = name,
                description = description,
                category = category,
                hasDelivery = hasDelivery,
                contactEmail = contactEmail,
                contactNumber = contactNumber,
                city = city,
                street = street,
                postalCode = postalCode
            };

            return await _client.PostAsJsonAsync("/api/restaurants", requestBody);
        }

        [Fact]
        public async Task TC024_Create_Restaurant_Valid_Data()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Restaurant_Missing_Name()
        {
            // arrange
            string name = null;
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Create_Restaurant_Name_Too_Short()
        {
            // arrange
            string name = "Re";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Create_Restaurant_Name_Too_Long()
        {
            // arrange
            string name = "ThisRestaurantNameIsWayTooLongToMeetTheRequirementsOfTheTest";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Create_Restaurant_Missing_Description()
        {
            // arrange
            string name = "Restaurant Name";
            string description = null;
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Create_Restaurant_Description_Too_Short()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "De";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Create_Restaurant_Description_Too_Long()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "ThisRestaurantDescriptionIsWayTooLongToMeetTheRequirementsOfTheTest";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Create_Restaurant_Missing_Category()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = null;
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Create_Restaurant_Category_Too_Short()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "It";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Create_Restaurant_Category_Too_Long()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "ThisCategoryIsWayTooLongToMeetTheRequirementsOfTheTest";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Create_Restaurant_Missing_Contact_Email()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = null;
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Create_Restaurant_Invalid_Contact_Email()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "invalid-email";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Create_Restaurant_Missing_Contact_Number()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = null;
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Create_Restaurant_Contact_Number_Too_Short()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Create_Restaurant_Contact_Number_Too_Long()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "123456789012345";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Create_Restaurant_Invalid_Contact_Number_Characters()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890abc";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Create_Restaurant_Missing_City()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = null;
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Create_Restaurant_City_Too_Short()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "Ne";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Create_Restaurant_City_Too_Long()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "ThisCityNameIsWayTooLongToMeetTheRequirementsOfTheTest";
            string street = "Main Street";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Create_Restaurant_Missing_Street()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = null;
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Create_Restaurant_Street_Too_Short()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Ma";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Create_Restaurant_Street_Too_Long()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "ThisStreetNameIsWayTooLongToMeetTheRequirementsOfTheTest";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC046_Create_Restaurant_Missing_Postal_Code()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = null;

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Create_Restaurant_Postal_Code_Too_Short()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "12";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC048_Create_Restaurant_Postal_Code_Too_Long()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "1234567890123456";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC049_Create_Restaurant_Invalid_Postal_Code_Characters()
        {
            // arrange
            string name = "Restaurant Name";
            string description = "Restaurant Description";
            string category = "Italian";
            bool hasDelivery = true;
            string contactEmail = "test@example.com";
            string contactNumber = "1234567890";
            string city = "New York";
            string street = "Main Street";
            string postalCode = "10001abc";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}