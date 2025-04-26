namespace SimpleSite.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public string? ConfirmationToken { get; set; }
        public bool IsConfirmed { get; set; }
        public int Status { get; set; }
        public Student? Student { get; set; }
        public List<StaffSubject> StaffSubjects { get; set; } = new List<StaffSubject>();
    }
}



