namespace SimpleSite.Models
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public int Status { get; set; }
        public string? DepartmentName { get; set; }
        public decimal? CGPA { get; set; }
        public string? Subjects { get; set; } 
    }
}


