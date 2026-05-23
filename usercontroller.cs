using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using TaskManagement.Models;

namespace TaskManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Usercontroller : ControllerBase
{
    private readonly string _connectionString = "Data Source=taskmanagement.db";

    [HttpGet]
    public ActionResult<List<user>> GetUsers()
    {
        var users = new List<user>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("SELECT Id, Name, Email FROM Users", conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            users.Add(new user
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2)
            });
        }
        return Ok(users);
    }

    [HttpPost]
    public IActionResult CreateUser(user user)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();

        using var cmd = new SqliteCommand("INSERT INTO Users (Name, Email) VALUES (@name, @email)", conn);
        cmd.Parameters.AddWithValue("@name", user.Name);
        cmd.Parameters.AddWithValue("@email", user.Email);

        cmd.ExecuteNonQuery();
        return Ok(new { message = "User registered successfully!" });
    }
}