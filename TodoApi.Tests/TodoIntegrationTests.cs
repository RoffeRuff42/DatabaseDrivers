using System.Net;
using DatabaseDrivers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;


namespace TodoApi.Tests
{
    public class TodoIntegrationTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public TodoIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetTodos_ReturnsOk()
        {

            var response = await _client.GetAsync("/api/v1/todos");
            var content = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        [Fact]
        public async Task GetTodo_InvalidId_ReturnsNotFound()
        {
            var response = await _client.GetAsync("/api/v1/todos/999?ticketId=test");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}