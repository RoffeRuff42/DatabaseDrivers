using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data
{
    public class TodoDbContext : DbContext
    {
        // Constructor that passes settings to the base Entity Framework class
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }

        // This property represents the table in our database
        public DbSet<Todo> Todos { get; set; }
    }
}
