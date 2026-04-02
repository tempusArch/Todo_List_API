using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ToDoListAPI.Data;
using ToDoListAPI.models;
using ToDoListAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace ToDoListAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class TodoController : ControllerBase {
    private readonly ToDoListAPIDbContext _context;
    
    public TodoController(ToDoListAPIDbContext context) {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoModel>>> EGetOneUsersAllTodos(int page = 1, int limit = 10) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");
     
        var result = await _context.TodoTable
                .Where(n => n.UserId == userId)
                .OrderBy(n => n.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TodoModel>> EGetOneSpecificTodo(int id) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        var result = await _context.TodoTable.FindAsync(id);

        if (result == null)
            return NotFound();

        if (result.UserId != userId)
            return Forbid("User is not authorized to view this todo");

        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TodoModel>>> EGetOneUsersAllTodos([FromQuery] string query, int page = 1, int limit = 10) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        var result = await _context.TodoTable
                .Where(n => n.UserId == userId)           
                .Where(n => n.Title.Contains(query) || n.Description.Contains(query))
                .OrderBy(n => n.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
        
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TodoModel>> ECreateTodo(CreateTodoDto dto) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        var tdm = new TodoModel {
            Title = dto.Title,
            Description = dto.Description,
            UserId = userId
        };

        _context.TodoTable.Add(tdm);
        await _context.SaveChangesAsync();

        return Created(string.Empty, tdm);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TodoModel>> EUpdateTodo(int id, CreateTodoDto dto) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        var existingOne = await _context.TodoTable.FindAsync(id);

        if (existingOne == null)
            return NotFound();

        if (existingOne.UserId != userId)
            return Forbid("User is not authorized to update this todo");

        existingOne.Title = dto.Title;
        existingOne.Description = dto.Description;

        await _context.SaveChangesAsync();

        return Ok(existingOne);      
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<TodoModel>> EDeleteTodo(int id) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
            return Unauthorized("User is not authenticated");

        var existingOne = await _context.TodoTable.FindAsync(id);

        if (existingOne == null)
            return NotFound();

        if (existingOne.UserId != userId)
            return Forbid("User is not authorized to delete this todo");

        _context.TodoTable.Remove(existingOne);
        await _context.SaveChangesAsync();

        return NoContent();    
    }
}
