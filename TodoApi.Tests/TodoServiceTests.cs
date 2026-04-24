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

        [Fact]
        public async Task GetAllAsync_AppliesSearchAndPagination()
        {
            using var context = CreateContext();

            context.Todos.AddRange(
                new Todo { Id = 1, Title = "Buy milk", IsDone = false, UserId = 1 },
                new Todo { Id = 2, Title = "Buy bread", IsDone = false, UserId = 1 },
                new Todo { Id = 3, Title = "Wash car", IsDone = false, UserId = 1 }
            );
            await context.SaveChangesAsync();

            var userApiMock = new Mock<IUserApiClient>();
            var externalApiMock = new Mock<IExternalApiClient>();

            userApiMock
                .Setup(x => x.ValidateTicketAsync("ticket123"))
                .ReturnsAsync(CreateValidUserTicket(1, "ticket123"));

            var service = CreateService(context, userApiMock, externalApiMock);

            var result = await service.GetAllAsync(1, 1, "buy", "ticket123");

            Assert.Single(result);
            Assert.Contains("Buy", result[0].Title);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTodo_WhenTodoExistsForUser()
        {
            using var context = CreateContext();

            context.Todos.Add(new Todo
            {
                Id = 5,
                Title = "Test Todo",
                IsDone = false,
                UserId = 1
            });
            await context.SaveChangesAsync();

            var userApiMock = new Mock<IUserApiClient>();
            var externalApiMock = new Mock<IExternalApiClient>();

            userApiMock
                .Setup(x => x.ValidateTicketAsync("ticket123"))
                .ReturnsAsync(CreateValidUserTicket(1, "ticket123"));

            var service = CreateService(context, userApiMock, externalApiMock);

            var result = await service.GetByIdAsync(5, "ticket123");

            Assert.NotNull(result);
            Assert.Equal(5, result!.Id);
            Assert.Equal("Test Todo", result.Title);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenTodoBelongsToAnotherUser()
        {
            using var context = CreateContext();

            context.Todos.Add(new Todo
            {
                Id = 10,
                Title = "Other users todo",
                IsDone = false,
                UserId = 2
            });
            await context.SaveChangesAsync();

            var userApiMock = new Mock<IUserApiClient>();
            var externalApiMock = new Mock<IExternalApiClient>();

            userApiMock
                .Setup(x => x.ValidateTicketAsync("ticket123"))
                .ReturnsAsync(CreateValidUserTicket(1, "ticket123"));

            var service = CreateService(context, userApiMock, externalApiMock);

            var result = await service.GetByIdAsync(10, "ticket123");

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTodoAsync_CreatesTodoWithIsDoneFalse_AndCallsExternalApi()
        {
            using var context = CreateContext();

            var userApiMock = new Mock<IUserApiClient>();
            var externalApiMock = new Mock<IExternalApiClient>();

            userApiMock
                .Setup(x => x.ValidateTicketAsync("ticket123"))
                .ReturnsAsync(CreateValidUserTicket(1, "ticket123"));

            externalApiMock
                .Setup(x => x.GetTestDataAsync("random-quote"))
                .ReturnsAsync(new { message = "fake data" });

            var service = CreateService(context, userApiMock, externalApiMock);

            var dto = new CreateTodoDto
            {
                Title = "New Todo",
                TicketId = "ticket123"
            };

            var result = await service.CreateTodoAsync(dto, "ticket123");

            Assert.NotNull(result);
            Assert.Equal("New Todo", result!.Title);
            Assert.False(result.IsDone);
            Assert.Equal(1, result.UserId);

            var savedTodo = await context.Todos.FirstOrDefaultAsync(t => t.Title == "New Todo");
            Assert.NotNull(savedTodo);
            Assert.False(savedTodo!.IsDone);
            Assert.Equal(1, savedTodo.UserId);

            externalApiMock.Verify(x => x.GetTestDataAsync("random-quote"), Times.Once);
        }

        [Fact]
        public async Task UpdateTodoAsync_ReturnsTrue_AndUpdatesTodo_WhenOwnerMatches()
        {
            using var context = CreateContext();

            context.Todos.Add(new Todo
            {
                Id = 7,
                Title = "Old title",
                IsDone = false,
                UserId = 1
            });
            await context.SaveChangesAsync();

            var userApiMock = new Mock<IUserApiClient>();
            var externalApiMock = new Mock<IExternalApiClient>();

            userApiMock
                .Setup(x => x.ValidateTicketAsync("ticket123"))
                .ReturnsAsync(CreateValidUserTicket(1, "ticket123"));

            var service = CreateService(context, userApiMock, externalApiMock);

            var dto = new UpdateTodoDto
            {
                Title = "Updated title",
                IsDone = true,
                TicketId = "ticket123"
            };

            var result = await service.UpdateTodoAsync(7, dto, "ticket123");

            Assert.True(result);

            var updatedTodo = await context.Todos.FindAsync(7);
            Assert.NotNull(updatedTodo);
            Assert.Equal("Updated title", updatedTodo!.Title);
            Assert.True(updatedTodo.IsDone);
        }

        [Fact]
        public async Task DeleteTodoAsync_ReturnsTrue_AndRemovesTodo_WhenOwnerMatches()
        {
            using var context = CreateContext();

            context.Todos.Add(new Todo
            {
                Id = 9,
                Title = "Delete me",
                IsDone = false,
                UserId = 1
            });
            await context.SaveChangesAsync();

            var userApiMock = new Mock<IUserApiClient>();
            var externalApiMock = new Mock<IExternalApiClient>();

            userApiMock
                .Setup(x => x.ValidateTicketAsync("ticket123"))
                .ReturnsAsync(CreateValidUserTicket(1, "ticket123"));

            var service = CreateService(context, userApiMock, externalApiMock);

            var result = await service.DeleteTodoAsync(9, "ticket123");

            Assert.True(result);
            Assert.Null(await context.Todos.FindAsync(9));
        }

        [Fact]
        public async Task GetAllAsync_ThrowsUnauthorizedAccessException_WhenTicketIsInvalid()
        {
            using var context = CreateContext();

            var userApiMock = new Mock<IUserApiClient>();
            var externalApiMock = new Mock<IExternalApiClient>();

            userApiMock
                .Setup(x => x.ValidateTicketAsync("bad-ticket"))
                .ReturnsAsync((UserLoginDto?)null);

            var service = CreateService(context, userApiMock, externalApiMock);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                service.GetAllAsync(1, 10, null, "bad-ticket"));
        }
    }
}