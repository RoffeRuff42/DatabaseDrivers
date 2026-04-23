using Microsoft.EntityFrameworkCore;
using Moq;
using TodoApi.Clients;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests
{
    public class TodoServiceTests
    {
        private static TodoDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TodoDbContext(options);
        }

        private static UserLoginDto CreateValidUserTicket(int userId = 1, string ticketId = "ticket123")
        {
            return new UserLoginDto
            {
                UserId = userId,
                Username = "Robin",
                TicketId = ticketId
            };
        }

        private static TodoService CreateService(
            TodoDbContext context,
            Mock<IUserApiClient> userApiMock,
            Mock<IExternalApiClient> externalApiMock)
        {
            return new TodoService(context, userApiMock.Object, externalApiMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyTodosForValidatedUser()
        {
            using var context = CreateContext();

            context.Todos.AddRange(
                new Todo { Id = 1, Title = "User 1 Todo A", IsDone = false, UserId = 1 },
                new Todo { Id = 2, Title = "User 1 Todo B", IsDone = false, UserId = 1 },
                new Todo { Id = 3, Title = "User 2 Todo", IsDone = false, UserId = 2 }
            );
            await context.SaveChangesAsync();

            var userApiMock = new Mock<IUserApiClient>();
            var externalApiMock = new Mock<IExternalApiClient>();

            userApiMock
                .Setup(x => x.ValidateTicketAsync("ticket123"))
                .ReturnsAsync(CreateValidUserTicket(1, "ticket123"));

            var service = CreateService(context, userApiMock, externalApiMock);

            var result = await service.GetAllAsync(1, 10, null, "ticket123");

            Assert.Equal(2, result.Count);
            Assert.All(result, todo => Assert.Equal(1, todo.UserId));
            Assert.DoesNotContain(result, todo => todo.Title == "User 2 Todo");
        }

        
    }
}