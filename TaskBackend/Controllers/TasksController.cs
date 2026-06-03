using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TaskBackend.Models;
using System.Collections.Generic;
using System;

namespace TaskBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly string _connectionString;

        public TasksController()
        {
            
            var envUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrEmpty(envUrl))
            {
                
                envUrl = "postgresql://postgres:qhlzKLRzWbEXMjFFDtJygeIdwZrwegHi@caboose.proxy.rlwy.net:27018/railway";
            }

            
            var uri = new Uri(envUrl);
            var userInfo = uri.UserInfo.Split(':');
            _connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=True;";

            
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS tasks (
                            Id SERIAL PRIMARY KEY,
                            Title TEXT NOT NULL,
                            Description TEXT,
                            Priority TEXT,
                            Status TEXT DEFAULT 'todo'
                        );";
                    command.ExecuteNonQuery();
                }
            }
        }

        
        [HttpGet]
        public ActionResult<IEnumerable<TaskItem>> GetTasks()
        {
            var tasks = new List<TaskItem>();

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Id, Title, Description, Priority, Status FROM tasks";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new TaskItem
                            {
                                Id = reader.GetInt32(0),
                                Title = reader.IsDBNull(1) ? "" : reader.GetString(1),
                                Description = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                Priority = reader.IsDBNull(3) ? "Medium" : reader.GetString(3),
                                Status = reader.IsDBNull(4) ? "todo" : reader.GetString(4)
                            });
                        }
                    }
                }
            }
            return Ok(tasks);
        }

   
        [HttpPost]
        public ActionResult<TaskItem> CreateTask([FromBody] TaskItem task)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO tasks (Title, Description, Priority, Status) 
                        VALUES (@title, @desc, @priority, @status)
                        RETURNING Id;";

                    command.Parameters.AddWithValue("@title", task.Title);
                    command.Parameters.AddWithValue("@desc", task.Description ?? "");
                    command.Parameters.AddWithValue("@priority", task.Priority);
                    command.Parameters.AddWithValue("@status", task.Status);

                    int newId = (int)command.ExecuteScalar();
                    task.Id = newId;
                }
            }
            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
        }

        
        [HttpPut("{id}")]
        public IActionResult UpdateTaskStatus(int id, [FromBody] TaskItem taskUpdate)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE tasks SET Status = @status WHERE Id = @id";

                    command.Parameters.AddWithValue("@status", taskUpdate.Status);
                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0) return NotFound();
                }
            }
            return NoContent();
        }

       
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM tasks WHERE Id = @id";

                    command.Parameters.AddWithValue("@id", id);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0) return NotFound();
                }
            }
            return NoContent();
        }
    }
}