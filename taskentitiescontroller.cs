using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using TaskManagement.Models;

namespace TaskManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Taskcontroller : ControllerBase
{
    private readonly string _connectionString = "Data Source=taskmanagement.db";

    // 1. GET ALL TASKS
    [HttpGet]
    public ActionResult<List<Taskentities>> GetAllTasks()
    {
        var tasks = new List<Taskentities>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        string query = "SELECT Id, Title, Description, Priority, DueDate, Status, CategoryId, UserId FROM Tasks";
        using var cmd = new SqliteCommand(query, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            tasks.Add(new Taskentities
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                Priority = reader.GetString(3),
                DueDate = reader.GetString(4),
                Status = reader.GetString(5),
                CategoryId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                UserId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
            });
        }
        return Ok(tasks);
    }

    // 2. GET TASK BY ID
    [HttpGet("{id}")]
    public ActionResult<Taskentities> GetTaskById(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        string query = "SELECT Id, Title, Description, Priority, DueDate, Status, CategoryId, UserId FROM Tasks WHERE Id = @id";
        using var cmd = new SqliteCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return Ok(new Taskentities
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                Priority = reader.GetString(3),
                DueDate = reader.GetString(4),
                Status = reader.GetString(5),
                CategoryId = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                UserId = reader.IsDBNull(7) ? null : reader.GetInt32(7)
            });
        }
        return NotFound(new { message = "Task not found." });
    }

    // 3. CREATE TASK
    [HttpPost]
    public IActionResult CreateTask(Taskentities task)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        string query = @"INSERT INTO Tasks (Title, Description, Priority, DueDate, Status, CategoryId, UserId) 
                         VALUES (@title, @desc, @priority, @dueDate, @status, @catId, @userId)";

        using var cmd = new SqliteCommand(query, conn);
        cmd.Parameters.AddWithValue("@title", task.Title);
        cmd.Parameters.AddWithValue("@desc", task.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@priority", task.Priority);
        cmd.Parameters.AddWithValue("@dueDate", task.DueDate);
        cmd.Parameters.AddWithValue("@status", task.Status);
        cmd.Parameters.AddWithValue("@catId", task.CategoryId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@userId", task.UserId ?? (object)DBNull.Value);

        cmd.ExecuteNonQuery();
        return StatusCode(201, new { message = "Task added successfully!" });
    }

    // 4. UPDATE TASK
    [HttpPut("{id}")]
    public IActionResult UpdateTask(int id, Taskentities task)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        string query = @"UPDATE Tasks 
                         SET Title = @title, Description = @desc, Priority = @priority, 
                             DueDate = @dueDate, Status = @status, CategoryId = @catId, UserId = @userId 
                         WHERE Id = @id";

        using var cmd = new SqliteCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@title", task.Title);
        cmd.Parameters.AddWithValue("@desc", task.Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@priority", task.Priority);
        cmd.Parameters.AddWithValue("@dueDate", task.DueDate);
        cmd.Parameters.AddWithValue("@status", task.Status);
        cmd.Parameters.AddWithValue("@catId", task.CategoryId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@userId", task.UserId ?? (object)DBNull.Value);

        int rows = cmd.ExecuteNonQuery();
        if (rows == 0) return NotFound(new { message = "Task not found to update." });

        return NoContent();
    }

    // 5. DELETE TASK
    [HttpDelete("{id}")]
    public IActionResult DeleteTask(int id)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("DELETE FROM Tasks WHERE Id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        int rows = cmd.ExecuteNonQuery();
        if (rows == 0) return NotFound(new { message = "Task not found to delete." });

        return Ok(new { message = "Task deleted successfully!" });
    }
}