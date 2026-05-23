namespace TaskManagement.Models
{
    
    public class Taskentities
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Priority { get; set; } = "Medium"; // Low, Medium, High
        public string DueDate { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";  // Pending, Completed
        public int? CategoryId { get; set; }             // Foreign Key linking to Category
        public int? UserId { get; set; }                 // Foreign Key linking to User (Assigned To)
    }
}

