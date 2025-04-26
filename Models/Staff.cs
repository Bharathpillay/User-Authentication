namespace SimpleSite.Models
{
    public class Staff
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        public User User { get; set; }
        public Department Department { get; set; }
        public List<StaffSubject> StaffSubjects { get; set; } 
    }
}