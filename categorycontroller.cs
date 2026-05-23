using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

using TaskManagement.Models;

namespace TaskManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class categorycontroller : ControllerBase
{
    private readonly string _connectionString = "Data Source=taskmanagement.db";

    [HttpGet]
    public ActionResult<List<Category>> GetCategories()
    {
        var categories = new List<Category>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("SELECT Id, Name FROM Categories", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            categories.Add(new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1)
            });
        }
        return Ok(categories);
    }

    [HttpPost]
    public IActionResult CreateCategory(Category category)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("INSERT INTO Categories (Name) VALUES (@name)", conn);
        cmd.Parameters.AddWithValue("@name", category.Name);

        cmd.ExecuteNonQuery();
        return Ok(new { message = "Category created successfully!" });
    }
}