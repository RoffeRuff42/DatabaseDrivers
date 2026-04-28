using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Timers;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Tests
{
    public class TodoServiceTests
    {
        //Method to create a new in-memory database context for each test
        private static TodoDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<TodoDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TodoDbContext(options);
        }
       
        //Creates a TodoService instance and mocked dependencies
        private static TodoService CreateService(TodoDbContext context)
        {
            return new TodoService(context, new MemoryCache(new MemoryCacheOptions()));
        }
        //Tests that GetAllAsync returns only the todos that belong to the validated user
        [Fact]
        public async Task GetAllAsync_ReturnsOnlyTodosForValidatedUser()
        {
            //Creates context
            using var context = CreateContext();
            //Creates 3 todos, 2 for user 1 and 1 for user 2
            context.Todos.AddRange(
                new Todo { Id = 1, Title = "User 1 Todo A", IsDone = false, UserId = 1 },
                new Todo { Id = 2, Title = "User 1 Todo B", IsDone = false, UserId = 1 },
                new Todo { Id = 3, Title = "User 2 Todo", IsDone = false, UserId = 2 }
            );
            //Saves changes to the in-memory database
            await context.SaveChangesAsync();

            var service = CreateService(context);

            //Calls GetAllAsync with the valid ticket and checks that only the todos for user 1 are returned
            var result = await service.GetAllAsync(1, 10, null, 1);

            //Asserts that 2 todos are returned, all belong to user 1, and none have the title "User 2 Todo"
            Assert.Equal(2, result.Count);
            Assert.All(result, todo => Assert.Equal(1, todo.UserId));
            Assert.DoesNotContain(result, todo => todo.Title == "User 2 Todo");
        }

        [Fact]
        public async Task GetAllAsync_AppliesSearchAndPagination()
        {
            using var context = CreateContext();

            // Creates 3 todos for user 1, with different titles
            context.Todos.AddRange(
                new Todo { Id = 1, Title = "Buy milk", IsDone = false, UserId = 1 },
                new Todo { Id = 2, Title = "Buy bread", IsDone = false, UserId = 1 },
                new Todo { Id = 3, Title = "Wash car", IsDone = false, UserId = 1 }
            );
            await context.SaveChangesAsync();

            var service = CreateService(context);

            // Calls GetAllAsync with page 1, page size 1, and search term buy
            var result = await service.GetAllAsync(1, 1, "buy", 1);

            // Asserts that only 1 todo is returned and its title contains "Buy"
            Assert.Single(result);
            Assert.Contains("Buy", result[0].Title);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsTodo_WhenTodoExistsForUser()
        {
            using var context = CreateContext();

            //Adds a todo with id 5 for user 1
            context.Todos.Add(new Todo
            {
                Id = 5,
                Title = "Test Todo",
                IsDone = false,
                UserId = 1
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            //Calls GetByIdAsync with the id of the existing todo and checks that the correct todo is returned
            var result = await service.GetByIdAsync(5, 1);

            Assert.NotNull(result);
            Assert.Equal(5, result!.Id);
            Assert.Equal("Test Todo", result.Title);
            Assert.Equal(1, result.UserId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenTodoBelongsToAnotherUser()
        {
            using var context = CreateContext();

            //Adds a todo with id 10 for user 2
            context.Todos.Add(new Todo
            {
                Id = 10,
                Title = "Other users todo",
                IsDone = false,
                UserId = 2
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            //Calls GetByIdAsync with the id of the todo that belongs to another user and checks that null is returned
            var result = await service.GetByIdAsync(10, 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateTodoAsync_CreatesTodoWithIsDoneFalse()
        {
            using var context = CreateContext();

            var service = CreateService(context);

            //Creates a CreateTodoDto with the title "New Todo" and a valid ticket id
            var dto = new CreateTodoDto
            {
                Title = "New Todo"
            };

            var result = await service.CreateTodoAsync(dto, 1);

            Assert.NotNull(result);
            Assert.Equal("New Todo", result!.Title);
            Assert.False(result.IsDone);
            Assert.Equal(1, result.UserId);

            var savedTodo = await context.Todos.FirstOrDefaultAsync(t => t.Title == "New Todo");
            Assert.NotNull(savedTodo);
            Assert.False(savedTodo!.IsDone);
            Assert.Equal(1, savedTodo.UserId);

        }

        [Fact]
        public async Task UpdateTodoAsync_ReturnsTrue_AndUpdatesTodo_WhenOwnerMatches()
        {
            using var context = CreateContext();

            //Adds a todo with id 7 for user 1
            context.Todos.Add(new Todo
            {
                Id = 7,
                Title = "Old title",
                IsDone = false,
                UserId = 1
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            //Creates an UpdateTodoDto with the new title, isDone status, and a valid ticket id
            var dto = new UpdateTodoDto
            {
                Title = "Updated title",
                IsDone = true
            };

            //Calls UpdateTodoAsync with the id of the existing todo, the update DTO, and checks that the method returns true and the todo is updated in the database
            var result = await service.UpdateTodoAsync(7, dto, 1);

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

            //Adds a todo with id 9 for user 1
            context.Todos.Add(new Todo
            {
                Id = 9,
                Title = "Delete me",
                IsDone = false,
                UserId = 1
            });
            await context.SaveChangesAsync();

            var service = CreateService(context);

            //Calls DeleteTodoAsync with the id of the existing todo and a valid ticket id, and checks that the method returns true and the todo is removed from the database
            var result = await service.DeleteTodoAsync(9, 1);

            Assert.True(result);
            Assert.Null(await context.Todos.FindAsync(9));
        }
    }
}
