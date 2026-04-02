using Microsoft.EntityFrameworkCore;
using ToDoListAPI.models;

namespace ToDoListAPI.Data;

public class ToDoListAPIDbContext : DbContext {
    public ToDoListAPIDbContext(DbContextOptions<ToDoListAPIDbContext> options) : base(options) {

    }
    public DbSet<TodoModel> TodoTable {get; set;}
    public DbSet<UserModel> UserTable {get; set;}
    public DbSet<RefreshTokenModel> RefreshTokenTable {get; set;}
    
}